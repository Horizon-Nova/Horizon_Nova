using HNB.Areas.Backoffice.Utilities;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 檔案管理服務層，負責處理檔案和資料夾的業務邏輯
/// </summary>
public class FileManagerServices(DirectoryManagerUtilities DM)
{
    #region 統一的 ViewBag 模型設置
    /// <summary>
    /// 設置 ViewBag 模型資料（從資料庫讀取）
    /// </summary>
    public void ViewBagModel(dynamic viewBag, string? parentCode = null, string? username = null)
    {
        viewBag.CurrentParentCode = parentCode;
        
        var files = DM.LoadFileList(username, parentCode);
        
        viewBag.Folders = files.Where(f => f.item_type == "folder").ToList();
        viewBag.Files = files.Where(f => f.item_type == "file").ToList();
        
        viewBag.FolderCount = viewBag.Folders.Count;
        viewBag.FileCount = viewBag.Files.Count;
        viewBag.TotalSize = files.Where(f => f.item_type == "file").Sum(f => f.file_size ?? 0);
    }
    #endregion

    #region 查詢方法（從資料庫）

    public List<vw_file_manager> LoadFileList(string? username = null, string? parentCode = null)
        => DM.LoadFileList(username, parentCode);

    public vw_file_manager? LoadFile(long? id = null, string? code = null)
        => DM.LoadFile(id, code);

    public (string Content, string EncodingName, DateTime? LastWriteUtc) LoadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
        => DM.LoadTextFile(virtualPath, fileName, maxBytes);

    #endregion

    #region 資料夾 CRUD 操作
    /// <summary>
    /// 建立資料夾
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="folderName">資料夾名稱</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) CreateFolder(string virtualPath, string folderName)
    {
        DM.CreateFolder(virtualPath, folderName);
        return (true, "資料夾已建立");
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="folderName">資料夾名稱</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) DeleteFolder(string virtualPath, string folderName)
    {
        DM.DeleteFolder(virtualPath, folderName);
        return (true, "資料夾已刪除");
    }

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="oldName">舊資料夾名</param>
    /// <param name="newName">新資料夾名</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) RenameFolder(string virtualPath, string oldName, string newName)
    {
        DM.RenameFolder(virtualPath, oldName, newName);
        return (true, "資料夾已重新命名");
    }
    #endregion

    #region 檔案 CRUD 操作
    /// <summary>
    /// 建立空檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) CreateFile(string virtualPath, string fileName)
    {
        DM.CreateEmptyFile(virtualPath, fileName);
        return (true, "檔案已建立");
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) DeleteFile(string virtualPath, string fileName)
    {
        DM.DeleteFile(virtualPath, fileName);
        return (true, "檔案已刪除");
    }

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="oldName">舊檔名</param>
    /// <param name="newName">新檔名</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) RenameFile(string virtualPath, string oldName, string newName)
    {
        DM.RenameFile(virtualPath, oldName, newName);
        return (true, "檔案已重新命名");
    }

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <param name="content">檔案內容</param>
    /// <param name="encodingName">編碼名稱</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) SaveTextFile(string virtualPath, string fileName, string content, string? encodingName = "utf-8")
    {
        DM.SaveTextFile(virtualPath, fileName, content, encodingName);
        return (true, "檔案已儲存");
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

