using HNB.Areas.Backoffice.Utilities;

namespace HNB.Areas.Backoffice.Services;

public class FileManagerService(IConfiguration cfg, DirectoryManagerUtilities dm)
{

    #region 查詢（直接委派）
    public string NormalizePath(string? path) => dm.NormalizePath(path);
    public bool CanAddHere(string _) => true;
    public List<(string Name, string VirtualPath, int Depth)> BuildTree() => dm.BuildTree();
    public List<(string Name, string VirtualPath)> BuildBreadcrumb(string virtualPath) => dm.BuildBreadcrumb(virtualPath);
    public List<(string Name, DateTime? LastWriteUtc)> ListFolders(string virtualPath) => dm.ListFolders(virtualPath);
    public List<(string Name, long Size, DateTime? LastWriteUtc)> ListFiles(string virtualPath) => dm.ListFiles(virtualPath);
    #endregion

    #region 上傳（僅負責把 Stream 寫入 Utilities 計算出的安全目標）
    public async Task UploadAsync(string virtualPath, IFormFile file, CancellationToken ct = default)
    {
        var (absFile, _) = dm.PrepareSingleUploadTarget(virtualPath, file.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(absFile)!);
        await using var fs = File.Create(absFile);
        await file.CopyToAsync(fs, ct);
    }

    public async Task UploadManyAsync(string virtualPath, IReadOnlyList<IFormFile> files, CancellationToken ct = default)
    {
        if (files is null || files.Count == 0) return;

        foreach (var f in files)
        {
            var (absFile, _, _) = dm.PrepareBatchUploadTarget(virtualPath, f.FileName ?? "");
            Directory.CreateDirectory(Path.GetDirectoryName(absFile)!);
            await using var fs = File.Create(absFile);
            await f.CopyToAsync(fs, ct);
        }
    }
    #endregion

    #region 建立 / 刪除 / 重新命名（直接委派到 Utilities）
    public void CreateFolder(string virtualPath, string folderName) => dm.CreateFolder(virtualPath, folderName);
    public void CreateEmptyFile(string virtualPath, string fileName) => dm.CreateEmptyFile(virtualPath, fileName);
    public void DeleteFile(string virtualPath, string fileName) => dm.DeleteFile(virtualPath, fileName);
    public void DeleteFolder(string virtualPath, string folderName) => dm.DeleteFolder(virtualPath, folderName);
    public void RenameFile(string virtualPath, string oldName, string newName) => dm.RenameFile(virtualPath, oldName, newName);
    public void RenameFolder(string virtualPath, string oldName, string newName) => dm.RenameFolder(virtualPath, oldName, newName);
    #endregion

    #region 讀取 / 下載 / Zip / Raw（直接委派到 Utilities）
    public (Stream Stream, string FileName, string ContentType) OpenRead(string virtualPath, string fileName)
        => dm.OpenRead(virtualPath, fileName);

    public (Stream Stream, string ContentType) OpenRaw(string virtualPath, string fileName)
        => dm.OpenRaw(virtualPath, fileName);

    public (Stream Stream, string FileName, string ContentType) ZipFolder(string virtualPath, string folderName)
        => dm.ZipFolder(virtualPath, folderName);
    #endregion

    #region 文字檔 Read/Save（直接委派到 Utilities）

    public (string Content, string EncodingName, DateTime? LastWriteUtc) ReadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
        => dm.ReadTextFile(virtualPath, fileName, maxBytes);

    public void SaveTextFile(string virtualPath, string fileName, string content, string? encodingName = "utf-8")
        => dm.SaveTextFile(virtualPath, fileName, content, encodingName);

    #endregion

}
