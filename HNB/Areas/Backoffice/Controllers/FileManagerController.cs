using HNB.Areas.Backoffice.Dtos;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class FileManagerController(FileManagerServices svc) : BaseController
{
    #region 主頁面
    /// <summary>
    /// 檔案管理主頁面
    /// </summary>
    public IActionResult Index(string path = "/")
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        svc.ViewBagModel(ViewBag, path, currentUser);
        var model = svc.LoadUserFileSystemItems(path ?? "/", currentUser);
        return View(model);
    }
    #endregion

    #region Modal 載入
    public IActionResult LoadDetail(string? name = null, string? currentPath = null, string? type = null)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        var detail = svc.LoadFileSystemDetail(currentPath ?? "/", name ?? "", currentUser);
        ViewBag.CurrentPath = currentPath ?? "/";
        ViewBag.CurrentUser = currentUser;
        return PartialView("Partials/FileManager/_SidePanelBody", detail);
    }

    #endregion

    #region 資料夾操作 API
    /// <summary>
    /// 建立資料夾
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitFolder([FromBody] CreateItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var entry = new FileSystemEntry { VirtualPath = request.Path, Name = request.Name, Type = "folder" };
        svc.CreateFolder(entry);
        if (request.SharedUsers != null && request.SharedUsers.Count > 0)
            svc.UpdateItemOwners(entry, request.SharedUsers.ToArray());

        return Json(new FileManagerResponse { Success = true, Message = "資料夾已建立" });
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete([FromBody] DeleteItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var entry = new FileSystemEntry { VirtualPath = request.Path, Name = request.Name, Type = "folder" };
        svc.DeleteFolder(entry);
        return Json(new FileManagerResponse { Success = true, Message = "資料夾已刪除" });
    }

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitRename([FromBody] RenameItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var entry = new FileSystemEntry { VirtualPath = request.Path, Name = request.OldName, Type = "folder" };
        svc.RenameFolder(entry, request.NewName);
        return Json(new FileManagerResponse { Success = true, Message = "資料夾已重新命名" });
    }
    #endregion

    #region 檔案操作 API
    /// <summary>
    /// 建立檔案
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitFile([FromBody] CreateItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var entry = new FileSystemEntry { VirtualPath = request.Path, Name = request.Name, Type = "file" };
        svc.CreateFile(entry);
        if (request.SharedUsers != null && request.SharedUsers.Count > 0)
            svc.UpdateItemOwners(entry, request.SharedUsers.ToArray());

        return Json(new FileManagerResponse { Success = true, Message = "檔案已建立" });
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteFile([FromBody] DeleteItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var entry = new FileSystemEntry { VirtualPath = request.Path, Name = request.Name, Type = "file" };
        svc.DeleteFile(entry);
        return Json(new FileManagerResponse { Success = true, Message = "檔案已刪除" });
    }

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitRenameFile([FromBody] RenameItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var entry = new FileSystemEntry { VirtualPath = request.Path, Name = request.OldName, Type = "file" };
        svc.RenameFile(entry, request.NewName);
        return Json(new FileManagerResponse { Success = true, Message = "檔案已重新命名" });
    }

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitTextFile([FromBody] SaveTextFileRequest request)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var entry = new FileSystemEntry { VirtualPath = request.Path, Name = request.Name, Type = "file" };
        svc.SaveTextFile(entry, request.Content, request.Encoding);
        return Json(new FileManagerResponse { Success = true, Message = "檔案已儲存" });
    }

    /// <summary>
    /// 設定分享擁有者（檔案或資料夾）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitShare([FromBody] UpdateOwnersRequest request)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var entry = new FileSystemEntry { VirtualPath = request.Path, Name = request.Name };
        var currentUser = User.Identity!.Name!;
        var detail = svc.LoadFileSystemDetail(request.Path, request.Name, currentUser);
        var primary = detail.PrimaryOwner;

        var incoming = request.Owners ?? new List<string>();
        var normalized = new List<string>();
        if (!string.IsNullOrWhiteSpace(primary)) normalized.Add(primary);
        foreach (var u in incoming)
        {
            if (!string.IsNullOrWhiteSpace(u) && !u.Equals(primary, StringComparison.OrdinalIgnoreCase))
                normalized.Add(u);
        }

        if (normalized.Count == 0)
            return BadRequest("擁有者列表不可為空，且不得移除原擁有者。");

        svc.UpdateItemOwners(entry, normalized.ToArray());
        return Json(new FileManagerResponse { Success = true, Message = "分享設定已更新" });
    }
    #endregion

    #region 檔案上傳
    /// <summary>
    /// 上傳檔案（最大 4GB）
    /// </summary>
    /// <param name="virtualPath">表單欄位名稱：virtualPath（上傳目標虛擬路徑，例如：/、/FolderA/Sub）</param>
    /// <param name="files">表單欄位名稱：files（檔案清單，多個檔案可重複此欄位）</param>
    /// <param name="relativePaths">表單欄位名稱：relativePaths（可選；每個檔案對應一個相對路徑，用於保留資料夾結構）</param>
    [HttpPost, DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = 4294967296), RequestSizeLimit(4294967296)] // 4GB
    [ValidateAntiForgeryToken]
    public IActionResult SubmitUpload(string virtualPath, List<IFormFile> files, List<string>? relativePaths)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        if (files == null || files.Count == 0)
            return Json(new FileManagerResponse { Success = false, Message = "未選擇檔案" });

        var result = svc.UploadBatchFiles(virtualPath, files, relativePaths);
        
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
    /// 下載檔案
    /// </summary>
    public IActionResult Download(string path, string name)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var (stream, fileName, contentType) = svc.DownloadFile(path, name);
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// 下載資料夾（ZIP）
    /// </summary>
    public IActionResult DownloadFolder(string path, string name)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var (stream, fileName, contentType) = svc.DownloadFolderAsZip(path, name);
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// 預覽檔案
    /// </summary>
    public IActionResult Preview(string path, string name)
    {
        if (string.IsNullOrWhiteSpace(User.Identity?.Name)) return Unauthorized();
        var (stream, contentType) = svc.PreviewFile(path, name);
        Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{Uri.EscapeDataString(name)}";
        return File(stream, contentType, enableRangeProcessing: true);
    }

    #endregion

}