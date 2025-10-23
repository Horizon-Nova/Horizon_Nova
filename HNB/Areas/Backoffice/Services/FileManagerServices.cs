using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.Backoffice.Dtos;
using Microsoft.AspNetCore.Http;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 檔案管理服務層，負責處理檔案和資料夾的業務邏輯
/// </summary>
public class FileManagerServices(DirectoryManagerUtilities DM, IHttpContextAccessor httpContextAccessor)
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
    public List<FileSystemItem> LoadUserFileSystemItems(string virtualPath, string currentUser)
        => DM.LoadUserFileSystemItems(virtualPath, currentUser);

    /// <summary>
    /// 載入檔案詳細資訊
    /// </summary>
    public FileSystemDetail LoadFileSystemDetail(string virtualPath, string name, string currentUser)
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
    public void CreateFolder(string virtualPath, string folderName)
        => DM.InsertFolder(virtualPath, folderName);
    
    /// <summary>
    /// 刪除資料夾
    /// </summary>
    public void DeleteFolder(string virtualPath, string folderName)
        => DM.DeleteFolder(virtualPath, folderName);
    
    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    public void RenameFolder(string virtualPath, string oldName, string newName)
        => DM.UpdateFolder(virtualPath, oldName, newName);
    
    /// <summary>
    /// 建立空檔案
    /// </summary>
    public void CreateFile(string virtualPath, string fileName)
        => DM.InsertFile(virtualPath, fileName);
    
    /// <summary>
    /// 刪除檔案
    /// </summary>
    public void DeleteFile(string virtualPath, string fileName)
        => DM.DeleteFile(virtualPath, fileName);
    
    /// <summary>
    /// 重新命名檔案
    /// </summary>
    public void RenameFile(string virtualPath, string oldName, string newName)
        => DM.UpdateFile(virtualPath, oldName, newName);
    
    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    public void SaveTextFile(string virtualPath, string fileName, string content, string? encodingName = "utf-8")
        => DM.UpdateTextFile(virtualPath, fileName, content, encodingName);

    /// <summary>
    /// 更新資料夾擁有者
    /// </summary>
    public void UpdateFolderOwners(string virtualPath, string folderName, string[] owners)
        => DM.UpdateFolderOwners(virtualPath, folderName, owners);
    
    #endregion

    #region 上傳和下載
    
    /// <summary>
    /// 上傳單一檔案
    /// </summary>
    public (bool success, string message, string? safeFileName) UploadSingleFile(string virtualPath, IFormFile file)
    {
        var (absPath, safeName) = DM.PrepareSingleUploadTarget(virtualPath, file.FileName);
        
        using (var stream = new FileStream(absPath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
        
        var uploader = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(uploader))
        {
            DirectoryManagerUtilities.SetAppOwners(absPath, new[] { uploader });
        }
        
        return (true, "檔案已上傳", safeName);
    }

    /// <summary>
    /// 批次上傳檔案
    /// </summary>
    public (bool success, int savedCount, int failedCount, List<string> errors) UploadBatchFiles(string virtualPath, List<IFormFile> files, List<string>? relativePaths = null)
    {
        var saved = 0;
        var errors = new List<string>();
        var uploader = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        
        relativePaths ??= files.Select(f => f.FileName).ToList();
        
        if (relativePaths.Count != files.Count)
            return (false, 0, 0, new List<string> { "檔案數量與路徑數量不符" });
        
        for (int i = 0; i < files.Count; i++)
        {
            try
            {
                var (absPath, _, _) = DM.PrepareBatchUploadTarget(virtualPath, relativePaths[i]);
                
                using (var stream = new FileStream(absPath, FileMode.Create))
                {
                    files[i].CopyTo(stream);
                }
                
                if (!string.IsNullOrWhiteSpace(uploader))
                {
                    DirectoryManagerUtilities.SetAppOwners(absPath, new[] { uploader });
                }
                
                saved++;
            }
            catch (Exception ex)
            {
                errors.Add($"{relativePaths[i]}: {ex.Message}");
            }
        }
        
        var failed = errors.Count;
        return (failed == 0, saved, failed, errors);
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

