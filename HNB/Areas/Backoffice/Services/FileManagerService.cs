using HNB.Areas.Backoffice.Utilities;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 檔案管理器服務層，負責處理檔案和資料夾的業務邏輯
/// 遵循現有的架構模式：統一的查詢來源 + 功能分組
/// </summary>
public class FileManagerService(IConfiguration cfg, DirectoryManagerUtilities dm)
{
    #region 統一的查詢來源
    /// <summary>
    /// 標準化路徑
    /// </summary>
    public string NormalizePath(string? path) => dm.NormalizePath(path);
    
    /// <summary>
    /// 檢查是否可以在指定路徑新增檔案/資料夾
    /// </summary>
    public bool CanAddHere(string virtualPath) => !dm.IsProtected(virtualPath);
    
    /// <summary>
    /// 建立目錄樹結構
    /// </summary>
    public List<(string Name, string VirtualPath, int Depth)> BuildTree() => dm.BuildTree();
    
    /// <summary>
    /// 建立麵包屑導航
    /// </summary>
    public List<(string Name, string VirtualPath)> BuildBreadcrumb(string virtualPath) => dm.BuildBreadcrumb(virtualPath);
    
    /// <summary>
    /// 列出資料夾
    /// </summary>
    public List<(string Name, DateTime? LastWriteUtc)> ListFolders(string virtualPath) => dm.ListFolders(virtualPath);
    
    /// <summary>
    /// 列出檔案
    /// </summary>
    public List<(string Name, long Size, DateTime? LastWriteUtc)> ListFiles(string virtualPath) => dm.ListFiles(virtualPath);
    #endregion

    #region 檔案上傳功能
    /// <summary>
    /// 上傳單一檔案
    /// </summary>
    public async Task<(bool Success, string Message, string? FileName)> UploadAsync(string virtualPath, IFormFile file, CancellationToken ct = default)
    {
        try
        {
            var (absFile, safeFileName) = dm.PrepareSingleUploadTarget(virtualPath, file.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(absFile)!);
            await using var fs = File.Create(absFile);
            await file.CopyToAsync(fs, ct);
            return (true, "上傳成功", safeFileName);
        }
        catch (Exception ex)
        {
            return (false, $"上傳失敗：{ex.Message}", null);
        }
    }

    /// <summary>
    /// 批量上傳檔案
    /// </summary>
    public async Task<(bool Success, string Message, int UploadedCount, List<string> FileNames)> UploadManyAsync(string virtualPath, IReadOnlyList<IFormFile> files, CancellationToken ct = default)
    {
        if (files is null || files.Count == 0) 
            return (true, "沒有檔案需要上傳", 0, new List<string>());

        var uploadedFiles = new List<string>();
        var errors = new List<string>();

        foreach (var f in files)
        {
            try
            {
                var (absFile, safeFileName, _) = dm.PrepareBatchUploadTarget(virtualPath, f.FileName ?? "");
                Directory.CreateDirectory(Path.GetDirectoryName(absFile)!);
                await using var fs = File.Create(absFile);
                await f.CopyToAsync(fs, ct);
                uploadedFiles.Add(safeFileName);
            }
            catch (Exception ex)
            {
                errors.Add($"{f.FileName}: {ex.Message}");
            }
        }

        var success = uploadedFiles.Count > 0;
        var message = success 
            ? $"成功上傳 {uploadedFiles.Count} 個檔案" + (errors.Count > 0 ? $"，{errors.Count} 個失敗" : "")
            : "所有檔案上傳失敗";

        return (success, message, uploadedFiles.Count, uploadedFiles);
    }
    #endregion

