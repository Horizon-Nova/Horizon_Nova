using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.Backoffice.Dtos;
using Microsoft.AspNetCore.Http;
using HNB.Areas.Backoffice.Services; // for PermissionManagementService
using Microsoft.Extensions.Configuration;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 檔案管理服務層，負責處理檔案和資料夾的業務邏輯
/// </summary>
public class FileManagerServices(DirectoryManagerUtilities DM, IHttpContextAccessor httpContextAccessor, PermissionManagementService permissionService, IConfiguration configuration)
{
    #region 查詢方法

    /// <summary>
    /// 取得用戶的儲存路徑（/storage/UserName），如果不存在則自動建立
    /// </summary>
    public string GetUserStoragePath(string currentUser)
    {
        var userStoragePath = $"/storage/{currentUser}";
        EnsureUserStorageExists(userStoragePath, currentUser);
        return userStoragePath;
    }

    /// <summary>
    /// 確保用戶儲存資料夾存在，如果不存在則建立並設定擁有者
    /// </summary>
    private void EnsureUserStorageExists(string virtualPath, string currentUser)
    {
        try
        {
            var relativePath = virtualPath.StartsWith("/storage/", StringComparison.OrdinalIgnoreCase)
                ? virtualPath.Substring("/storage/".Length)
                : virtualPath.TrimStart('/');
            
            var root = Path.GetFullPath(configuration["Storage:Root"] ?? "Areas/Backoffice/storage");
            Directory.CreateDirectory(root);
            
            var combined = Path.GetFullPath(Path.Combine(root, relativePath));
            
            if (!combined.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return;
            
            if (!Directory.Exists(combined))
            {
                Directory.CreateDirectory(combined);
                DirectoryManagerUtilities.SetAppOwners(combined, new[] { currentUser });
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 載入文字檔案內容
    /// </summary>
    public (string Content, string EncodingName, DateTime? LastWriteUtc) LoadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
    {
        var convertedPath = ConvertStoragePathToVirtualPath(virtualPath);
        return DM.LoadTextFile(convertedPath, fileName, maxBytes);
    }


    /// <summary>
    /// 將 /storage/UserName 格式的虛擬路徑轉換為 DirectoryManagerUtilities 可用的路徑
    /// </summary>
    private string ConvertStoragePathToVirtualPath(string storagePath)
    {
        if (storagePath.StartsWith("/storage/", StringComparison.OrdinalIgnoreCase))
            return storagePath.Substring("/storage".Length);
        return storagePath;
    }

    /// <summary>
    /// 取得實際路徑（用於 Controller）
    /// </summary>
    public string GetActualPath(string currentUser, string path)
    {
        if (path?.StartsWith("/storage/", StringComparison.OrdinalIgnoreCase) == true)
        {
            return path;
        }
        
        var userStoragePath = GetUserStoragePath(currentUser);
        return path == "/" ? userStoragePath : $"{userStoragePath}{path}";
    }

    /// <summary>
    /// 計算資料夾統計資訊（資料夾數、檔案數、總大小）
    /// </summary>
    public (int folderCount, int fileCount, long totalSize) CalculateFolderStatistics(string virtualPath, string currentUser)
    {
        var items = LoadUserFileSystemItems(virtualPath, currentUser);
        var folders = items.Where(f => f.Type == "folder").ToList();
        var files = items.Where(f => f.Type == "file").ToList();
        return (folders.Count, files.Count, files.Sum(f => f.Size ?? 0L));
    }

    /// <summary>
    /// 載入用戶有權限的檔案系統項目
    /// </summary>
    public List<FileSystemEntry> LoadUserFileSystemItems(string virtualPath, string currentUser)
    {
        var convertedPath = ConvertStoragePathToVirtualPath(virtualPath);
        return DM.LoadUserFileSystemItems(convertedPath, currentUser);
    }

    /// <summary>
    /// 載入用戶的資料夾列表（從 /storage/UserName 開始）
    /// </summary>
    public List<FileSystemEntry> LoadUserFolders(string currentUser)
    {
        var userStoragePath = GetUserStoragePath(currentUser);
        var items = LoadUserFileSystemItems(userStoragePath, currentUser);
        return items.Where(f => f.Type == "folder").ToList();
    }

    /// <summary>
    /// 載入與當前使用者共用的檔案和資料夾
    /// </summary>
    public List<FileSystemEntry> LoadSharedWithMe(string currentUser)
    {
        var root = Path.GetFullPath(configuration["Storage:Root"] ?? "Areas/Backoffice/storage");
        if (!Directory.Exists(root)) return new List<FileSystemEntry>();

        var sharedItems = new List<FileSystemEntry>();

        foreach (var userDir in Directory.GetDirectories(root))
        {
            var ownerUserName = Path.GetFileName(userDir);
            if (string.IsNullOrWhiteSpace(ownerUserName) || string.Equals(ownerUserName, currentUser, StringComparison.OrdinalIgnoreCase))
                continue;

            ScanSharedItems(userDir, $"/storage/{ownerUserName}", currentUser, sharedItems);
        }

        return sharedItems.OrderByDescending(i => i.UpdatedAt ?? DateTime.MinValue).ToList();
    }

    /// <summary>
    /// 載入最近90天內修改的檔案和資料夾
    /// </summary>
    public List<FileSystemEntry> LoadRecentItems(string currentUser)
    {
        var root = Path.GetFullPath(configuration["Storage:Root"] ?? "Areas/Backoffice/storage");
        var userAbsPath = Path.Combine(root, currentUser);
        
        if (!Directory.Exists(userAbsPath)) return new List<FileSystemEntry>();

        var recentItems = new List<FileSystemEntry>();
        var cutoffDate = DateTime.UtcNow.AddDays(-90);

        ScanRecentItems(userAbsPath, "/storage/" + currentUser, currentUser, recentItems, cutoffDate);

        return recentItems.OrderByDescending(i => i.UpdatedAt ?? DateTime.MinValue).ToList();
    }

    /// <summary>
    /// 遞迴掃描最近修改的項目
    /// </summary>
    private void ScanRecentItems(string absPath, string virtualPath, string currentUser, List<FileSystemEntry> recentItems, DateTime cutoffDate)
    {
        try
        {
            if (Directory.Exists(absPath))
            {
                foreach (var dir in Directory.GetDirectories(absPath))
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        var dirName = dirInfo.Name;
                        var dirVirtualPath = virtualPath == "/storage/" + currentUser ? $"{virtualPath}/{dirName}" : $"{virtualPath}/{dirName}";

                        if (!string.IsNullOrEmpty(DirectoryManagerUtilities.IsMarkedForDeletion(dir))) continue;

                        var owners = DirectoryManagerUtilities.GetAppOwners(dir);
                        var hasPermission = owners.Length == 0 || owners.Any(o => string.Equals(o, currentUser, StringComparison.OrdinalIgnoreCase));
                        if (!hasPermission) continue;

                        var lastWriteTime = dirInfo.LastWriteTimeUtc;
                        var createdTime = dirInfo.CreationTimeUtc;

                        if (lastWriteTime >= cutoffDate || createdTime >= cutoffDate)
                        {
                            var primaryOwner = owners.Length > 0 ? owners[0] : currentUser;
                            recentItems.Add(new FileSystemEntry
                            {
                                Name = dirName,
                                Type = "folder",
                                Size = null,
                                PrimaryOwner = primaryOwner,
                                Owner = primaryOwner,
                                SharedUsers = owners.Length > 0 ? owners.ToList() : new List<string> { primaryOwner },
                                VirtualPath = dirVirtualPath,
                                CreatedAt = createdTime,
                                UpdatedAt = lastWriteTime
                            });
                        }

                        ScanRecentItems(dir, dirVirtualPath, currentUser, recentItems, cutoffDate);
                    }
                    catch
                    {
                    }
                }

                foreach (var file in Directory.GetFiles(absPath))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(DirectoryManagerUtilities.IsMarkedForDeletion(file))) continue;

                        var owners = DirectoryManagerUtilities.GetAppOwners(file);
                        var hasPermission = owners.Length == 0 || owners.Any(o => string.Equals(o, currentUser, StringComparison.OrdinalIgnoreCase));
                        if (!hasPermission) continue;

                        var fileInfo = new FileInfo(file);
                        var lastWriteTime = fileInfo.LastWriteTimeUtc;
                        var createdTime = fileInfo.CreationTimeUtc;

                        if (lastWriteTime >= cutoffDate || createdTime >= cutoffDate)
                        {
                            var fileName = fileInfo.Name;
                            var primaryOwner = owners.Length > 0 ? owners[0] : currentUser;
                            var mimeType = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType)
                                ? contentType
                                : "application/octet-stream";

                            recentItems.Add(new FileSystemEntry
                            {
                                Name = fileName,
                                Type = "file",
                                Size = fileInfo.Length,
                                MimeType = mimeType,
                                PrimaryOwner = primaryOwner,
                                Owner = primaryOwner,
                                SharedUsers = owners.Length > 0 ? owners.ToList() : new List<string> { primaryOwner },
                                VirtualPath = virtualPath,
                                CreatedAt = createdTime,
                                UpdatedAt = lastWriteTime
                            });
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 遞迴掃描共用項目
    /// </summary>
    private void ScanSharedItems(string absPath, string virtualPath, string currentUser, List<FileSystemEntry> sharedItems)
    {
        try
        {
            if (Directory.Exists(absPath))
            {
                var owners = DirectoryManagerUtilities.GetAppOwners(absPath);
                var isFolderShared = owners.Length > 0 && owners.Contains(currentUser, StringComparer.OrdinalIgnoreCase) && 
                                    owners.Any(o => !string.Equals(o, currentUser, StringComparison.OrdinalIgnoreCase));
                
                if (isFolderShared)
                {
                    var dirInfo = new DirectoryInfo(absPath);
                    var folderName = dirInfo.Name;
                    
                    sharedItems.Add(new FileSystemEntry
                    {
                        Name = folderName,
                        Type = "folder",
                        Size = null,
                        PrimaryOwner = owners[0],
                        Owner = owners[0],
                        SharedUsers = owners.ToList(),
                        VirtualPath = virtualPath,
                        CreatedAt = dirInfo.CreationTimeUtc,
                        UpdatedAt = dirInfo.LastWriteTimeUtc
                    });
                    
                    return;
                }

                foreach (var subDir in Directory.GetDirectories(absPath))
                {
                    var subDirName = Path.GetFileName(subDir);
                    ScanSharedItems(subDir, $"{virtualPath}/{subDirName}", currentUser, sharedItems);
                }

                foreach (var file in Directory.GetFiles(absPath))
                {
                    try
                    {
                        var fileOwners = DirectoryManagerUtilities.GetAppOwners(file);
                        if (fileOwners.Length > 0 && fileOwners.Contains(currentUser, StringComparer.OrdinalIgnoreCase) && 
                            fileOwners.Any(o => !string.Equals(o, currentUser, StringComparison.OrdinalIgnoreCase)))
                        {
                            var fileInfo = new FileInfo(file);
                            var fileName = fileInfo.Name;
                            var mimeType = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType) 
                                ? contentType 
                                : "application/octet-stream";
                            
                            sharedItems.Add(new FileSystemEntry
                            {
                                Name = fileName,
                                Type = "file",
                                Size = fileInfo.Length,
                                MimeType = mimeType,
                                PrimaryOwner = fileOwners[0],
                                Owner = fileOwners[0],
                                SharedUsers = fileOwners.ToList(),
                                VirtualPath = virtualPath,
                                CreatedAt = fileInfo.CreationTimeUtc,
                                UpdatedAt = fileInfo.LastWriteTimeUtc
                            });
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 建立資料夾樹狀結構
    /// </summary>
    public List<HNB.Areas.Backoffice.Dtos.FolderTreeNode> BuildFolderTree(string currentUser)
    {
        var userStoragePath = GetUserStoragePath(currentUser);
        var rootItems = LoadUserFileSystemItems(userStoragePath, currentUser);
        var rootFolders = rootItems.Where(f => f.Type == "folder").ToList();
        
        var tree = new List<HNB.Areas.Backoffice.Dtos.FolderTreeNode>();
        
        foreach (var folder in rootFolders)
        {
            var node = BuildFolderTreeNode(userStoragePath, folder, currentUser, userStoragePath);
            tree.Add(node);
        }
        
        return tree;
    }

    private HNB.Areas.Backoffice.Dtos.FolderTreeNode BuildFolderTreeNode(string parentPath, FileSystemEntry folder, string currentUser, string userStoragePath)
    {
        var folderPath = parentPath.EndsWith("/") ? $"{parentPath}{folder.Name}" : $"{parentPath}/{folder.Name}";
        var convertedPath = ConvertStoragePathToVirtualPath(folderPath);
        var items = DM.LoadUserFileSystemItems(convertedPath, currentUser);
        var childFolders = items.Where(f => f.Type == "folder").ToList();
        
        var displayPath = folderPath;
        if (displayPath.StartsWith(userStoragePath, StringComparison.OrdinalIgnoreCase))
        {
            displayPath = displayPath.Substring(userStoragePath.Length);
            if (string.IsNullOrEmpty(displayPath))
                displayPath = "/";
            else if (!displayPath.StartsWith("/"))
                displayPath = "/" + displayPath;
        }
        
        var node = new HNB.Areas.Backoffice.Dtos.FolderTreeNode
        {
            Name = folder.Name,
            Path = displayPath
        };
        
        foreach (var childFolder in childFolders)
        {
            var childNode = BuildFolderTreeNode(folderPath, childFolder, currentUser, userStoragePath);
            node.Children.Add(childNode);
        }
        
        return node;
    }

    /// <summary>
    /// 載入檔案詳細資訊
    /// </summary>
    public FileSystemEntry? LoadFileSystemDetail(string virtualPath, string name, string currentUser)
    {
        var convertedPath = ConvertStoragePathToVirtualPath(virtualPath);
        return DM.LoadFileSystemDetail(convertedPath, name, currentUser);
    }

    /// <summary>
    /// 計算使用者儲存空間使用量
    /// </summary>
    public long CalculateUserStorageUsage(string currentUser)
    {
        try
        {
            var userStoragePath = GetUserStoragePath(currentUser);
            var convertedPath = ConvertStoragePathToVirtualPath(userStoragePath);
            var items = DM.LoadUserFileSystemItems(convertedPath, currentUser);
            
            long totalSize = 0;
            CalculateDirectorySize(convertedPath, currentUser, ref totalSize);
            
            return totalSize;
        }
        catch
        {
            return 0;
        }
    }

    private void CalculateDirectorySize(string virtualPath, string currentUser, ref long totalSize)
    {
        try
        {
            var items = DM.LoadUserFileSystemItems(virtualPath, currentUser);
            
            foreach (var item in items)
            {
                if (item.Type == "file" && item.Size.HasValue)
                {
                    totalSize += item.Size.Value;
                }
                else if (item.Type == "folder")
                {
                    var folderPath = virtualPath == "/" ? $"/{item.Name}" : $"{virtualPath}/{item.Name}";
                    CalculateDirectorySize(folderPath, currentUser, ref totalSize);
                }
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 取得使用者儲存空間上限（從配置或預設值）
    /// </summary>
    public long GetUserStorageLimit(string currentUser)
    {
        var limit = configuration["Storage:UserLimit"];
        if (!string.IsNullOrEmpty(limit) && long.TryParse(limit, out var limitBytes))
        {
            return limitBytes;
        }
        return 5_242_880_000;
    }



    /// <summary>
    /// 檢查檔案是否可編輯
    /// </summary>
    public static bool IsEditable(string fileName)
        => DirectoryManagerUtilities.IsEditable(fileName);

    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 建立資料夾
    /// </summary>
    public void CreateFolder(FileSystemEntry entry)
    {
        entry.VirtualPath = ConvertStoragePathToVirtualPath(entry.VirtualPath);
        DM.InsertFolder(entry);
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    public void DeleteFolder(FileSystemEntry entry)
    {
        entry.VirtualPath = ConvertStoragePathToVirtualPath(entry.VirtualPath);
        DM.DeleteFolder(entry);
    }

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    public void RenameFolder(FileSystemEntry entry, string newName)
    {
        entry.VirtualPath = ConvertStoragePathToVirtualPath(entry.VirtualPath);
        DM.UpdateFolder(entry, newName);
    }

    /// <summary>
    /// 建立空檔案
    /// </summary>
    public void CreateFile(FileSystemEntry entry)
    {
        entry.VirtualPath = ConvertStoragePathToVirtualPath(entry.VirtualPath);
        DM.InsertFile(entry);
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    public void DeleteFile(FileSystemEntry entry)
    {
        entry.VirtualPath = ConvertStoragePathToVirtualPath(entry.VirtualPath);
        DM.DeleteFile(entry);
    }

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    public void RenameFile(FileSystemEntry entry, string newName)
    {
        entry.VirtualPath = ConvertStoragePathToVirtualPath(entry.VirtualPath);
        DM.UpdateFile(entry, newName);
    }

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    public void SaveTextFile(FileSystemEntry entry, string content, string? encodingName = "utf-8")
    {
        entry.VirtualPath = ConvertStoragePathToVirtualPath(entry.VirtualPath);
        DM.UpdateTextFile(entry, content, encodingName);
    }

    /// <summary>
    /// 更新資料夾擁有者
    /// </summary>
    public void UpdateFolderOwners(FileSystemEntry entry, string[] owners)
    {
        entry.VirtualPath = ConvertStoragePathToVirtualPath(entry.VirtualPath);
        DM.UpdateFolderOwners(entry, owners);
    }

    /// <summary>
    /// 更新項目擁有者
    /// </summary>
    public void UpdateItemOwners(FileSystemEntry entry, string[] owners)
    {
        entry.VirtualPath = ConvertStoragePathToVirtualPath(entry.VirtualPath);
        DM.UpdateItemOwners(entry, owners);
    }

    #endregion

    #region 上傳和下載

    /// <summary>
    /// 上傳檔案（支援單個或批量上傳）
    /// </summary>
    public (bool success, int savedCount, int failedCount, List<string> errors) UploadBatchFiles(string virtualPath, List<IFormFile> files, List<string>? relativePaths = null)
    {
        if (files == null || files.Count == 0)
            return (false, 0, 0, new List<string> { "未選擇檔案" });

        var uploader = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        
        if (relativePaths == null || relativePaths.Count != files.Count)
            relativePaths = files.Select(f => f.FileName).ToList();
        
        var parentOwners = GetParentOwners(virtualPath, uploader);
        var convertedVirtualPath = ConvertStoragePathToVirtualPath(virtualPath);
        
        var savedCount = 0;
        var failedCount = 0;
        var errors = new List<string>();
        
        for (int i = 0; i < files.Count; i++)
        {
            try
            {
                using var fileStream = files[i].OpenReadStream();
                DM.UploadFile(convertedVirtualPath, relativePaths[i], fileStream, parentOwners);
                savedCount++;
            }
            catch (Exception ex)
            {
                failedCount++;
                var fileName = relativePaths[i] ?? files[i].FileName ?? "未知檔案";
                errors.Add($"{fileName}: {ex.Message}");
            }
        }
        
        return (failedCount == 0, savedCount, failedCount, errors);
    }

    /// <summary>
    /// 上傳檔案分塊
    /// </summary>
    public (bool success, int receivedChunks, string message) UploadChunk(string uploadId, int chunkIndex, int totalChunks, IFormFile chunkFile)
    {
        if (chunkFile == null || chunkFile.Length == 0)
            return (false, 0, "分塊檔案為空");

        try
        {
            using var chunkStream = chunkFile.OpenReadStream();
            DM.SaveChunk(uploadId, chunkIndex, chunkStream);
            
            var receivedChunks = DM.GetReceivedChunksCount(uploadId, totalChunks);
            return (true, receivedChunks, "分塊上傳成功");
        }
        catch (Exception ex)
        {
            return (false, 0, $"分塊上傳失敗: {ex.Message}");
        }
    }

    /// <summary>
    /// 合併檔案分塊
    /// </summary>
    public (bool success, string message) MergeChunks(string uploadId, string virtualPath, string fileName, string? relativePath, int totalChunks, long fileSize)
    {
        try
        {
            var uploader = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
            var parentOwners = GetParentOwners(virtualPath, uploader);
            var convertedVirtualPath = ConvertStoragePathToVirtualPath(virtualPath);
            
            var finalRelativePath = relativePath ?? fileName;
            DM.MergeChunks(uploadId, convertedVirtualPath, finalRelativePath, totalChunks, parentOwners);
            
            return (true, "檔案合併成功");
        }
        catch (Exception ex)
        {
            return (false, $"檔案合併失敗: {ex.Message}");
        }
    }

    /// <summary>
    /// 取得父資料夾的擁有者列表
    /// </summary>
    private string[] GetParentOwners(string virtualPath, string? uploader)
    {
        if (string.IsNullOrWhiteSpace(uploader))
            return Array.Empty<string>();

        try
        {
            var root = Path.GetFullPath(configuration["Storage:Root"] ?? "Areas/Backoffice/storage");
            var convertedPath = ConvertStoragePathToVirtualPath(virtualPath);
            var normalized = convertedPath.Replace('\\', '/').Trim();
            if (string.IsNullOrEmpty(normalized) || normalized == "/") normalized = "/";
            if (!normalized.StartsWith('/')) normalized = "/" + normalized;
            
            var relativePath = normalized.TrimStart('/');
            var absDir = Path.GetFullPath(Path.Combine(root, relativePath));
            
            if (!absDir.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return new[] { uploader };
            
            if (Directory.Exists(absDir))
            {
                var owners = DirectoryManagerUtilities.GetAppOwners(absDir);
                return owners.Length > 0 ? owners : new[] { uploader };
            }
        }
        catch
        {
        }
        
        return new[] { uploader };
    }
    
    /// <summary>
    /// 下載檔案
    /// </summary>
    public (Stream Stream, string FileName, string ContentType) DownloadFile(string virtualPath, string fileName)
    {
        var convertedPath = ConvertStoragePathToVirtualPath(virtualPath);
        return DM.OpenRead(convertedPath, fileName);
    }

    /// <summary>
    /// 下載資料夾為 ZIP 檔案
    /// </summary>
    public (Stream Stream, string FileName, string ContentType) DownloadFolderAsZip(string virtualPath, string folderName)
    {
        var convertedPath = ConvertStoragePathToVirtualPath(virtualPath);
        return DM.ZipFolder(convertedPath, folderName);
    }

    /// <summary>
    /// 預覽檔案
    /// </summary>
    public (Stream Stream, string ContentType) PreviewFile(string virtualPath, string fileName)
    {
        var convertedPath = ConvertStoragePathToVirtualPath(virtualPath);
        return DM.OpenRaw(convertedPath, fileName);
    }
    
    /// <summary>
    /// 處理待刪除項目（背景服務）
    /// </summary>
    public (int successCount, int failedCount, List<string> errors) ProcessPendingDeletions(int maxItems = 100)
    {
        var root = Path.GetFullPath(configuration["Storage:Root"] ?? "Areas/Backoffice/storage");
        return DM.ProcessPendingDeletions(root, maxItems);
    }
    
    #endregion

}

