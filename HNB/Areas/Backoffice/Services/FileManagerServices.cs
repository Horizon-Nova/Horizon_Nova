using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.Backoffice.Repositories;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 檔案管理服務層，負責處理檔案和資料夾的業務邏輯
/// </summary>
public class FileManagerServices(DirectoryManagerUtilities DM, FileManagerRepository repo, IHttpContextAccessor httpContextAccessor)
{
    #region 統一的 ViewBag 設置
    /// <summary>
    /// 設置 ViewBag（從資料庫讀取，根據當前用戶權限過濾）
    /// </summary>
    public void ViewBagModel(dynamic viewBag, string virtualPath = "/")
    {
        var safePath = virtualPath ?? "/";
        var currentUsername = httpContextAccessor.HttpContext?.User?.Identity?.Name;
        
        viewBag.CurrentPath = safePath;
        viewBag.Breadcrumb = DM.LoadBreadcrumb(safePath) ?? new List<(string, string)>();
        
        var allFolders = repo.QueryFileManagerList(currentUsername: currentUsername, itemType: "folder");
        var allFiles = repo.QueryFileManagerList(currentUsername: currentUsername, itemType: "file");
        
        viewBag.Tree = allFolders
            .Select(f => (f.file_name, f.file_path + "/" + f.file_name, CalculateDepth(f.file_path ?? "/")))
            .OrderBy(t => t.Item2)
            .ToList();
        
        viewBag.Folders = repo.QueryFileManagerList(currentUsername: currentUsername, virtualPath: safePath, itemType: "folder")
            .Select(f => (f.file_name, f.updated_at ?? f.created_at))
            .ToList();
        
        viewBag.Files = repo.QueryFileManagerList(currentUsername: currentUsername, virtualPath: safePath, itemType: "file")
            .Select(f => (f.file_name, f.file_size ?? 0L, f.updated_at ?? f.created_at))
            .ToList();
        
        var foldersInPath = repo.QueryFileManagerList(currentUsername: currentUsername, virtualPath: safePath, itemType: "folder");
        var filesInPath = repo.QueryFileManagerList(currentUsername: currentUsername, virtualPath: safePath, itemType: "file");
        
        viewBag.FolderCount = foldersInPath.Count;
        viewBag.FileCount = filesInPath.Count;
        viewBag.TotalSize = filesInPath.Sum(f => f.file_size ?? 0L);
        viewBag.LastModified = filesInPath.Any() || foldersInPath.Any()
            ? filesInPath.Select(f => f.updated_at ?? f.created_at)
                .Concat(foldersInPath.Select(f => f.updated_at ?? f.created_at))
                .Where(d => d.HasValue)
                .Max()
            : (DateTime?)null;
    }
    
    private int CalculateDepth(string path)
    {
        if (path == "/") return 0;
        return path.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
    }
    #endregion

    #region 查詢方法
    /// <summary>
    /// 載入資料夾清單
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <returns>資料夾清單</returns>
    public List<(string Name, DateTime? LastWriteUtc)> LoadFolders(string virtualPath)
        => DM.LoadFolders(virtualPath);

    /// <summary>
    /// 載入檔案清單
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <returns>檔案清單</returns>
    public List<(string Name, long Size, DateTime? LastWriteUtc)> LoadFiles(string virtualPath)
        => DM.LoadFiles(virtualPath);

    /// <summary>
    /// 載入麵包屑導航
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <returns>麵包屑清單</returns>
    public List<(string Name, string VirtualPath)> LoadBreadcrumb(string virtualPath)
        => DM.LoadBreadcrumb(virtualPath);

    /// <summary>
    /// 載入目錄樹結構
    /// </summary>
    /// <returns>目錄樹清單</returns>
    public List<(string Name, string VirtualPath, int Depth)> LoadTree()
        => DM.LoadTree();

    /// <summary>
    /// 載入文字檔案內容
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <param name="maxBytes">最大位元組數</param>
    /// <returns>檔案內容、編碼名稱和最後修改時間</returns>
    public (string Content, string EncodingName, DateTime? LastWriteUtc) LoadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
        => DM.LoadTextFile(virtualPath, fileName, maxBytes);

