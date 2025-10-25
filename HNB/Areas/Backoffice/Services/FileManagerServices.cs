using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.Backoffice.Dtos;
using Microsoft.AspNetCore.Http;
using HNB.Areas.Backoffice.Services; // for PermissionManagementService

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 檔案管理服務層，負責處理檔案和資料夾的業務邏輯
/// </summary>
public class FileManagerServices(DirectoryManagerUtilities DM, IHttpContextAccessor httpContextAccessor, PermissionManagementService permissionService)
{
    #region 查詢方法

    /// <summary>
    /// 載入文字檔案內容
    /// </summary>
    public (string Content, string EncodingName, DateTime? LastWriteUtc) LoadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
        => DM.LoadTextFile(virtualPath, fileName, maxBytes);


    /// <summary>
    /// 載入用戶有權限的檔案系統項目
    /// </summary>
    public List<FileSystemEntry> LoadUserFileSystemItems(string virtualPath, string currentUser)
        => DM.LoadUserFileSystemItems(virtualPath, currentUser);

    /// <summary>
    /// 載入檔案詳細資訊
    /// </summary>
    public FileSystemEntry LoadFileSystemDetail(string virtualPath, string name, string currentUser)
        => DM.LoadFileSystemDetail(virtualPath, name, currentUser);


    /// <summary>
    /// 載入用戶有權限的目錄樹
    /// </summary>
    public List<object> LoadUserTree(string currentUser)
    {
        var tree = new List<object>();

        void BuildTree(string virtualPath, int depth = 0)
        {
            if (depth > 5) return;

            var items = LoadUserFileSystemItems(virtualPath, currentUser);
            var folders = items.Where(f => f.Type == "folder").ToList();

            foreach (var folder in folders)
            {
                var folderPath = virtualPath == "/" ? $"/{folder.Name}" : $"{virtualPath}/{folder.Name}";

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

        BuildTree("/");
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
        => DM.InsertFolder(entry);

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    public void DeleteFolder(FileSystemEntry entry)
        => DM.DeleteFolder(entry);

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    public void RenameFolder(FileSystemEntry entry, string newName)
        => DM.UpdateFolder(entry, newName);

    /// <summary>
    /// 建立空檔案
    /// </summary>
    public void CreateFile(FileSystemEntry entry)
        => DM.InsertFile(entry);

    /// <summary>
    /// 刪除檔案
    /// </summary>
    public void DeleteFile(FileSystemEntry entry)
        => DM.DeleteFile(entry);

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    public void RenameFile(FileSystemEntry entry, string newName)
        => DM.UpdateFile(entry, newName);

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    public void SaveTextFile(FileSystemEntry entry, string content, string? encodingName = "utf-8")
        => DM.UpdateTextFile(entry, content, encodingName);

    /// <summary>
    /// 更新資料夾擁有者
    /// </summary>
    public void UpdateFolderOwners(FileSystemEntry entry, string[] owners)
        => DM.UpdateFolderOwners(entry, owners);

    /// <summary>
    /// 更新項目擁有者
    /// </summary>
    public void UpdateItemOwners(FileSystemEntry entry, string[] owners)
        => DM.UpdateItemOwners(entry, owners);

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
        
        for (int i = 0; i < files.Count; i++)
        {
            var (absPath, _, _) = DM.PrepareBatchUploadTarget(virtualPath, relativePaths[i]);
            
            using (var stream = new FileStream(absPath, FileMode.Create))
            {
                files[i].CopyTo(stream);
            }
            
            // 添加擁有者
            if (!string.IsNullOrWhiteSpace(uploader))
                DirectoryManagerUtilities.SetAppOwners(absPath, new[] { uploader });
        }
        
        return (true, files.Count, 0, new List<string>());
    }
    
    /// <summary>
    /// 下載檔案
    /// </summary>
    public (Stream Stream, string FileName, string ContentType) DownloadFile(string virtualPath, string fileName)
        => DM.OpenRead(virtualPath, fileName);

    /// <summary>
    /// 下載資料夾為 ZIP 檔案
    /// </summary>
    public (Stream Stream, string FileName, string ContentType) DownloadFolderAsZip(string virtualPath, string folderName)
        => DM.ZipFolder(virtualPath, folderName);

    /// <summary>
    /// 預覽檔案
    /// </summary>
    public (Stream Stream, string ContentType) PreviewFile(string virtualPath, string fileName)
        => DM.OpenRaw(virtualPath, fileName);
    
    #endregion

    #region ViewBag 設定方法

    /// <summary>
    /// 設定檔案管理統一的 ViewBag 資料
    /// </summary>
    public void ViewBagModel(dynamic viewBag, string path, string currentUser)
    {
        viewBag.CurrentPath = path ?? "/";
        viewBag.CurrentUser = currentUser;
        viewBag.Tree = LoadUserTree(currentUser);
        
        var items = LoadUserFileSystemItems(path, currentUser);
        viewBag.Items = items;
        
        var folders = items.Where(f => f.Type == "folder").ToList();
        var files = items.Where(f => f.Type == "file").ToList();
        
        viewBag.FolderCount = folders.Count;
        viewBag.FileCount = files.Count;
        viewBag.TotalSize = files.Sum(f => f.Size ?? 0L);
    }

    #endregion
}

