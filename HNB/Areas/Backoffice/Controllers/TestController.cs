using HNB.Areas.Backoffice.Utilities;
using Microsoft.AspNetCore.Mvc;
using static HNB.Areas.Backoffice.Utilities.DirectoryManagerUtilities;

namespace HNB.Areas.Backoffice.Controllers;

/// <summary>
/// 測試控制器
/// </summary>
[Area("Backoffice")]
public class TestController : Controller
{
    /// <summary>
    /// DropZone 測試頁面
    /// </summary>
    public IActionResult Index()
    {
        var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Areas", "Backoffice", "storage");
        
        // 確保 storage 目錄存在
        if (!Directory.Exists(storagePath))
        {
            Directory.CreateDirectory(storagePath);
        }
        
        // 獲取檔案列表
        var files = new List<dynamic>();
        if (Directory.Exists(storagePath))
        {
            var fileInfos = new DirectoryInfo(storagePath).GetFiles();
            foreach (var file in fileInfos)
            {
                files.Add(new
                {
                    FileName = file.Name,
                    Size = file.Length,
                    CreatedAt = file.CreationTime,
                    Owners = new string[0] // 暫時設為空陣列
                });
            }
        }
        
        ViewBag.StoragePath = storagePath;
        ViewBag.Files = files;
        
        return View();
    }

    /// <summary>
    /// 上傳檔案
    /// </summary>
    public IActionResult UploadFile(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "請選擇檔案" });
            }

            var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Areas", "Backoffice", "storage");
            
            // 確保目錄存在
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            var filePath = Path.Combine(storagePath, file.FileName);
            
            // 如果檔案已存在，添加時間戳
            if (System.IO.File.Exists(filePath))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                var ext = Path.GetExtension(file.FileName);
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                filePath = Path.Combine(storagePath, $"{nameWithoutExt}_{timestamp}{ext}");
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Json(new { success = true, message = "檔案上傳成功", filePath = filePath });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    public IActionResult DeleteFile(string fileName)
    {
        try
        {
            var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Areas", "Backoffice", "storage");
            var filePath = Path.Combine(storagePath, fileName);
            
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { success = false, message = "檔案不存在" });
            }
            
            System.IO.File.Delete(filePath);
            return Json(new { success = true, message = "檔案刪除成功" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 獲取檔案（用於預覽和下載）
    /// </summary>
    public IActionResult GetFile(string fileName)
    {
        try
        {
            var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Areas", "Backoffice", "storage");
            var filePath = Path.Combine(storagePath, fileName);
            
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("檔案不存在");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = GetContentType(fileName);
            return File(fileBytes, contentType);
        }
        catch (Exception ex)
        {
            return BadRequest($"無法讀取檔案: {ex.Message}");
        }
    }

    /// <summary>
    /// 獲取檔案內容（用於預覽）
    /// </summary>
    public IActionResult GetFileContent(string fileName, string type = "text")
    {
        try
        {
            var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Areas", "Backoffice", "storage");
            var filePath = Path.Combine(storagePath, fileName);
            
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { success = false, message = "檔案不存在" });
            }

            // 根據類型返回不同的內容
            if (type == "image" || type == "pdf" || type == "video" || type == "audio")
            {
                // 直接返回檔案流
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(fileName);
                return File(fileBytes, contentType);
            }
            else if (type == "text")
            {
                // 文字檔案返回 JSON
                var content = System.IO.File.ReadAllText(filePath);
                return Json(new { success = true, content = content });
            }
            else if (type == "info")
            {
                // 返回檔案資訊
                var fileInfo = new FileInfo(filePath);
                return Json(new { success = true, size = fileInfo.Length, name = fileInfo.Name });
            }
            else if (type == "download")
            {
                // 下載檔案
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(fileName);
                return File(fileBytes, contentType, fileName);
            }
            else
            {
                // 預設返回檔案流
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(fileName);
                return File(fileBytes, contentType);
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 儲存檔案內容
    /// </summary>
    [HttpPost]
    public IActionResult SaveFileContent(string fileName, string content)
    {
        try
        {
            var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Areas", "Backoffice", "storage");
            var filePath = Path.Combine(storagePath, fileName);
            
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { success = false, message = "檔案不存在" });
            }
            
            System.IO.File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
            return Json(new { success = true, message = "檔案儲存成功" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 根據檔案副檔名獲取 MIME 類型
    /// </summary>
    private string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLower();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".ogg" => "video/ogg",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".aac" => "audio/aac",
            ".flac" => "audio/flac",
            _ => "application/octet-stream"
        };
    }

}

