using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

#region 請求與響應模型
public class CreateItemRequest
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class DeleteItemRequest
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class RenameItemRequest
{
    public string Path { get; set; } = string.Empty;
    public string OldName { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
}

public class SaveTextFileRequest
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Encoding { get; set; } = "utf-8";
}

public class FileManagerResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ReadTextFileResponse
{
    public bool Success { get; set; }
    public string? Content { get; set; }
    public string? Encoding { get; set; }
    public DateTime? LastModified { get; set; }
    public string? Message { get; set; }
}

public class UploadResponse
{
    public bool Success { get; set; }
    public int Saved { get; set; }
    public int Failed { get; set; }
    public List<string>? Errors { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class LoadDirectoryResponse
{
    public bool Success { get; set; }
    public object? Folders { get; set; }
    public object? Files { get; set; }
    public object? Breadcrumb { get; set; }
    public object? Stats { get; set; }
    public string? Message { get; set; }
}

public class LoadTreeResponse
{
    public bool Success { get; set; }
    public object? Tree { get; set; }
    public string? Message { get; set; }
}
#endregion

[Area("Backoffice")]
public class FileManagerController(FileManagerServices svc) : BaseController
{
    #region 主頁面
    /// <summary>
    /// 檔案管理主頁面
    /// </summary>
    public IActionResult FileManager(string? parentCode = null)
    {
        var username = User.Identity?.Name;
        svc.ViewBagModel(ViewBag, parentCode, username);
        return View();
    }
    #endregion

    #region 資料夾操作 API
    /// <summary>
    /// 建立資料夾
    /// </summary>
    [HttpPost]
    public IActionResult CreateFolder([FromBody] CreateItemRequest request)
    {
        var result = svc.CreateFolder(request.Path, request.Name);
        return Json(new FileManagerResponse { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    [HttpPost]
    public IActionResult DeleteFolder([FromBody] DeleteItemRequest request)
    {
        var result = svc.DeleteFolder(request.Path, request.Name);
        return Json(new FileManagerResponse { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    [HttpPost]
    public IActionResult RenameFolder([FromBody] RenameItemRequest request)
    {
        var result = svc.RenameFolder(request.Path, request.OldName, request.NewName);
        return Json(new FileManagerResponse { Success = result.success, Message = result.message });
    }
    #endregion

    #region 檔案操作 API
    /// <summary>
    /// 建立檔案
    /// </summary>
    [HttpPost]
    public IActionResult CreateFile([FromBody] CreateItemRequest request)
    {
        var result = svc.CreateFile(request.Path, request.Name);
        return Json(new FileManagerResponse { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    [HttpPost]
    public IActionResult DeleteFile([FromBody] DeleteItemRequest request)
    {
        var result = svc.DeleteFile(request.Path, request.Name);
        return Json(new FileManagerResponse { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    [HttpPost]
    public IActionResult RenameFile([FromBody] RenameItemRequest request)
    {
        var result = svc.RenameFile(request.Path, request.OldName, request.NewName);
        return Json(new FileManagerResponse { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 讀取文字檔案
    /// </summary>
    [HttpGet]
    public IActionResult ReadTextFile(string path, string name)
    {
        try
        {
            var (content, encoding, lastModified) = svc.LoadTextFile(path, name);
            return Json(new ReadTextFileResponse 
            { 
                Success = true, 
                Content = content, 
                Encoding = encoding, 
                LastModified = lastModified 
            });
        }
        catch (Exception ex)
        {
            return Json(new ReadTextFileResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    [HttpPost]
    public IActionResult SaveTextFile([FromBody] SaveTextFileRequest request)
    {
        var result = svc.SaveTextFile(request.Path, request.Name, request.Content, request.Encoding);
        return Json(new FileManagerResponse { Success = result.success, Message = result.message });
    }
    #endregion

    #region 檔案上傳
    /// <summary>
    /// 上傳檔案（最大 4GB）
    /// </summary>
    [HttpPost]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = 4294967296)] // 4GB = 4 * 1024 * 1024 * 1024
    [RequestSizeLimit(4294967296)] // 4GB
    public IActionResult Upload(string path, List<IFormFile> files, List<string>? keys)
    {
        if (files == null || files.Count == 0)
            return Json(new FileManagerResponse { Success = false, Message = "未選擇檔案" });

        var username = User.Identity?.Name ?? "anonymous";
        var result = svc.UploadBatchFiles(path, files, keys, username);
        
        return Json(new UploadResponse
        {
            Success = result.success,
            Saved = result.savedCount,
            Failed = result.failedCount,
            Errors = result.errors,
            Message = result.success 
                ? $"成功上傳 {result.savedCount} 個檔案" 
                : $"上傳完成：{result.savedCount} 成功，{result.failedCount} 失敗"
        });
    }
    #endregion

    #region 檔案下載
    /// <summary>
    /// 下載檔案（重定向到 URL）
    /// </summary>
    [HttpGet]
    public IActionResult Download(string code)
    {
        var url = svc.GetFileUrl(code);
        if (string.IsNullOrEmpty(url))
            return NotFound("檔案不存在");

        return Redirect(url);
    }
    #endregion

    #region 輔助 API
    /// <summary>
    /// 重新載入目錄內容（AJAX，從資料庫）
    /// </summary>
    [HttpGet]
    public IActionResult LoadDirectory(string? parentCode = null)
    {
        try
        {
            var username = User.Identity?.Name;
            var files = svc.LoadFileList(username, parentCode);
            
            var folders = files.Where(f => f.item_type == "folder").ToList();
            var fileList = files.Where(f => f.item_type == "file").ToList();

            return Json(new LoadDirectoryResponse
            {
                Success = true,
                Folders = folders,
                Files = fileList,
                Breadcrumb = null,
                Stats = new
                {
                    FolderCount = folders.Count,
                    FileCount = fileList.Count,
                    TotalSize = fileList.Sum(f => f.file_size ?? 0)
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new LoadDirectoryResponse { Success = false, Message = ex.Message });
        }
    }
    #endregion
}

