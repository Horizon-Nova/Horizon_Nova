using HNB.Areas.Backoffice.Dtos;
using HNB.Areas.Backoffice.Services;
using HNB.IntelligentSystems.ObjectDetection.Models;
using HNB.IntelligentSystems.ObjectDetection.Utils;
using OpenCvSharp;

namespace HNB.Areas.Backoffice.Services;

public class ClothingAIService(IWebHostEnvironment env, ObjectDetectionService detectionService, DallE3Service dallE3Service)
{
    private const string StorageRoot = "Areas/Backoffice/storage/ObjectDetection";
    
    #region 統一的查詢方法
    public List<WardrobeItem> LoadWardrobeList()
    {
        var items = new List<WardrobeItem>();
        var storagePath = Path.Combine(env.ContentRootPath, StorageRoot);
        var productPath = Path.Combine(storagePath, "商品圖");

        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        
        // 掃描商品圖目錄
        if (Directory.Exists(productPath))
        {
            var productDateDirs = Directory.GetDirectories(productPath);
            foreach (var productDateDir in productDateDirs)
            {
                var files = Directory.GetFiles(productDateDir)
                    .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .ToList();

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var fileName = Path.GetFileName(file);
                    var dateFolder = Path.GetFileName(productDateDir);
                    
                    // 從 product_{guid}_{類別}_{序號}.png 提取類別
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    var category = ExtractCategoryFromFileName(fileNameWithoutExt);
                    var originalFileName = fileNameWithoutExt.StartsWith("product_") 
                        ? fileNameWithoutExt.Substring("product_".Length) + Path.GetExtension(file)
                        : fileName;
                    
                    items.Add(new WardrobeItem
                    {
                        FileName = originalFileName,
                        Category = category,
                        WebPath = $"/storage/ObjectDetection/商品圖/{dateFolder}/{fileName}",
                        FileSize = fileInfo.Length,
                        UploadDate = fileInfo.CreationTime,
                        ModifiedDate = fileInfo.LastWriteTime
                    });
                }
            }
        }

