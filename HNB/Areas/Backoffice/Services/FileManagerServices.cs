using HNB.Areas.Backoffice.Utilities;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 檔案管理服務層，負責處理檔案和資料夾的業務邏輯
/// </summary>
public class FileManagerServices(DirectoryManagerUtilities DM)
{

    #region 查詢方法

    public List<vw_file_manager> LoadFileList(string? username = null, string? parentCode = null)
        => DM.LoadFileList(username, parentCode);

    public List<vw_file_manager> LoadAllFolders(string? username = null)
        => DM.LoadAllFolders(username);

    public vw_file_manager? LoadFile(long? id = null, string? code = null)
        => DM.LoadFile(id, code);

    public (string Content, string EncodingName, DateTime? LastWriteUtc) LoadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
        => DM.LoadTextFile(virtualPath, fileName, maxBytes);

    #endregion

    #region 資料夾 CRUD 操作

    public (bool success, string message) CreateFolder(string? parentCode, string folderName)
    {
        try
        {
            DM.CreateFolderByCode(parentCode, folderName);
            return (true, "資料夾已建立");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public (bool success, string message) DeleteFolder(string code)
    {
        try
        {
            DM.DeleteByCode(code);
            return (true, "資料夾已刪除");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public (bool success, string message) RenameFolder(string code, string newName)
    {
        try
        {
            DM.RenameByCode(code, newName);
            return (true, "資料夾已重新命名");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public (bool success, byte[]? zipBytes, string? fileName, string message) DownloadFolderAsZip(string code)
    {
        try
        {
            var (bytes, fileName) = DM.CreateZipFromFolder(code);
            return (true, bytes, fileName, "成功");
        }
        catch (Exception ex)
        {
            return (false, null, null, ex.Message);
        }
    }

    #endregion

    #region 檔案 CRUD 操作

    public (bool success, string message) CreateFile(string? parentCode, string fileName)
    {
        try
        {
            DM.CreateFileByCode(parentCode, fileName);
            return (true, "檔案已建立");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public (bool success, string message) DeleteFile(string code)
    {
        try
        {
            DM.DeleteByCode(code);
            return (true, "檔案已刪除");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public (bool success, string message) RenameFile(string code, string newName)
    {
        try
        {
            DM.RenameByCode(code, newName);
            return (true, "檔案已重新命名");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public (string content, string encoding, DateTime? lastModified) ReadTextFile(string code)
    {
        var file = DM.LoadFile(code: code);
        if (file == null) throw new FileNotFoundException("檔案不存在");
        
        return DM.LoadTextFileByPath(file.file_path!);
    }

    public (bool success, string message) SaveTextFile(string code, string content, string? encodingName = "utf-8")
    {
        try
        {
            var file = DM.LoadFile(code: code);
            if (file == null) throw new FileNotFoundException("檔案不存在");
            
            DM.SaveTextFileByPath(file.file_path!, content, encodingName);
            return (true, "檔案已儲存");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    #endregion

    #region 檔案上傳

    public (bool success, string message, string? safeFileName) UploadSingleFile(string virtualPath, IFormFile file, string username)
    {
        var (safeName, code) = DM.SaveUploadedFile(virtualPath, file, username);
        return (true, "檔案已上傳", safeName);
    }

    public (bool success, int savedCount, int failedCount, List<string> errors) UploadBatchFiles(string virtualPath, List<IFormFile> files, List<string>? relativePaths, string username)
    {
        var (savedCount, failedCount, errors) = DM.SaveBatchUploadedFiles(virtualPath, files, relativePaths, username);
        return (failedCount == 0, savedCount, failedCount, errors);
    }

    #endregion

    #region 檔案下載

    public string? GetFileUrl(string code)
    {
        var file = DM.LoadFile(code: code);
        return file?.url;
    }

    #endregion

    #region 輔助方法
    /// <summary>
    /// 檢查檔案是否可編輯
    /// </summary>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>是否可編輯</returns>
    public static bool IsEditable(string fileName)
        => DirectoryManagerUtilities.IsEditable(fileName);
    #endregion
}

