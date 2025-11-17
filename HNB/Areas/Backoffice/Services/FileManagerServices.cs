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
                throw new InvalidOperationException("路徑不安全");
            
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
    public FileSystemEntry LoadFileSystemDetail(string virtualPath, string name, string currentUser)
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
    /// 載入用戶有權限的目錄樹
    /// </summary>
    public List<object> LoadUserTree(string currentUser)
    {
        var tree = new List<object>();
        var userStoragePath = GetUserStoragePath(currentUser);

        void BuildTree(string virtualPath, int depth = 0)
        {
            if (depth > 5) return;

            var items = LoadUserFileSystemItems(virtualPath, currentUser);
            var folders = items.Where(f => f.Type == "folder").ToList();

            foreach (var folder in folders)
            {
                var folderPath = virtualPath == userStoragePath ? $"{userStoragePath}/{folder.Name}" : $"{virtualPath}/{folder.Name}";

                tree.Add(new
                {
                    name = folder.Name,
                    path = folderPath,
                    type = "folder",
                    depth = depth,
                    hasChildren = true
                });

                BuildTree(folderPath, depth + 1);
            }
        }

        BuildTree(userStoragePath);
        return tree;
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
    /// <param name="virtualPath">表單欄位：virtualPath（目標虛擬路徑）</param>
    /// <param name="files">表單欄位：files（檔案清單）</param>
    /// <param name="relativePaths">表單欄位：relativePaths（可選；每個檔案的相對路徑，用於保留資料夾結構）</param>
    /// <remarks>
    /// 如果未提供 relativePaths 或數量不符，會自動使用檔案名稱（不保留資料夾結構）
    /// </remarks>
    public (bool success, int savedCount, int failedCount, List<string> errors) UploadBatchFiles(string virtualPath, List<IFormFile> files, List<string>? relativePaths = null)
    {
        var uploader = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        
        if (relativePaths == null || relativePaths.Count != files.Count)
            relativePaths = files.Select(f => f.FileName).ToList();
        
        var parentOwners = new List<string>();
        if (!string.IsNullOrWhiteSpace(uploader))
        {
            try
            {
                var convertedPath = ConvertStoragePathToVirtualPath(virtualPath);
                var parentItems = DM.LoadUserFileSystemItems(convertedPath, uploader);
                
                var parentAbsPath = GetParentDirectoryAbsolutePath(virtualPath);
                if (!string.IsNullOrEmpty(parentAbsPath) && Directory.Exists(parentAbsPath))
                {
                    var owners = DirectoryManagerUtilities.GetAppOwners(parentAbsPath);
                    parentOwners = owners.Length > 0 ? owners.ToList() : new List<string> { uploader };
                }
                else
                {
                    parentOwners = new List<string> { uploader };
                }
            }
            catch
            {
                parentOwners = new List<string> { uploader };
            }
        }
        
        string? GetParentDirectoryAbsolutePath(string vPath)
        {
            try
            {
                var convertedPath = ConvertStoragePathToVirtualPath(vPath);
                var (tempPath, _, _) = DM.PrepareBatchUploadTarget(convertedPath, "temp.tmp");
                return Path.GetDirectoryName(tempPath);
            }
            catch
            {
                return null;
            }
        }
        
        var convertedVirtualPath = ConvertStoragePathToVirtualPath(virtualPath);
        for (int i = 0; i < files.Count; i++)
        {
            var (absPath, _, _) = DM.PrepareBatchUploadTarget(convertedVirtualPath, relativePaths[i]);
            
            using (var stream = new FileStream(absPath, FileMode.Create))
            {
                files[i].CopyTo(stream);
            }
            
            if (parentOwners.Count > 0)
            {
                DirectoryManagerUtilities.SetAppOwners(absPath, parentOwners.ToArray());
            }
            else if (!string.IsNullOrWhiteSpace(uploader))
            {
                DirectoryManagerUtilities.SetAppOwners(absPath, new[] { uploader });
            }
        }
        
        return (true, files.Count, 0, new List<string>());
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
    
    #endregion

}

