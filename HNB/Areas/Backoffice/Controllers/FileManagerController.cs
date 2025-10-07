using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HNB.Areas.Backoffice.Controllers;

/// <summary>
/// 檔案管理器控制器，負責處理檔案和資料夾的HTTP請求
/// 遵循現有的架構模式：統一的錯誤處理 + 功能分組
/// </summary>
[Area("Backoffice")]
public class FileManagerController(FileManagerService svc) : BaseController
{
    #region 統一的錯誤處理方法
    /// <summary>
    /// 標準化路徑
    /// </summary>
    private string V(string? path) => svc.NormalizePath(path ?? "/");

    /// <summary>
    /// 嘗試執行操作（同步版本）
    /// </summary>
    private IActionResult TryDo(string? path, Func<string, IActionResult> action)
    {
        var v = V(path);
        try { return action(v); }
        catch (Exception ex) { return ErrorOrRedirect(v, ex); }
    }

    /// <summary>
    /// 嘗試執行操作（非同步版本）
    /// </summary>
    private async Task<IActionResult> TryDoAsync(string? path, Func<string, Task> action, object? okPayload = null)
    {
        var v = V(path);
        try { await action(v); return OkOrRedirect(v, okPayload); }
        catch (Exception ex) { return ErrorOrRedirect(v, ex); }
    }
    #endregion

    #region 主要頁面
    /// <summary>
    /// 檔案管理器主頁面
    /// </summary>
    public IActionResult FileManager(string? path = "/")
    {
        var vPath = V(path);
        
        // 使用統一的ViewBag設定
        svc.ViewBagModel(ViewBag, vPath);
        
        return View();
    }

    #endregion

    #region 檔案上傳功能
    /// <summary>
    /// 上傳單一檔案
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(200_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([Required] IFormFile file, string? path = "/")
    {
        var v = V(path);
        var (success, message, fileName) = await svc.UploadAsync(v, file);
        
        if (success)
        {
            return OkOrRedirect(v, new { message, fileName });
        }
        else
        {
            return ErrorOrRedirect(v, new Exception(message));
        }
    }

    /// <summary>
    /// 批量上傳檔案
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(2_000_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMany([Required] IReadOnlyList<IFormFile> files, string? path = "/")
    {
        var v = V(path);
        var (success, message, uploadedCount, fileNames) = await svc.UploadManyAsync(v, files);
        
        if (success)
        {
            return OkOrRedirect(v, new { message, uploadedCount, fileNames });
        }
        else
        {
            return ErrorOrRedirect(v, new Exception(message));
        }
    }
    #endregion

    #region 檔案和資料夾管理功能
    /// <summary>
    /// 建立資料夾
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateFolder([Required] string folderName, string? path = "/")
    {
        var v = V(path);
        var (success, message) = svc.CreateFolder(v, folderName);
        
        if (success)
        {
            return OkOrRedirect(v, new { message, name = folderName });
        }
        else
        {
            return ErrorOrRedirect(v, new Exception(message));
        }
    }

    /// <summary>
    /// 建立空檔案
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateEmptyFile([Required] string fileName, string? path = "/")
    {
        var v = V(path);
        var (success, message) = svc.CreateEmptyFile(v, fileName);
        
        if (success)
        {
            return OkOrRedirect(v, new { message, name = fileName });
        }
        else
        {
            return ErrorOrRedirect(v, new Exception(message));
        }
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteFile([Required] string name, string? path = "/")
    {
        var v = V(path);
        var (success, message) = svc.DeleteFile(v, name);
        
        if (success)
        {
            return OkOrRedirect(v, new { message, name });
        }
        else
        {
            return ErrorOrRedirect(v, new Exception(message));
        }
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteFolder([Required] string name, string? path = "/")
    {
        var v = V(path);
        var (success, message) = svc.DeleteFolder(v, name);
        
        if (success)
        {
            return OkOrRedirect(v, new { message, name });
        }
        else
        {
            return ErrorOrRedirect(v, new Exception(message));
        }
    }

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RenameFile([Required] string oldName, [Required] string newName, string? path = "/")
    {
        var v = V(path);
        var (success, message) = svc.RenameFile(v, oldName, newName);
        
        if (success)
        {
            return OkOrRedirect(v, new { message, oldName, newName });
        }
        else
        {
            return ErrorOrRedirect(v, new Exception(message));
        }
    }

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RenameFolder([Required] string oldName, [Required] string newName, string? path = "/")
    {
        var v = V(path);
        var (success, message) = svc.RenameFolder(v, oldName, newName);
        
        if (success)
        {
            return OkOrRedirect(v, new { message, oldName, newName });
        }
        else
        {
            return ErrorOrRedirect(v, new Exception(message));
        }
    }
    #endregion

    #region 檔案讀取和下載功能
    /// <summary>
    /// 下載檔案
    /// </summary>
    [HttpGet]
    public IActionResult Download([Required] string name, string? path = "/")
    {
        var v = V(path);
        var (stream, fileName, contentType) = svc.OpenRead(v, name);
        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// 下載資料夾ZIP
    /// </summary>
    [HttpGet]
    public IActionResult DownloadFolderZip([Required] string name, string? path = "/")
        => TryDo(path, v => {
            var (stream, fileName, ct) = svc.ZipFolder(v, name);
            return File(stream, ct, fileName);
        });

    /// <summary>
    /// 原始檔案內容
    /// </summary>
    [HttpGet]
    public IActionResult Raw([Required] string name, string? path = "/")
    {
        var v = V(path);
        var (stream, ct) = svc.OpenRaw(v, name);
        return File(stream, ct);
    }
    #endregion

    #region 文字檔案編輯功能
    /// <summary>
    /// 讀取文字檔案
    /// </summary>
    [HttpGet]
    public IActionResult ReadText([Required] string name, string? path = "/")
        => TryDo(path, v => {
            var (content, encoding, last) = svc.ReadTextFile(v, name);
            return Json(new { ok = true, content, encoding, lastWriteUtc = last });
        });

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SaveText([Required] string name, [Required] string content, string? encoding = "utf-8", string? path = "/")
    {
        var v = V(path);
        var (success, message) = svc.SaveTextFile(v, name, content, encoding);
        
        if (success)
        {
            return OkOrRedirect(v, new { message, name });
        }
        else
        {
            return ErrorOrRedirect(v, new Exception(message));
        }
    }
    #endregion


    #region 統一的回應處理方法
    /// <summary>
    /// 檢查是否為AJAX請求
    /// </summary>
    private bool IsAjaxRequest()
    {
        var xrw = Request?.Headers["X-Requested-With"].ToString();
        if (!string.IsNullOrEmpty(xrw) && xrw.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase)) return true;
        var accept = Request?.Headers["Accept"].ToString() ?? "";
        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 成功回應或重導向
    /// </summary>
    private IActionResult OkOrRedirect(string path, object? payload = null)
    {
        if (IsAjaxRequest()) return Json(new { ok = true, path, data = payload });
        return RedirectToAction(nameof(FileManager), new { path });
    }

    /// <summary>
    /// 錯誤回應或重導向
    /// </summary>
    private IActionResult ErrorOrRedirect(string path, Exception ex)
    {
        if (IsAjaxRequest()) return BadRequest(new { ok = false, path, error = ex.Message });
        TempData["Error"] = ex.Message;
        return RedirectToAction(nameof(FileManager), new { path });
    }
    #endregion

}
