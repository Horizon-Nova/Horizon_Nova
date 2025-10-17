using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.Backoffice.Repositories;
using Microsoft.AspNetCore.Http;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 檔案管理服務層，負責處理檔案和資料夾的業務邏輯
/// </summary>
public class FileManagerServices(DirectoryManagerUtilities DM, FileManagerRepository repo, IHttpContextAccessor httpContextAccessor)
{
    #region 查詢方法
    
    public List<file_manager> LoadFileManagerList(file_manager? filter = null, string? currentUsername = null)
        => repo.QueryFileManagerList(filter, currentUsername);
    
    public file_manager? LoadFileManager(file_manager filter)
        => repo.QueryFileManager(filter);
    
    public List<vw_file_manager> LoadVWFileManagerList(vw_file_manager? filter = null, string? currentUsername = null)
        => repo.QueryVWFileManagerList(filter, currentUsername);
    
    public vw_file_manager? LoadVWFileManager(vw_file_manager filter)
        => repo.QueryVWFileManager(filter);
    
    public (string Content, string EncodingName, DateTime? LastWriteUtc) LoadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
        => DM.LoadTextFile(virtualPath, fileName, maxBytes);
        
    #endregion

    #region 基本 CRUD 操作
    
    public void CreateFolder(string virtualPath, string folderName, List<string>? sharedUsers = null)
    {
        // 先创建文件夹
        DM.CreateFolder(virtualPath, folderName);
        
        // 同步到数据库
        // 將當前登入者作為預設共享使用者，如果沒指定 sharedUsers
        var defaultUser = (sharedUsers != null && sharedUsers.Any()) ? null : httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        DM.SyncAllFilesToDatabase(defaultUser);
        
        // 設置共享使用者
        if (sharedUsers != null && sharedUsers.Any())
        {
            var filter = new file_manager { file_path = virtualPath, file_name = folderName };
            var entity = repo.QueryFileManager(filter);
            if (entity != null)
            {
                entity.shared_users = sharedUsers;
                repo.UpdateFileManager(entity);
            }
        }
    }
    
    public void DeleteFolder(string virtualPath, string folderName)
    {
        DM.DeleteFolder(virtualPath, folderName);
        DM.SyncAllFilesToDatabase();
    }
    
    public void RenameFolder(string virtualPath, string oldName, string newName)
    {
        DM.RenameFolder(virtualPath, oldName, newName);
        DM.SyncAllFilesToDatabase();
    }
    
    public void CreateFile(string virtualPath, string fileName, List<string>? sharedUsers = null)
    {
        // 先创建文件
        DM.CreateEmptyFile(virtualPath, fileName);
        
        // 同步后再更新 shared_users（若未指定，帶入當前使用者作為預設共享）
        var defaultUserForFile = (sharedUsers != null && sharedUsers.Any()) ? null : httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        DM.SyncAllFilesToDatabase(defaultUserForFile);
        
        // 設置共享使用者
        if (sharedUsers != null && sharedUsers.Any())
        {
            var filter = new file_manager { file_path = virtualPath, file_name = fileName };
            var entity = repo.QueryFileManager(filter);
            if (entity != null)
            {
                entity.shared_users = sharedUsers;
                repo.UpdateFileManager(entity);
            }
        }
    }
    
    public void DeleteFile(string virtualPath, string fileName)
    {
        DM.DeleteFile(virtualPath, fileName);
        DM.SyncAllFilesToDatabase();
    }
    
    public void RenameFile(string virtualPath, string oldName, string newName)
    {
        DM.RenameFile(virtualPath, oldName, newName);
        DM.SyncAllFilesToDatabase();
    }
    
    public void SaveTextFile(string virtualPath, string fileName, string content, string? encodingName = "utf-8")
    {
        DM.SaveTextFile(virtualPath, fileName, content, encodingName);
        DM.SyncAllFilesToDatabase();
    }
    
    #endregion


    #region 輔助方法
    
    public (bool success, string message, string? safeFileName) UploadSingleFile(string virtualPath, IFormFile file)
    {
        var (absPath, safeName) = DM.PrepareSingleUploadTarget(virtualPath, file.FileName);
        
        using var stream = new FileStream(absPath, FileMode.Create);
        file.CopyTo(stream);
        
        var defaultUser = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        DM.SyncAllFilesToDatabase(defaultUser);
        
        return (true, "檔案已上傳", safeName);
    }

    public (bool success, int savedCount, int failedCount, List<string> errors) UploadBatchFiles(string virtualPath, List<IFormFile> files, List<string>? relativePaths = null)
    {
        var saved = 0;
        var errors = new List<string>();
        
        relativePaths ??= files.Select(f => f.FileName).ToList();
        
        if (relativePaths.Count != files.Count)
            return (false, 0, 0, new List<string> { "檔案數量與路徑數量不符" });
        
        for (int i = 0; i < files.Count; i++)
        {
            var (absPath, _, _) = DM.PrepareBatchUploadTarget(virtualPath, relativePaths[i]);
            
            using var stream = new FileStream(absPath, FileMode.Create);
            files[i].CopyTo(stream);
            
            saved++;
        }
        
        var defaultUser = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        DM.SyncAllFilesToDatabase(defaultUser);
        
        var failed = errors.Count;
        return (failed == 0, saved, failed, errors);
    }
    
    public (Stream Stream, string FileName, string ContentType) DownloadFile(string virtualPath, string fileName)
        => DM.OpenRead(virtualPath, fileName);

    public (Stream Stream, string FileName, string ContentType) DownloadFolderAsZip(string virtualPath, string folderName)
        => DM.ZipFolder(virtualPath, folderName);

    public (Stream Stream, string ContentType) PreviewFile(string virtualPath, string fileName)
        => DM.OpenRaw(virtualPath, fileName);
    
    public static bool IsEditable(string fileName)
        => DirectoryManagerUtilities.IsEditable(fileName);
    
    #endregion
}

