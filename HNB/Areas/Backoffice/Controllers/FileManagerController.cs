using HNB.Areas.Backoffice.Dtos;
using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Core;
using Microsoft.AspNetCore.Mvc;
using Models.HnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class FileManagerController(FileManagerServices svc, PermissionManagementService permissionService, OrganizationScope organizationScope) : BaseController
{
    #region 主頁面
    /// <summary>
    /// 檔案管理主頁面
    /// </summary>
    public IActionResult Index(string? path = null) => View();

    /// <summary>
    /// 載入側邊欄（用於 AJAX 更新）
    /// </summary>
    public IActionResult LoadSidebar(string? currentPath = "/", string? viewMode = null)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var dto = new HNB.Areas.Backoffice.Dtos.FileManagerIndexDto
        {
            CurrentPath = currentPath ?? "/",
            ViewMode = viewMode,
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
            CurrentPath = path,
            CurrentUser = currentUser
        };
        
        var partialView = view?.ToLower() switch
        {
            "shared" => "Partials/FileManager/_SharedWithMe",
            "recent" => "Partials/FileManager/_Recent",
            _ => "Partials/FileManager/_FileList"
        };
        
        switch (view?.ToLower())
        {
            case "shared":
                dto.Items = svc.LoadSharedWithMe(currentUser);
                var sharedFolders = dto.Items.Where(i => i.Type == "folder").ToList();
                var sharedFiles = dto.Items.Where(i => i.Type == "file").ToList();
                dto.FolderCount = sharedFolders.Count;
                dto.FileCount = sharedFiles.Count;
                dto.TotalSize = sharedFiles.Sum(f => f.Size ?? 0L);
                break;
            case "recent":
                dto.Items = svc.LoadRecentItems(currentUser);
                var recentFolders = dto.Items.Where(i => i.Type == "folder").ToList();
                var recentFiles = dto.Items.Where(i => i.Type == "file").ToList();
                dto.FolderCount = recentFolders.Count;
                dto.FileCount = recentFiles.Count;
                dto.TotalSize = recentFiles.Sum(f => f.Size ?? 0L);
                break;
            
            default:
                var userStoragePath = svc.GetUserStoragePath(currentUser);
                var actualPath = svc.GetActualPath(currentUser, path);
                dto.UserStoragePath = userStoragePath;
                dto.UserFolders = svc.LoadUserFolders(currentUser);
                dto.Items = svc.LoadUserFileSystemItems(actualPath, currentUser);
                
                var stats = svc.CalculateFolderStatistics(actualPath, currentUser);
                dto.FolderCount = stats.folderCount;
                dto.FileCount = stats.fileCount;
                dto.TotalSize = stats.totalSize;
                break;
        }
        
        return PartialView(partialView, dto);
    }

    /// <summary>
    /// 載入麵包屑（用於 AJAX 更新）
    /// </summary>
    public IActionResult LoadBreadcrumb(string? view = null, string path = "/")
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var dto = new FileManagerDto
        {
            ViewMode = view,
            CurrentPath = path
        };
        
        switch (view?.ToLower())
        {
            case "shared":
                var sharedItems = svc.LoadSharedWithMe(currentUser);
                var sharedFolders = sharedItems.Where(i => i.Type == "folder").ToList();
                var sharedFiles = sharedItems.Where(i => i.Type == "file").ToList();
                dto.FolderCount = sharedFolders.Count;
                dto.FileCount = sharedFiles.Count;
                dto.TotalSize = sharedFiles.Sum(f => f.Size ?? 0L);
                break;
            case "recent":
                var recentItems = svc.LoadRecentItems(currentUser);
                var recentFolders = recentItems.Where(i => i.Type == "folder").ToList();
                var recentFiles = recentItems.Where(i => i.Type == "file").ToList();
                dto.FolderCount = recentFolders.Count;
                dto.FileCount = recentFiles.Count;
                dto.TotalSize = recentFiles.Sum(f => f.Size ?? 0L);
                break;
            
            default:
                var actualPath = svc.GetActualPath(currentUser, path);
                var stats = svc.CalculateFolderStatistics(actualPath, currentUser);
                dto.FolderCount = stats.folderCount;
                dto.FileCount = stats.fileCount;
                dto.TotalSize = stats.totalSize;
                break;
        }
        
        return PartialView("Partials/FileManager/_Breadcrumb", dto);
    }

    /// <summary>
    /// 載入共享 Modal 資料
    /// </summary>
    public IActionResult LoadShareModal(string path, string name)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var detail = svc.LoadFileSystemDetail(svc.GetActualPath(currentUser, path), name, currentUser);
        
        if (detail == null)
        {
            return Json(new { success = false, message = "檔案或資料夾不存在，或您沒有權限存取" });
        }
        
        if (organizationScope == null)
        {
            return Json(new { success = false, message = "組織範圍服務未初始化" });
        }
        
        if (permissionService == null)
        {
            return Json(new { success = false, message = "權限管理服務未初始化" });
        }
        
        var scope = organizationScope.ResolveUserScope(User);
        if (scope == null)
        {
            return Json(new { success = false, message = "無法取得使用者組織範圍" });
        }
        
        var users = permissionService.LoadUserList(isActive: true, organizationIds: scope.ScopeIds ?? new List<int>());
        
        ViewBag.ItemDetail = detail;
        ViewBag.Users = users;
        
        return PartialView("Partials/FileManager/Modal/_Share");
    }

    /// <summary>
    /// 搜尋使用者（用於共享關鍵字搜尋，不限制組織）
    /// </summary>
    public IActionResult SearchUsersForShare(string? searchTerm = null)
    {
        var users = permissionService.LoadUserList(searchTerm: searchTerm, isActive: true);
        return Json(users.Select(u => new
        {
            name = u.name ?? "",
            email = u.email ?? "",
            fullName = u.full_name ?? u.name ?? "",
            initial = !string.IsNullOrEmpty(u.full_name) ? u.full_name.Substring(0, 1).ToUpper() : (!string.IsNullOrEmpty(u.name) ? u.name.Substring(0, 1).ToUpper() : "?")
        }).ToList());
    }

    /// <summary>
    /// 載入項目詳細資訊
    /// </summary>
    public IActionResult LoadDetail(string path, string name)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        string actualPath;
        if (path?.StartsWith("/storage/", StringComparison.OrdinalIgnoreCase) == true)
        {
            actualPath = path;
        }
        else
        {
            var userStoragePath = svc.GetUserStoragePath(currentUser);
            actualPath = path == "/" ? userStoragePath : $"{userStoragePath}{path}";
        }
        
        var detail = svc.LoadFileSystemDetail(actualPath, name, currentUser);
        
        if (detail == null)
        {
            return Content("<div class='text-center text-muted small py-4'><i data-lucide='alert-circle' style='width:2rem;height:2rem;' class='mb-2'></i><div>檔案或資料夾不存在，或您沒有權限存取</div></div>");
        }
        
        ViewBag.CurrentUser = currentUser;
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
        
        var entry = new FileSystemEntry { VirtualPath = svc.GetActualPath(currentUser, request.Path), Name = request.Name, Type = "folder" };
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
        
        var entry = new FileSystemEntry { VirtualPath = svc.GetActualPath(currentUser, request.Path), Name = request.Name, Type = "folder" };
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
        
        var entry = new FileSystemEntry { VirtualPath = svc.GetActualPath(currentUser, request.Path), Name = request.OldName, Type = "folder" };
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
        
        var entry = new FileSystemEntry { VirtualPath = svc.GetActualPath(currentUser, request.Path), Name = request.Name, Type = "file" };
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
        
        var entry = new FileSystemEntry { VirtualPath = svc.GetActualPath(currentUser, request.Path), Name = request.Name, Type = "file" };
        
        try
        {
            svc.DeleteFile(entry);
            return Json(new FileManagerResponse { Success = true, Message = "檔案已刪除" });
        }
        catch (IOException)
        {
            return Json(new FileManagerResponse { Success = true, Message = "檔案已標記為待刪除，將在背景服務中處理" });
        }
        catch (UnauthorizedAccessException)
        {
            return Json(new FileManagerResponse { Success = true, Message = "檔案已標記為待刪除，將在背景服務中處理" });
        }
        catch (Exception ex)
        {
            return Json(new FileManagerResponse { Success = false, Message = $"刪除失敗：{ex.Message}" });
        }
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
        
        var entry = new FileSystemEntry { VirtualPath = svc.GetActualPath(currentUser, request.Path), Name = request.OldName, Type = "file" };
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
        
        var entry = new FileSystemEntry { VirtualPath = svc.GetActualPath(currentUser, request.Path), Name = request.Name, Type = "file" };
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
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var actualPath = svc.GetActualPath(currentUser, request.Path);
        var entry = new FileSystemEntry { VirtualPath = actualPath, Name = request.Name };
        var detail = svc.LoadFileSystemDetail(actualPath, request.Name, currentUser);
        
        if (detail == null)
        {
            return Json(new { success = false, message = "檔案或資料夾不存在，或您沒有權限存取" });
        }
        
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
            return Json(new { success = false, message = "擁有者列表不可為空，且不得移除原擁有者。" });

        svc.UpdateItemOwners(entry, normalized.ToArray());
        return Json(new FileManagerResponse { Success = true, Message = "共享設定已更新" });
    }
    #endregion

    #region 檔案上傳
    /// <summary>
    /// 上傳檔案（最大 5GB）
    /// </summary>
    /// <param name="virtualPath">表單欄位名稱：virtualPath（上傳目標虛擬路徑，例如：/、/FolderA/Sub）</param>
    /// <param name="files">表單欄位名稱：files（檔案清單，多個檔案可重複此欄位）</param>
    /// <param name="relativePaths">表單欄位名稱：relativePaths（可選；每個檔案對應一個相對路徑，用於保留資料夾結構）</param>
    [HttpPost, DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = 5368709120), RequestSizeLimit(5368709120)] // 5GB
    [ValidateAntiForgeryToken]
    public IActionResult SubmitUpload(string virtualPath, List<IFormFile> files, List<string>? relativePaths)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();

        var result = svc.UploadBatchFiles(svc.GetActualPath(currentUser, virtualPath), files, relativePaths);
        
        return Json(new UploadResponse
        {
            Success = result.success,
            Saved = result.savedCount,
            Failed = result.failedCount,
            Errors = result.errors,
            Message = result.success 
                ? $"成功上傳 {result.savedCount} 個檔案" 
                : (result.errors.Count > 0 ? result.errors[0] : $"上傳完成：{result.savedCount} 成功，{result.failedCount} 失敗")
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
        
        var (stream, fileName, contentType) = svc.DownloadFile(svc.GetActualPath(currentUser, path), name);
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// 下載資料夾（ZIP）
    /// </summary>
    public IActionResult DownloadFolder(string path, string name)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var (stream, fileName, contentType) = svc.DownloadFolderAsZip(svc.GetActualPath(currentUser, path), name);
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// 預覽檔案
    /// </summary>
    public IActionResult Preview(string path, string name)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();
        
        var (stream, contentType) = svc.PreviewFile(svc.GetActualPath(currentUser, path), name);
        Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{Uri.EscapeDataString(name)}";
        return File(stream, contentType, enableRangeProcessing: true);
    }

    /// <summary>
    /// 處理待刪除項目（背景服務 API）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ProcessPendingDeletions([FromQuery] int maxItems = 100)
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();

        var result = svc.ProcessPendingDeletions(maxItems);
        
        return Json(new
        {
            Success = result.failedCount == 0,
            SuccessCount = result.successCount,
            FailedCount = result.failedCount,
            Errors = result.errors,
            Message = result.failedCount == 0
                ? $"成功處理 {result.successCount} 個待刪除項目"
                : $"處理完成：{result.successCount} 成功，{result.failedCount} 失敗"
        });
    }

    #endregion

}