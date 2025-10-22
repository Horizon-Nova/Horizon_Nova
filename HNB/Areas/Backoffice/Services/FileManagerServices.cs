using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.Backoffice.Models;
using Microsoft.AspNetCore.Http;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 檔案管理服務層，負責處理檔案和資料夾的業務邏輯
/// </summary>
public class FileManagerServices(DirectoryManagerUtilities DM, IHttpContextAccessor httpContextAccessor)
{
    #region 查詢方法
    
    public (string Content, string EncodingName, DateTime? LastWriteUtc) LoadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
        => DM.LoadTextFile(virtualPath, fileName, maxBytes);

    public List<FileSystemItem> LoadFileSystemItems(string virtualPath)
        => DM.LoadFileSystemItems(virtualPath);

    public List<(string Name, string VirtualPath, int Depth)> LoadTree()
        => DM.LoadTree();
        
    #endregion

    #region 基本 CRUD 操作
    
    public void CreateFolder(string virtualPath, string folderName)
        => DM.CreateFolder(virtualPath, folderName);
    
    public void DeleteFolder(string virtualPath, string folderName)
        => DM.DeleteFolder(virtualPath, folderName);
    
    public void RenameFolder(string virtualPath, string oldName, string newName)
        => DM.RenameFolder(virtualPath, oldName, newName);
    
    public void CreateFile(string virtualPath, string fileName)
        => DM.CreateEmptyFile(virtualPath, fileName);
    
    public void DeleteFile(string virtualPath, string fileName)
        => DM.DeleteFile(virtualPath, fileName);
    
    public void RenameFile(string virtualPath, string oldName, string newName)
        => DM.RenameFile(virtualPath, oldName, newName);
    
    public void SaveTextFile(string virtualPath, string fileName, string content, string? encodingName = "utf-8")
        => DM.SaveTextFile(virtualPath, fileName, content, encodingName);
    
    #endregion


    #region 上傳和下載
    
    public (bool success, string message, string? safeFileName) UploadSingleFile(string virtualPath, IFormFile file)
    {
        var (absPath, safeName) = DM.PrepareSingleUploadTarget(virtualPath, file.FileName);
        
        using (var stream = new FileStream(absPath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
        
        // 設定擁有者（上傳者永遠在第一位）
        var uploader = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(uploader))
        {
            DirectoryManagerUtilities.UpsertAppOwnersWithActorFirst(absPath, uploader);
        }
        
        return (true, "檔案已上傳", safeName);
    }

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
                
                // 設定擁有者（上傳者永遠在第一位）
                if (!string.IsNullOrWhiteSpace(uploader))
                {
                    DirectoryManagerUtilities.UpsertAppOwnersWithActorFirst(absPath, uploader);
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

