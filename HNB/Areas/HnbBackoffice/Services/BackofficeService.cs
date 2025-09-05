using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using HNB.Areas.HnbBackoffice.Utilities;

namespace HNB.Areas.HnbBackoffice.Services;

public class BackofficeService
{
    private readonly DirectoryManagerUtilities _dm;
    private static readonly FileExtensionContentTypeProvider Ct = new();

    public BackofficeService(IConfiguration cfg)
    {
        _dm = new DirectoryManagerUtilities(cfg);
    }
    #region 檔案總管 (File Manager)
    /* ===== 查詢（直接委派） ===== */
    public string NormalizePath(string? path) => _dm.NormalizePath(path);
    public bool CanAddHere(string _) => true;
    public List<(string Name, string VirtualPath, int Depth)> BuildTree() => _dm.BuildTree();
    public List<(string Name, string VirtualPath)> BuildBreadcrumb(string virtualPath) => _dm.BuildBreadcrumb(virtualPath);
    public List<(string Name, DateTime? LastWriteUtc)> ListFolders(string virtualPath) => _dm.ListFolders(virtualPath);
    public List<(string Name, long Size, DateTime? LastWriteUtc)> ListFiles(string virtualPath) => _dm.ListFiles(virtualPath);

    /* ===== 動作（業務規則 + Utilities） ===== */

    public async Task UploadAsync(string virtualPath, IFormFile file, CancellationToken ct = default)
    {
        var absDir = _dm.GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(absDir);

        var safeName = _dm.SanitizeName(file.FileName);
        if (string.IsNullOrWhiteSpace(safeName)) throw new InvalidOperationException("檔名不合法");
        if (_dm.IsProtected(virtualPath, safeName)) throw new InvalidOperationException("保護路徑，禁止上傳");

        var target = _dm.EnsureUniqueFile(Path.Combine(absDir, safeName));
        await using var fs = File.Create(target);
        await file.CopyToAsync(fs, ct);
    }

    public async Task UploadManyAsync(string virtualPath, IReadOnlyList<IFormFile> files, CancellationToken ct = default)
    {
        if (files is null || files.Count == 0) return;

        foreach (var f in files)
        {
            var rel = (f.FileName ?? "").Replace('\\', '/').Trim('/');
            if (string.IsNullOrWhiteSpace(rel)) continue;

            rel = System.Net.WebUtility.HtmlDecode(rel);

            var safeRel = _dm.SanitizePathSegments(rel);
            if (string.IsNullOrWhiteSpace(safeRel)) continue;

            var segs = safeRel.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < segs.Length - 1; i++)
                _dm.EnsureFolderAsciiOnlyOrThrow(segs[i]);

            var dirPart = Path.GetDirectoryName("/" + safeRel)?.Replace('\\', '/') ?? "/";
            var filePart = _dm.SanitizeName(Path.GetFileName(safeRel));

            var targetVDir = _dm.NormalizePath((virtualPath ?? "/").TrimEnd('/') + "/" + (dirPart?.Trim('/') ?? ""));
            if (_dm.IsProtected(targetVDir, filePart))
                throw new InvalidOperationException("保護路徑，禁止上傳");

            var absDir = _dm.GetSafeAbsolutePath(targetVDir);
            Directory.CreateDirectory(absDir);

            var target = _dm.EnsureUniqueFile(Path.Combine(absDir, filePart));
            await using var fs = File.Create(target);
            await f.CopyToAsync(fs, ct);
        }
    }


    public void CreateFolder(string virtualPath, string folderName)
    {
        var absDir = _dm.GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(absDir);

        _dm.EnsureFolderAsciiOnlyOrThrow(folderName);

        var safe = _dm.SanitizeName(folderName);
        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("資料夾名稱不合法");
        if (_dm.IsProtected(virtualPath, safe)) throw new InvalidOperationException("保護路徑，禁止建立資料夾");

        var newDir = _dm.EnsureUniqueDirectory(Path.Combine(absDir, safe));
        Directory.CreateDirectory(newDir);
    }

    public void CreateEmptyFile(string virtualPath, string fileName)
    {
        var absDir = _dm.GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(absDir);

        var safe = _dm.SanitizeName(fileName);
        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("檔名不合法");
        if (_dm.IsProtected(virtualPath, safe)) throw new InvalidOperationException("保護路徑，禁止建立檔案");

        var target = _dm.EnsureUniqueFile(Path.Combine(absDir, safe));
        File.WriteAllBytes(target, Array.Empty<byte>());
    }

    public void DeleteFile(string virtualPath, string fileName)
    {
        if (_dm.IsProtected(virtualPath, fileName)) throw new InvalidOperationException("保護路徑，禁止刪除");
        var absDir = _dm.GetSafeAbsolutePath(virtualPath);
        var safe = _dm.SanitizeName(fileName);
        var full = Path.Combine(absDir, safe);
        if (File.Exists(full)) File.Delete(full);
    }

    public void DeleteFolder(string virtualPath, string folderName)
    {
        if (_dm.IsProtected(virtualPath, folderName)) throw new InvalidOperationException("保護路徑，禁止刪除");
        var absDir = _dm.GetSafeAbsolutePath(virtualPath);
        var safe = _dm.SanitizeName(folderName);
        var dir = Path.Combine(absDir, safe);
        if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
    }

    public void RenameFile(string virtualPath, string oldName, string newName)
    {
        var absDir = _dm.GetSafeAbsolutePath(virtualPath);
        var src = Path.Combine(absDir, _dm.SanitizeName(oldName));
        var newSafe = _dm.SanitizeName(newName);
        if (string.IsNullOrWhiteSpace(newSafe)) throw new InvalidOperationException("新檔名不合法");
        var dst = Path.Combine(absDir, newSafe);

        if (!File.Exists(src)) throw new FileNotFoundException();
        if (_dm.IsProtected(virtualPath, oldName) || _dm.IsProtected(virtualPath, newSafe)) throw new InvalidOperationException("保護路徑，禁止重命名");
        if (File.Exists(dst)) throw new InvalidOperationException("目標檔名已存在");

        File.Move(src, dst);
    }

    public void RenameFolder(string virtualPath, string oldName, string newName)
    {
        var absDir = _dm.GetSafeAbsolutePath(virtualPath);

        _dm.EnsureFolderAsciiOnlyOrThrow(newName);

        var src = Path.Combine(absDir, _dm.SanitizeName(oldName));
        var newSafe = _dm.SanitizeName(newName);
        if (string.IsNullOrWhiteSpace(newSafe)) throw new InvalidOperationException("新資料夾名稱不合法");
        var dst = Path.Combine(absDir, newSafe);

        if (!Directory.Exists(src)) throw new DirectoryNotFoundException();
        if (_dm.IsProtected(virtualPath, oldName) || _dm.IsProtected(virtualPath, newSafe)) throw new InvalidOperationException("保護路徑，禁止重命名");
        if (Directory.Exists(dst)) throw new InvalidOperationException("目標資料夾已存在");

        Directory.Move(src, dst);
    }

    public (Stream Stream, string FileName, string ContentType) OpenRead(string virtualPath, string fileName)
    {
        var absDir = _dm.GetSafeAbsolutePath(virtualPath);
        var safe = _dm.SanitizeName(fileName);
        var full = Path.Combine(absDir, safe);
        if (!File.Exists(full)) throw new FileNotFoundException();

        var stream = File.OpenRead(full);
        var ct = new FileExtensionContentTypeProvider().TryGetContentType(safe, out var contentType)
            ? contentType : "application/octet-stream";
        return (stream, safe, ct);
    }

    #endregion

}