    /// <summary>
    /// 查詢目錄統計資訊
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <returns>統計資訊</returns>
    public (int FolderCount, int FileCount, long TotalSize, DateTime? LastModified) QueryStatistics(string virtualPath)
        => DM.QueryStatistics(virtualPath);
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
        try
        {
            var currentUsername = httpContextAccessor.HttpContext?.User?.Identity?.Name;
            DM.CreateFolder(virtualPath, folderName, currentUsername);
            return (true, "資料夾已建立");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="folderName">資料夾名稱</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) DeleteFolder(string virtualPath, string folderName)
    {
        try
        {
            // 記錄刪除操作（用於除錯）
            Console.WriteLine($"[刪除資料夾] 路徑: {virtualPath}, 名稱: {folderName}");
            
            DM.DeleteFolder(virtualPath, folderName);
            
            Console.WriteLine($"[刪除成功] 已刪除資料夾: {folderName}");
            return (true, "資料夾已刪除");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[刪除失敗] 錯誤: {ex.Message}");
            return (false, ex.Message);
        }
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
        try
        {
            DM.RenameFolder(virtualPath, oldName, newName);
            return (true, "資料夾已重新命名");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
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
        try
        {
            var currentUsername = httpContextAccessor.HttpContext?.User?.Identity?.Name;
            DM.CreateEmptyFile(virtualPath, fileName, currentUsername);
            return (true, "檔案已建立");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) DeleteFile(string virtualPath, string fileName)
    {
        try
        {
            // 記錄刪除操作（用於除錯）
            Console.WriteLine($"[刪除檔案] 路徑: {virtualPath}, 名稱: {fileName}");
            
            DM.DeleteFile(virtualPath, fileName);
            
            Console.WriteLine($"[刪除成功] 已刪除檔案: {fileName}");
            return (true, "檔案已刪除");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[刪除失敗] 錯誤: {ex.Message}");
            return (false, ex.Message);
        }
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
        try
        {
            DM.RenameFile(virtualPath, oldName, newName);
            return (true, "檔案已重新命名");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
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
        try
        {
            DM.SaveTextFile(virtualPath, fileName, content, encodingName);
            return (true, "檔案已儲存");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
    #endregion

    #region 檔案上傳
    /// <summary>
    /// 上傳單一檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="file">上傳檔案</param>
    /// <returns>操作結果</returns>
    public (bool success, string message, string? safeFileName) UploadSingleFile(string virtualPath, IFormFile file)
    {
        try
        {
            var (absPath, safeName) = DM.PrepareSingleUploadTarget(virtualPath, file.FileName);
            
            using var stream = new FileStream(absPath, FileMode.Create);
            file.CopyTo(stream);
            
            return (true, "檔案已上傳", safeName);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    /// <summary>
    /// 批量上傳檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="files">上傳檔案清單</param>
    /// <param name="relativePaths">相對路徑清單（支援資料夾結構）</param>
    /// <returns>操作結果</returns>
    public (bool success, int savedCount, int failedCount, List<string> errors) UploadBatchFiles(string virtualPath, List<IFormFile> files, List<string>? relativePaths = null)
    {
        var saved = 0;
        var errors = new List<string>();
        
        relativePaths ??= files.Select(f => f.FileName).ToList();
        
        if (relativePaths.Count != files.Count)
        {
            return (false, 0, 0, new List<string> { "檔案數量與路徑數量不符" });
        }
        
        for (int i = 0; i < files.Count; i++)
        {
            try
            {
                var (absPath, _, _) = DM.PrepareBatchUploadTarget(virtualPath, relativePaths[i]);
                
                using var stream = new FileStream(absPath, FileMode.Create);
                files[i].CopyTo(stream);
                
                saved++;
            }
            catch (Exception ex)
            {
                errors.Add($"{files[i].FileName}: {ex.Message}");
            }
        }
        
        var failed = errors.Count;
        return (failed == 0, saved, failed, errors);
    }
    #endregion

    #region 檔案下載
    /// <summary>
    /// 下載檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>檔案串流、檔名和內容類型</returns>
    public (Stream Stream, string FileName, string ContentType) DownloadFile(string virtualPath, string fileName)
        => DM.OpenRead(virtualPath, fileName);

    /// <summary>
    /// 下載資料夾（ZIP）
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="folderName">資料夾名稱</param>
    /// <returns>ZIP串流、檔名和內容類型</returns>
    public (Stream Stream, string FileName, string ContentType) DownloadFolderAsZip(string virtualPath, string folderName)
        => DM.ZipFolder(virtualPath, folderName);

    /// <summary>
    /// 預覽檔案（用於圖片、影片等）
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>檔案串流和內容類型</returns>
    public (Stream Stream, string ContentType) PreviewFile(string virtualPath, string fileName)
        => DM.OpenRaw(virtualPath, fileName);
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

