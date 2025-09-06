using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using HNB.Areas.HnbBackoffice.Utilities;

namespace HNB.Areas.HnbBackoffice.Services;

public class BackofficeService
{
    #region 欄位 & 建構子
    private readonly DirectoryManagerUtilities _dm;

    public BackofficeService(IConfiguration cfg)
    {
        _dm = new DirectoryManagerUtilities(cfg);
    }
    #endregion

    #region 查詢（直接委派）
    public string NormalizePath(string? path) => _dm.NormalizePath(path);
    public bool CanAddHere(string _) => true;
    public List<(string Name, string VirtualPath, int Depth)> BuildTree() => _dm.BuildTree();
    public List<(string Name, string VirtualPath)> BuildBreadcrumb(string virtualPath) => _dm.BuildBreadcrumb(virtualPath);
    public List<(string Name, DateTime? LastWriteUtc)> ListFolders(string virtualPath) => _dm.ListFolders(virtualPath);
    public List<(string Name, long Size, DateTime? LastWriteUtc)> ListFiles(string virtualPath) => _dm.ListFiles(virtualPath);
    #endregion

    #region 上傳（僅負責把 Stream 寫入 Utilities 計算出的安全目標）
    public async Task UploadAsync(string virtualPath, IFormFile file, CancellationToken ct = default)
    {
        var (absFile, _) = _dm.PrepareSingleUploadTarget(virtualPath, file.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(absFile)!);
        await using var fs = File.Create(absFile);
        await file.CopyToAsync(fs, ct);
    }

    public async Task UploadManyAsync(string virtualPath, IReadOnlyList<IFormFile> files, CancellationToken ct = default)
    {
        if (files is null || files.Count == 0) return;

        foreach (var f in files)
        {
            var (absFile, _, _) = _dm.PrepareBatchUploadTarget(virtualPath, f.FileName ?? "");
            Directory.CreateDirectory(Path.GetDirectoryName(absFile)!);
            await using var fs = File.Create(absFile);
            await f.CopyToAsync(fs, ct);
        }
    }
    #endregion

    #region 建立 / 刪除 / 重新命名（直接委派到 Utilities）
    public void CreateFolder(string virtualPath, string folderName) => _dm.CreateFolder(virtualPath, folderName);
    public void CreateEmptyFile(string virtualPath, string fileName) => _dm.CreateEmptyFile(virtualPath, fileName);
    public void DeleteFile(string virtualPath, string fileName) => _dm.DeleteFile(virtualPath, fileName);
    public void DeleteFolder(string virtualPath, string folderName) => _dm.DeleteFolder(virtualPath, folderName);
    public void RenameFile(string virtualPath, string oldName, string newName) => _dm.RenameFile(virtualPath, oldName, newName);
    public void RenameFolder(string virtualPath, string oldName, string newName) => _dm.RenameFolder(virtualPath, oldName, newName);
    #endregion

    #region 讀取 / 下載 / Zip / Raw（直接委派到 Utilities）
    public (Stream Stream, string FileName, string ContentType) OpenRead(string virtualPath, string fileName)
        => _dm.OpenRead(virtualPath, fileName);

    public (Stream Stream, string ContentType) OpenRaw(string virtualPath, string fileName)
        => _dm.OpenRaw(virtualPath, fileName);

    public (Stream Stream, string FileName, string ContentType) ZipFolder(string virtualPath, string folderName)
        => _dm.ZipFolder(virtualPath, folderName);
    #endregion

    #region 文字檔 Read/Save（直接委派到 Utilities）

    public (string Content, string EncodingName, DateTime? LastWriteUtc) ReadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
        => _dm.ReadTextFile(virtualPath, fileName, maxBytes);

    public void SaveTextFile(string virtualPath, string fileName, string content, string? encodingName = "utf-8")
        => _dm.SaveTextFile(virtualPath, fileName, content, encodingName);

    #endregion

}
