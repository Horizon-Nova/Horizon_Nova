using HNB.Areas.Backoffice.Controllers;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

[Area("Backoffice")]
public class FileManagerController(FileManagerServices svc) : BaseController
{
    #region 主頁面
    /// <summary>
    /// 檔案管理主頁面
    /// </summary>
    public IActionResult FileManager(string? parentCode = null)
    {
        var username = User.Identity!.Name!;
        var files = svc.LoadFileList(username, parentCode);
        
        ViewBag.CurrentParentCode = parentCode;
        ViewBag.CurrentFolder = !string.IsNullOrEmpty(parentCode) ? svc.LoadFile(code: parentCode) : null;
        ViewBag.AllFolders = svc.LoadAllFolders(username);
        
        return View(files);
    }
    #endregion

    #region 資料夾操作 API
    /// <summary>
    /// 建立資料夾
    /// </summary>
    [HttpPost]
    public IActionResult CreateFolder(string? parentCode, string name)
    {
        var result = svc.CreateFolder(parentCode, name);
        return Json(new { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    [HttpPost]
    public IActionResult DeleteFolder(string code)
    {
        var result = svc.DeleteFolder(code);
        return Json(new { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    [HttpPost]
    public IActionResult RenameFolder(string code, string newName)
    {
        var result = svc.RenameFolder(code, newName);
        return Json(new { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 下載資料夾（ZIP）
    /// </summary>
    [HttpGet]
    public IActionResult DownloadFolder(string code)
    {
        var result = svc.DownloadFolderAsZip(code);
        if (!result.success)
            return NotFound(result.message);
        
        return File(result.zipBytes!, "application/zip", result.fileName);
    }
    #endregion

    #region 檔案操作 API
    /// <summary>
    /// 建立檔案
    /// </summary>
    [HttpPost]
    public IActionResult CreateFile(string? parentCode, string name)
    {
        var result = svc.CreateFile(parentCode, name);
        return Json(new { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    [HttpPost]
    public IActionResult DeleteFile(string code)
    {
        var result = svc.DeleteFile(code);
        return Json(new { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    [HttpPost]
    public IActionResult RenameFile(string code, string newName)
    {
        var result = svc.RenameFile(code, newName);
        return Json(new { Success = result.success, Message = result.message });
    }

    /// <summary>
    /// 讀取文字檔案
    /// </summary>
    [HttpGet]
    public IActionResult ReadTextFile(string code)
    {
        try
        {
            var result = svc.ReadTextFile(code);
            return Json(new 
            { 
                Success = true, 
                Content = result.content, 
                Encoding = result.encoding, 
                LastModified = result.lastModified 
            });
        }
        catch (Exception ex)
        {
            return Json(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    [HttpPost]
    public IActionResult SaveTextFile(string code, string content, string? encoding = "utf-8")
    {
        var result = svc.SaveTextFile(code, content, encoding);
        return Json(new { Success = result.success, Message = result.message });
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
            return Json(new { Success = false, Message = "未選擇檔案" });

        var username = User.Identity!.Name!;
        var result = svc.UploadBatchFiles(path, files, keys, username);
        
        return Json(new
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

    #region AJAX API
    /// <summary>
    /// 重新載入目錄內容（AJAX，從資料庫）
    /// </summary>
    [HttpGet]
    public IActionResult LoadDirectory(string? parentCode = null)
    {
        try
        {
            var username = User.Identity!.Name!;
            var files = svc.LoadFileList(username, parentCode);
            
            var folders = files.Where(f => f.item_type == "folder").ToList();
            var fileList = files.Where(f => f.item_type == "file").ToList();

            return Json(new
            {
                Success = true,
                Folders = folders,
                Files = fileList,
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
            return Json(new { Success = false, Message = ex.Message });
        }
    }
    #endregion
}

