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
    public IActionResult Index() => View();

    /// <summary>
    /// 載入側邊欄（用於 AJAX 更新）
    /// </summary>
    public IActionResult LoadSidebar(string? currentPath = "/")
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var dto = new HNB.Areas.Backoffice.Dtos.FileManagerIndexDto
        {
            CurrentPath = currentPath ?? "/",
            UserFolderTree = svc.BuildFolderTree(currentUser),
            UserStoragePath = svc.GetUserStoragePath(currentUser),
            UsedStorage = svc.CalculateUserStorageUsage(currentUser),
            TotalStorage = svc.GetUserStorageLimit(currentUser)
        };
        
        return PartialView("Partials/FileManager/_Sidebar", dto);
    }

    /// <summary>
    /// 載入檔案總管視圖（用於 AJAX 切換畫面）
    /// </summary>
    public IActionResult LoadView(string? view = null, string path = "/")
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var dto = new FileManagerDto
        {
            ViewMode = view,
            CurrentPath = path
        };
        
        var partialView = view?.ToLower() switch
        {
            "shared" => "Partials/FileManager/_SharedWithMe",
            "recent" => "Partials/FileManager/_Recent",
            "trash" => "Partials/FileManager/_Trash",
            _ => "Partials/FileManager/_FileList"
        };
        
        switch (view?.ToLower())
        {
            case "shared":
            case "recent":
            case "trash":
                dto.CurrentPath = view ?? "/";
                dto.FolderCount = 0;
                dto.FileCount = 0;
                dto.TotalSize = 0L;
                break;
            
            default:
                var userStoragePath = svc.GetUserStoragePath(currentUser);
                var actualPath = path == "/" ? userStoragePath : $"{userStoragePath}{path}";
                dto.UserStoragePath = userStoragePath;
                dto.UserFolders = svc.LoadUserFolders(currentUser);
                var items = svc.LoadUserFileSystemItems(actualPath, currentUser);
                dto.Items = items;
                var folders = items.Where(f => f.Type == "folder").ToList();
                var files = items.Where(f => f.Type == "file").ToList();
                dto.FolderCount = folders.Count;
                dto.FileCount = files.Count;
                dto.TotalSize = files.Sum(f => f.Size ?? 0L);
                break;
        }
        
        return PartialView(partialView, dto);
    }

    /// <summary>
    /// 載入項目詳細資訊
    /// </summary>
    public IActionResult LoadDetail(string path, string name)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = path == "/" ? userStoragePath : $"{userStoragePath}{path}";
        
        var detail = svc.LoadFileSystemDetail(actualPath, name, currentUser);
        
        return PartialView("Partials/FileManager/_ItemDetail", detail);
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
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = request.Path == "/" ? userStoragePath : $"{userStoragePath}{request.Path}";
        var entry = new FileSystemEntry { VirtualPath = actualPath, Name = request.Name, Type = "folder" };
        svc.CreateFolder(entry);
        if (request.SharedUsers != null && request.SharedUsers.Count > 0)
            svc.UpdateItemOwners(entry, request.SharedUsers.ToArray());

        return Json(new FileManagerResponse { Success = true, Message = "資料夾已建立", ShouldReloadSidebar = true });
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete([FromBody] DeleteItemRequest request)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = request.Path == "/" ? userStoragePath : $"{userStoragePath}{request.Path}";
        var entry = new FileSystemEntry { VirtualPath = actualPath, Name = request.Name, Type = "folder" };
        svc.DeleteFolder(entry);
        return Json(new FileManagerResponse { Success = true, Message = "資料夾已刪除", ShouldReloadSidebar = true });
    }

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitRename([FromBody] RenameItemRequest request)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = request.Path == "/" ? userStoragePath : $"{userStoragePath}{request.Path}";
        var entry = new FileSystemEntry { VirtualPath = actualPath, Name = request.OldName, Type = "folder" };
        svc.RenameFolder(entry, request.NewName);
        return Json(new FileManagerResponse { Success = true, Message = "資料夾已重新命名", ShouldReloadSidebar = true });
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
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = request.Path == "/" ? userStoragePath : $"{userStoragePath}{request.Path}";
        var entry = new FileSystemEntry { VirtualPath = actualPath, Name = request.Name, Type = "file" };
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
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = request.Path == "/" ? userStoragePath : $"{userStoragePath}{request.Path}";
        var entry = new FileSystemEntry { VirtualPath = actualPath, Name = request.Name, Type = "file" };
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
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = request.Path == "/" ? userStoragePath : $"{userStoragePath}{request.Path}";
        var entry = new FileSystemEntry { VirtualPath = actualPath, Name = request.OldName, Type = "file" };
        svc.RenameFile(entry, request.NewName);
        return Json(new FileManagerResponse { Success = true, Message = "檔案已重新命名" });
    }

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitSaveTextFile([FromBody] SaveTextFileRequest request)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = request.Path == "/" ? userStoragePath : $"{userStoragePath}{request.Path}";
        var entry = new FileSystemEntry { VirtualPath = actualPath, Name = request.Name, Type = "file" };
        svc.SaveTextFile(entry, request.Content, request.Encoding);
        return Json(new FileManagerResponse { Success = true, Message = "檔案已儲存" });
    }

    /// <summary>
    /// 生成共享連結
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GenerateShareLink([FromBody] GenerateShareLinkRequest request)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = request.Path == "/" ? userStoragePath : $"{userStoragePath}{request.Path}";
        var detail = svc.LoadFileSystemDetail(actualPath, request.Name, currentUser);
        
        if (detail.PrimaryOwner != currentUser && !detail.SharedUsers.Contains(currentUser))
            return Unauthorized();
        
        var shareToken = Guid.NewGuid().ToString("N");
        var shareLink = $"{Request.Scheme}://{Request.Host}{Url.Action("AccessShared", "FileManager", new { area = "Backoffice", token = shareToken })}";
        
        return Json(new FileManagerResponse 
        { 
            Success = true, 
            Message = "共享連結已生成",
            Data = new { ShareLink = shareLink, ShareToken = shareToken }
        });
    }

    /// <summary>
    /// 透過共享連結存取檔案/資料夾
    /// </summary>
    [HttpGet]
    public IActionResult AccessShared(string token)
    {
        return BadRequest("共享連結功能開發中");
    }

    /// <summary>
    /// 設定分享擁有者（檔案或資料夾）- 保留舊版 API 以向後相容
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitShare([FromBody] UpdateOwnersRequest request)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = request.Path == "/" ? userStoragePath : $"{userStoragePath}{request.Path}";
        var entry = new FileSystemEntry { VirtualPath = actualPath, Name = request.Name };
        var detail = svc.LoadFileSystemDetail(actualPath, request.Name, currentUser);
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
        return Json(new FileManagerResponse { Success = true, Message = "共享設定已更新" });
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
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        if (files == null || files.Count == 0)
            return Json(new FileManagerResponse { Success = false, Message = "未選擇檔案" });

        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = virtualPath == "/" ? userStoragePath : $"{userStoragePath}{virtualPath}";
        var result = svc.UploadBatchFiles(actualPath, files, relativePaths);
        
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
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = path == "/" ? userStoragePath : $"{userStoragePath}{path}";
        var (stream, fileName, contentType) = svc.DownloadFile(actualPath, name);
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// 下載資料夾（ZIP）
    /// </summary>
    public IActionResult DownloadFolder(string path, string name)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = path == "/" ? userStoragePath : $"{userStoragePath}{path}";
        var (stream, fileName, contentType) = svc.DownloadFolderAsZip(actualPath, name);
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// 預覽檔案
    /// </summary>
    public IActionResult Preview(string path, string name)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var userStoragePath = svc.GetUserStoragePath(currentUser);
        var actualPath = path == "/" ? userStoragePath : $"{userStoragePath}{path}";
        var (stream, contentType) = svc.PreviewFile(actualPath, name);
        Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{Uri.EscapeDataString(name)}";
        return File(stream, contentType, enableRangeProcessing: true);
    }

    #endregion

}