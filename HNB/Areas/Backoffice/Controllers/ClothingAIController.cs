using Microsoft.AspNetCore.Mvc;
using HNB.Areas.Backoffice.Controllers;
using HNB.Areas.Backoffice.Dtos;
using HNB.IntelligentSystems.ObjectDetection.Utils;
using System.IO;

namespace HNB.Areas.Backoffice.Controllers;

/// <summary>
/// 簡介：服裝 AI 控制器，處理衣櫃管理和 AI 提取功能
/// </summary>
public class ClothingAIController : BaseController
{
    private const string StorageRoot = "Areas/Backoffice/storage/DallE3";
    private readonly IWebHostEnvironment _env;

    public ClothingAIController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// 簡介：顯示衣櫃頁面
    /// </summary>
    public IActionResult Wardrobe()
    {
        var wardrobeItems = LoadWardrobeItems();
        return View(wardrobeItems);
    }

    /// <summary>
    /// 簡介：顯示助手頁面
    /// </summary>
    public IActionResult Assistant()
        => View();

    /// <summary>
    /// 簡介：上傳服裝檔案
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitUpload(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return Json(new UploadClothingResponse { Success = false, Error = "請提供檔案" });

        var uploadedFiles = new List<UploadedClothingInfo>();
        var dateStr = DateTime.Now.ToString("yyyy_MM_dd");
        var savePath = Path.Combine(_env.ContentRootPath, StorageRoot, dateStr);
        Directory.CreateDirectory(savePath);

        foreach (var file in files)
        {
            if (file == null || file.Length == 0)
                continue;

            var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(savePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            uploadedFiles.Add(new UploadedClothingInfo
            {
                FileName = fileName,
                Label = "服裝",
                Confidence = 1.0,
                Path = $"/storage/DallE3/{dateStr}/{fileName}"
            });
        }

        return Json(new UploadClothingResponse
        {
            Success = true,
            UploadedFiles = uploadedFiles,
            Count = uploadedFiles.Count
        });
    }

    /// <summary>
    /// 簡介：刪除服裝檔案
    /// </summary>
    [HttpDelete]
    public IActionResult Delete(string fileName, string category)
    {
        if (string.IsNullOrEmpty(fileName))
            return Json(new DeleteClothingResponse { Success = false, Error = "檔案名稱不能為空" });

        var storagePath = Path.Combine(_env.ContentRootPath, StorageRoot);
        if (!Directory.Exists(storagePath))
            return Json(new DeleteClothingResponse { Success = false, Error = "儲存路徑不存在" });

        var dateDirs = Directory.GetDirectories(storagePath);
        foreach (var dateDir in dateDirs)
        {
            var filePath = Path.Combine(dateDir, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return Json(new DeleteClothingResponse { Success = true });
            }
        }

        return Json(new DeleteClothingResponse { Success = false, Error = "檔案不存在" });
    }

    /// <summary>
    /// 簡介：批量刪除服裝檔案
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BatchDelete([FromBody] BatchDeleteRequest request)
    {
        if (request == null || request.Items == null || request.Items.Count == 0)
            return Json(new BatchDeleteResponse { Success = false, Error = "請提供要刪除的項目" });

        var storagePath = Path.Combine(_env.ContentRootPath, StorageRoot);
        if (!Directory.Exists(storagePath))
            return Json(new BatchDeleteResponse { Success = false, Error = "儲存路徑不存在" });

        var results = new List<BatchDeleteItemResult>();
        var dateDirs = Directory.GetDirectories(storagePath);

        foreach (var item in request.Items)
        {
            var deleted = false;
            foreach (var dateDir in dateDirs)
            {
                var filePath = Path.Combine(dateDir, item.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    deleted = true;
                    break;
                }
            }

            results.Add(new BatchDeleteItemResult
            {
                FileName = item.FileName,
                Success = deleted
            });
        }

        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count - successCount;

        return Json(new BatchDeleteResponse
        {
            Success = failureCount == 0,
            Results = results,
            SuccessCount = successCount,
            FailureCount = failureCount
        });
    }

    /// <summary>
    /// 簡介：載入衣櫃項目列表
    /// </summary>
    private List<WardrobeItem> LoadWardrobeItems()
    {
        var items = new List<WardrobeItem>();
        var storagePath = Path.Combine(_env.ContentRootPath, StorageRoot);

        if (!Directory.Exists(storagePath))
            return items;

        var dateDirs = Directory.GetDirectories(storagePath);
        foreach (var dateDir in dateDirs)
        {
            var dateStr = Path.GetFileName(dateDir);
            var files = Directory.GetFiles(dateDir, "*.jpg").Concat(Directory.GetFiles(dateDir, "*.png")).ToList();

            foreach (var filePath in files)
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = fileInfo.Name;
                var category = ExtractCategoryFromFileName(fileName);

                items.Add(new WardrobeItem
                {
                    FileName = fileName,
                    Category = category,
                    WebPath = $"/storage/DallE3/{dateStr}/{fileName}",
                    FileSize = fileInfo.Length,
                    UploadDate = fileInfo.CreationTime,
                    ModifiedDate = fileInfo.LastWriteTime
                });
            }
        }

        return items.OrderByDescending(i => i.UploadDate).ToList();
    }

    /// <summary>
    /// 簡介：從檔案名稱提取類別（DALL E3 生成的圖片預設為"服裝"）
    /// </summary>
    private string ExtractCategoryFromFileName(string fileName)
    {
        return "服裝";
    }
}