    #region 檔案和資料夾管理功能
    /// <summary>
    /// 建立資料夾
    /// </summary>
    public (bool Success, string Message) CreateFolder(string virtualPath, string folderName)
    {
        try
        {
            dm.CreateFolder(virtualPath, folderName);
            return (true, "資料夾建立成功");
        }
        catch (Exception ex)
        {
            return (false, $"建立資料夾失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 建立空檔案
    /// </summary>
    public (bool Success, string Message) CreateEmptyFile(string virtualPath, string fileName)
    {
        try
        {
            dm.CreateEmptyFile(virtualPath, fileName);
            return (true, "檔案建立成功");
        }
        catch (Exception ex)
        {
            return (false, $"建立檔案失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    public (bool Success, string Message) DeleteFile(string virtualPath, string fileName)
    {
        try
        {
            dm.DeleteFile(virtualPath, fileName);
            return (true, "檔案刪除成功");
        }
        catch (Exception ex)
        {
            return (false, $"刪除檔案失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    public (bool Success, string Message) DeleteFolder(string virtualPath, string folderName)
    {
        try
        {
            dm.DeleteFolder(virtualPath, folderName);
            return (true, "資料夾刪除成功");
        }
        catch (Exception ex)
        {
            return (false, $"刪除資料夾失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    public (bool Success, string Message) RenameFile(string virtualPath, string oldName, string newName)
    {
        try
        {
            dm.RenameFile(virtualPath, oldName, newName);
            return (true, "檔案重新命名成功");
        }
        catch (Exception ex)
        {
            return (false, $"重新命名檔案失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    public (bool Success, string Message) RenameFolder(string virtualPath, string oldName, string newName)
    {
        try
        {
            dm.RenameFolder(virtualPath, oldName, newName);
            return (true, "資料夾重新命名成功");
        }
        catch (Exception ex)
        {
            return (false, $"重新命名資料夾失敗：{ex.Message}");
        }
    }
    #endregion

    #region 檔案讀取和下載功能
    /// <summary>
    /// 開啟檔案讀取
    /// </summary>
    public (Stream Stream, string FileName, string ContentType) OpenRead(string virtualPath, string fileName)
        => dm.OpenRead(virtualPath, fileName);

    /// <summary>
    /// 開啟原始檔案
    /// </summary>
    public (Stream Stream, string ContentType) OpenRaw(string virtualPath, string fileName)
        => dm.OpenRaw(virtualPath, fileName);

    /// <summary>
    /// 壓縮資料夾為ZIP
    /// </summary>
    public (Stream Stream, string FileName, string ContentType) ZipFolder(string virtualPath, string folderName)
        => dm.ZipFolder(virtualPath, folderName);
    #endregion

    #region 文字檔案編輯功能
    /// <summary>
    /// 讀取文字檔案
    /// </summary>
    public (string Content, string EncodingName, DateTime? LastWriteUtc) ReadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
        => dm.ReadTextFile(virtualPath, fileName, maxBytes);

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    public (bool Success, string Message) SaveTextFile(string virtualPath, string fileName, string content, string? encodingName = "utf-8")
    {
        try
        {
            dm.SaveTextFile(virtualPath, fileName, content, encodingName);
            return (true, "檔案儲存成功");
        }
        catch (Exception ex)
        {
            return (false, $"儲存檔案失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 檢查檔案是否可編輯
    /// </summary>
    public bool IsEditableFile(string fileName) => DirectoryManagerUtilities.IsEditable(fileName);
    #endregion

    #region 統一的ViewBag設定
    /// <summary>
    /// 設定檔案管理器統一的ViewBag資料
    /// </summary>
    /// <param name="viewBag">ViewBag 物件</param>
    /// <param name="currentPath">當前路徑</param>
    public void ViewBagModel(dynamic viewBag, string currentPath = "/")
    {
        var vPath = NormalizePath(currentPath);
        viewBag.CurrentPath = vPath;
        viewBag.CanAddHere = CanAddHere(vPath);
        viewBag.Breadcrumb = BuildBreadcrumb(vPath);
        viewBag.Tree = BuildTree();
        viewBag.Folders = ListFolders(vPath);
        viewBag.Files = ListFiles(vPath).Select(f => new FileItemViewModel { 
            Name = f.Name, 
            Size = f.Size, 
            LastWriteUtc = f.LastWriteUtc, 
            FormattedSize = FormatFileSize(f.Size) 
        }).ToList();
    }
    #endregion

    #region 檔案大小格式化工具
    /// <summary>
    /// 格式化檔案大小
    /// </summary>
    /// <param name="bytes">檔案大小（位元組）</param>
    /// <returns>格式化後的大小字串</returns>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
    #endregion
}

/// <summary>
/// 檔案項目ViewModel
/// </summary>
public class FileItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime? LastWriteUtc { get; set; }
    public string FormattedSize { get; set; } = string.Empty;
}