        return items.OrderByDescending(i => i.UploadDate).ToList();
    }

    private string ExtractCategoryFromFileName(string fileNameWithoutExt)
    {
        if (!fileNameWithoutExt.StartsWith("product_"))
            return "unknown";
        
        var parts = fileNameWithoutExt.Substring("product_".Length).Split('_');
        if (parts.Length >= 2)
        {
            return parts[1];
        }
        return "unknown";
    }
    #endregion

    #region 上傳與處理
    public async Task<(bool success, List<UploadedClothingInfo> uploadedFiles, string? error)> UploadAndProcessClothing(
        List<Microsoft.AspNetCore.Http.IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return (false, new List<UploadedClothingInfo>(), "請選擇檔案");

        var dateStr = DateTime.Now.ToString("yyyy_MM_dd");
        var storagePath = Path.Combine(env.ContentRootPath, StorageRoot);
        var originalPath = Path.Combine(storagePath, "原圖", dateStr);
        var detectedPath = Path.Combine(storagePath, "辨識", dateStr);
        var productPath = Path.Combine(storagePath, "商品圖", dateStr);
        
        Directory.CreateDirectory(originalPath);
        Directory.CreateDirectory(detectedPath);
        Directory.CreateDirectory(productPath);

        var uploadedFiles = new List<UploadedClothingInfo>();
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

        foreach (var file in files)
        {
            if (file.Length == 0)
                continue;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!imageExtensions.Contains(extension))
                continue;

            var (success, infos) = await ProcessSingleImage(file, originalPath, detectedPath, productPath, dateStr, extension);
            if (success)
            {
                uploadedFiles.AddRange(infos);
            }
        }

        if (uploadedFiles.Count == 0)
            return (false, new List<UploadedClothingInfo>(), "沒有有效的圖片檔案或辨識結果");

        return (true, uploadedFiles, null);
    }

    private async Task<(bool success, List<UploadedClothingInfo> infos)> ProcessSingleImage(
        Microsoft.AspNetCore.Http.IFormFile file, string originalPath, string detectedPath, string productPath,
        string dateStr, string extension)
    {
        using var imageBytes = new MemoryStream();
        await file.CopyToAsync(imageBytes);
        byte[] imgData = imageBytes.ToArray();
        
        var guid = Guid.NewGuid().ToString("N");
        var originalFileName = $"{guid}_{file.FileName}";
        var originalFilePath = Path.Combine(originalPath, originalFileName);
        
        await System.IO.File.WriteAllBytesAsync(originalFilePath, imgData);

        var detections = detectionService.DetectObjectsFromBytes(imgData);
        
        using var originalImage = Cv2.ImDecode(imgData, ImreadModes.Color);
        var processedFiles = new List<UploadedClothingInfo>();

        foreach (var detection in detections)
        {
            var cleanLabel = CleanLabel(detection.Label);
            var labelDir = Path.Combine(detectedPath, cleanLabel);
            Directory.CreateDirectory(labelDir);

            var cropped = ImageUtils.CropBox(originalImage, detection.Box);
            // 簡化檔名：去掉座標，只保留必要的識別資訊
            var croppedFileName = $"{guid}_{cleanLabel}_{processedFiles.Count}{extension}";
            var croppedFilePath = Path.Combine(labelDir, croppedFileName);
            
            Cv2.ImWrite(croppedFilePath, cropped);
            cropped.Dispose();

            // 自動生成商品圖：傳入辨識完成的檔名，讓系統去查找
            var productFileName = $"product_{croppedFileName}";
            var (productSuccess, productError) = await GenerateProductImageFromDetectedFile(
                croppedFileName, cleanLabel, productPath, productFileName);

            processedFiles.Add(new UploadedClothingInfo
            {
                FileName = croppedFileName,
                Label = cleanLabel,
                Confidence = Math.Round(detection.Score * 100, 2),
                Path = productSuccess 
                    ? $"/storage/ObjectDetection/商品圖/{dateStr}/{productFileName}"
                    : $"/storage/ObjectDetection/辨識/{dateStr}/{cleanLabel}/{croppedFileName}",
                Error = productError
            });
        }

        originalImage.Dispose();

        return (true, processedFiles);
    }

    private async Task<(bool success, string? error)> GenerateProductImageFromDetectedFile(
        string detectedFileName, string category, string productPath, string productFileName)
    {
        var prompt = BuildProductImagePrompt(category);
        
        var (success, localPath, error) = await dallE3Service.GenerateAndSaveProductImage(
            prompt, productPath, productFileName, CancellationToken.None);
        
        return (success, error);
    }

    /// <summary>
    /// 建構商品圖生成提示詞
    /// 將辨識出的服裝轉換為商品展示圖：純白背景、整潔擺放、無陰影、無人物模型
    /// </summary>
    private string BuildProductImagePrompt(string category)
    {
        // 根據類別調整描述
        var categorySpecific = category.ToLower() switch
        {
            "jacket" or "coat" => "外套",
            "shirt" or "top" or "clothes" => "上衣",
            "pants" or "trouser" => "褲子",
            "shoes" or "shoe" => "鞋子",
            _ => category
        };

        return $"將圖片轉換為{categorySpecific}商品展示圖，" +
               $"保留衣服細節與材質、真實，" +
               $"排除掉衣服上的配飾，" +
               $"移除背景、人物、人體模型，" +
               $"換成完全純白的背景（RGB 255,255,255），" +
               $"服裝整潔擺放在平面上，" +
               $"乾淨、專業、完全無陰影、無任何光線效果";
    }

    private string CleanLabel(string label)
    {
        if (label.Contains("(") && label.Contains(")"))
        {
            int bracketIndex = label.IndexOf('(');
            return label.Substring(0, bracketIndex);
        }
        return label;
    }
    #endregion

    #region DALL-E 商品圖生成（手動重新生成用）
    /// <summary>
    /// 根據辨識檔名重新生成商品圖（避免掃描整個資料夾）
    /// </summary>
    /// <param name="detectedFileNames">辨識完成的檔名列表，格式：{日期}/{類別}/{檔名}</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool success, List<ProductImageInfo>? results, string? error)> GenerateProductImagesByFileNames(
        List<string> detectedFileNames,
        CancellationToken cancellationToken = default)
    {
        if (detectedFileNames == null || detectedFileNames.Count == 0)
            return (false, null, "請提供辨識檔名");

        var results = new List<ProductImageInfo>();
        var storagePath = Path.Combine(env.ContentRootPath, StorageRoot);
        var detectedPath = Path.Combine(storagePath, "辨識");

        foreach (var fileNamePath in detectedFileNames)
        {
            // 解析路徑：{日期}/{類別}/{檔名}
            var parts = fileNamePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                results.Add(new ProductImageInfo
                {
                    SourcePath = fileNamePath,
                    Success = false,
                    Error = "路徑格式錯誤，應為：日期/類別/檔名"
                });
                continue;
            }

            var dateStr = parts[0];
            var category = parts[1];
            var detectedFileName = parts[2];
            
            // 根據檔名直接查找檔案，不掃描整個資料夾
            var detectedFilePath = Path.Combine(detectedPath, dateStr, category, detectedFileName);
            
            if (!System.IO.File.Exists(detectedFilePath))
            {
                results.Add(new ProductImageInfo
                {
                    SourcePath = fileNamePath,
                    Success = false,
                    Error = "檔案不存在"
                });
                continue;
            }

            var extension = Path.GetExtension(detectedFileName);
            var productPath = Path.Combine(storagePath, "商品圖", dateStr);
            Directory.CreateDirectory(productPath);
            
            var productFileName = $"product_{detectedFileName}";
            var prompt = BuildProductImagePrompt(category);

            var (success, localPath, error) = await dallE3Service.GenerateAndSaveProductImage(
                prompt, 
                productPath, 
                productFileName, 
                cancellationToken);

            results.Add(new ProductImageInfo
            {
                SourcePath = fileNamePath,
                ProductPath = success ? $"/storage/ObjectDetection/商品圖/{dateStr}/{productFileName}" : null,
                Success = success,
                Error = error,
                Category = category
            });
        }

        return (true, results, null);
    }
    #endregion

    #region 基本 CRUD 操作
    public (bool success, string? error) DeleteClothing(string fileName, string category)
    {
        if (string.IsNullOrEmpty(fileName))
            return (false, "檔案名稱不能為空");

        var storagePath = Path.Combine(env.ContentRootPath, StorageRoot);
        var productPath = Path.Combine(storagePath, "商品圖");

        if (!Directory.Exists(productPath))
            return (false, "檔案不存在");

        var dateDirs = Directory.GetDirectories(productPath);
        string? filePath = null;

        // 從 fileName 提取 product_ 前綴的實際檔名
        var actualFileName = fileName.StartsWith("product_") ? fileName : $"product_{fileName}";

        foreach (var dateDir in dateDirs)
        {
            var potentialFile = Path.Combine(dateDir, actualFileName);
            if (System.IO.File.Exists(potentialFile))
            {
                filePath = potentialFile;
                break;
            }
        }

        if (filePath == null || !System.IO.File.Exists(filePath))
            return (false, "檔案不存在");

        System.IO.File.Delete(filePath);
        return (true, null);
    }
    #endregion
}

