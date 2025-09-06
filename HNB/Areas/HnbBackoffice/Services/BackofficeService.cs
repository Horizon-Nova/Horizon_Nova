using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using HNB.Areas.HnbBackoffice.Utilities;
using System.IO.Compression;
using System.Text;

namespace HNB.Areas.HnbBackoffice.Services;

public class BackofficeService
{
    #region 欄位 & 建構子
    private readonly DirectoryManagerUtilities _dm;
    private static readonly FileExtensionContentTypeProvider Ct = new();

    public BackofficeService(IConfiguration cfg)
    {
        _dm = new DirectoryManagerUtilities(cfg);
    }
    #endregion

    #region 檔案總管 - 查詢 (直接委派)
    public string NormalizePath(string? path) => _dm.NormalizePath(path);
    public bool CanAddHere(string _) => true;
    public List<(string Name, string VirtualPath, int Depth)> BuildTree() => _dm.BuildTree();
    public List<(string Name, string VirtualPath)> BuildBreadcrumb(string virtualPath) => _dm.BuildBreadcrumb(virtualPath);
    public List<(string Name, DateTime? LastWriteUtc)> ListFolders(string virtualPath) => _dm.ListFolders(virtualPath);
    public List<(string Name, long Size, DateTime? LastWriteUtc)> ListFiles(string virtualPath) => _dm.ListFiles(virtualPath);
    #endregion

    #region 檔案總管 - 上傳
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
    #endregion

    #region 檔案總管 - 建立
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
    #endregion

    #region 檔案總管 - 刪除
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
    #endregion

    #region 檔案總管 - 重新命名
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
    #endregion

    #region 檔案總管 - 開啟 / 下載
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

    // 1) 資料夾壓縮為 ZIP，回傳可串流的檔案（用暫存檔 + DeleteOnClose）
    public (Stream Stream, string FileName, string ContentType) ZipFolder(string virtualPath, string folderName)
    {
        if (_dm.IsProtected(virtualPath, folderName))
            throw new InvalidOperationException("保護路徑，禁止壓縮");

        var absParent = _dm.GetSafeAbsolutePath(virtualPath);
        var safe = _dm.SanitizeName(folderName);
        var absTargetDir = Path.Combine(absParent, safe);
        if (!Directory.Exists(absTargetDir))
            throw new DirectoryNotFoundException("資料夾不存在");

        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".zip");
        var fs = new FileStream(temp, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 1 << 16, FileOptions.DeleteOnClose);

        using (var zip = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: true))
        {
            var baseV = NormalizePath((virtualPath ?? "/").TrimEnd('/') + "/" + safe);
            AddDir(absTargetDir, baseV, "");
            void AddDir(string absDir, string vDir, string rel)
            {
                foreach (var dir in Directory.EnumerateDirectories(absDir))
                {
                    var name = Path.GetFileName(dir);
                    if (_dm.IsProtected(vDir, name)) continue;
                    var nextV = vDir == "/" ? "/" + name : vDir + "/" + name;
                    AddDir(dir, nextV, rel + name + "/");
                }
                foreach (var file in Directory.EnumerateFiles(absDir))
                {
                    var name = Path.GetFileName(file);
                    if (_dm.IsProtected(vDir, name)) continue;
                    var entry = zip.CreateEntry(rel + name, CompressionLevel.Fastest);
                    using var src = File.OpenRead(file);
                    using var dst = entry.Open();
                    src.CopyTo(dst);
                }
            }
        }
        fs.Position = 0;
        return (fs, safe + ".zip", "application/zip");
    }

    // 以「inline」方式讀檔（給預覽 <img>/<video> 等用）
    public (Stream Stream, string ContentType) OpenRaw(string virtualPath, string fileName)
    {
        var (s, _, ct) = OpenRead(virtualPath, fileName);
        return (s, ct);
    }
    #endregion

    #region 檔案總管 - 文字檔編輯
    // 純文字類型判斷（可編輯/可文字預覽）
    private static readonly HashSet<string> EditableExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",".md",".json",".jsonl",".js",".ts",".css",".scss",".sass",
        ".cs",".cshtml",".html",".htm",".xml",".yml",".yaml",".ini",".conf",".cfg",
        ".log",".py",".sh",".bat",".ps1",".sql",".env",".properties",".toml",".gitignore",".editorconfig"
    };

    private static bool IsEditable(string fileName) => EditableExt.Contains(Path.GetExtension(fileName));

    // 讀取文字檔（含簡單 BOM 偵測；預設上限 1MB）
    public (string Content, string EncodingName, DateTime? LastWriteUtc) ReadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
    {
        if (!IsEditable(fileName))
            throw new InvalidOperationException("此檔案類型不支援線上查看/編輯");

        var absDir = _dm.GetSafeAbsolutePath(virtualPath);
        var safe = _dm.SanitizeName(fileName);
        var full = Path.Combine(absDir, safe);
        if (!File.Exists(full)) throw new FileNotFoundException();

        var fi = new FileInfo(full);
        if (fi.Length > maxBytes)
            throw new InvalidOperationException($"檔案過大（{fi.Length:N0} bytes），超過上限 {maxBytes:N0}。");

        using var fs = File.OpenRead(full);
        // BOM 偵測
        var preamble = new byte[4];
        var read = fs.Read(preamble, 0, 4);
        fs.Position = 0;

        Encoding enc = Encoding.UTF8;
        if (read >= 3 && preamble[0] == 0xEF && preamble[1] == 0xBB && preamble[2] == 0xBF) enc = new UTF8Encoding(true);
        else if (read >= 2 && preamble[0] == 0xFF && preamble[1] == 0xFE) enc = Encoding.Unicode;           // UTF-16 LE
        else if (read >= 2 && preamble[0] == 0xFE && preamble[1] == 0xFF) enc = Encoding.BigEndianUnicode;  // UTF-16 BE

        using var sr = new StreamReader(fs, enc, detectEncodingFromByteOrderMarks: true);
        var text = sr.ReadToEnd();
        return (text, enc.WebName, fi.LastWriteTimeUtc);
    }

    // 儲存文字檔（UTF-8 / UTF-8 BOM 可選）
    public void SaveTextFile(string virtualPath, string fileName, string content, string? encodingName = "utf-8")
    {
        if (_dm.IsProtected(virtualPath, fileName))
            throw new InvalidOperationException("保護路徑，禁止寫入");
        if (!IsEditable(fileName))
            throw new InvalidOperationException("此檔案類型不支援線上編輯");

        var absDir = _dm.GetSafeAbsolutePath(virtualPath);
        var safe = _dm.SanitizeName(fileName);
        var full = Path.Combine(absDir, safe);
        if (!File.Exists(full)) throw new FileNotFoundException();

        var enc = encodingName?.Equals("utf-8-bom", StringComparison.OrdinalIgnoreCase) == true
            ? new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
            : Encoding.UTF8;

        File.WriteAllText(full, content ?? string.Empty, enc);
    }
    #endregion
}
