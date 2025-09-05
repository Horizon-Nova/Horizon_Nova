using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace HNB.Areas.HnbBackoffice.Services;

public class BackofficeService(IConfiguration cfg)
{
    private readonly string _root = Path.GetFullPath(cfg["Storage:Root"] ?? "/app/storage");
    private static readonly Regex InvalidChars = new(@"[\\/:*?""<>|]", RegexOptions.Compiled);
    private static readonly FileExtensionContentTypeProvider Ct = new();

    /* ========= 查詢 ========= */

    public string NormalizePath(string? path)
    {
        var p = (path ?? "/").Replace('\\', '/').Trim();
        if (string.IsNullOrEmpty(p) || p == "/") return "/";
        if (!p.StartsWith('/')) p = "/" + p;
        while (p.Contains("//")) p = p.Replace("//", "/");
        var segs = p.Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => s != "." && s != "..");
        return "/" + string.Join('/', segs);
    }

    public bool CanAddHere(string virtualPath) => virtualPath != "/";

    public List<(string Name, string VirtualPath)> BuildBreadcrumb(string virtualPath)
    {
        var list = new List<(string, string)> { ("/", "/") };
        if (virtualPath == "/") return list;

        var segs = virtualPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var acc = "";
        foreach (var s in segs)
        {
            acc = string.IsNullOrEmpty(acc) ? "/" + s : acc + "/" + s;
            list.Add((s, acc));
        }
        return list;
    }

    // 扁平樹（含縮排層級 Depth）；根目錄固定第一筆
    public List<(string Name, string VirtualPath, int Depth)> BuildTree()
    {
        var tree = new List<(string, string, int)> { ("根目錄", "/", 0) };
        Dfs("/", 1);
        return tree;

        void Dfs(string curV, int depth)
        {
            var curAbs = GetSafeAbsolutePath(curV);
            foreach (var dir in Directory.EnumerateDirectories(curAbs).OrderBy(Path.GetFileName))
            {
                var name = Path.GetFileName(dir);
                var childV = curV == "/" ? "/" + name : curV + "/" + name;
                tree.Add((name, childV, depth));
                Dfs(childV, depth + 1);
            }
        }
    }

    public List<(string Name, DateTime? LastWriteUtc)> ListFolders(string virtualPath)
    {
        var abs = GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(abs);
        return Directory.EnumerateDirectories(abs)
            .OrderBy(Path.GetFileName)
            .Select(d => (Path.GetFileName(d), (DateTime?)Directory.GetLastWriteTimeUtc(d)))
            .ToList();
    }


    public List<(string Name, long Size, DateTime? LastWriteUtc)> ListFiles(string virtualPath)
    {
        var abs = GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(abs);
        return Directory.EnumerateFiles(abs)
            .OrderBy(Path.GetFileName)
            .Select(f => {
                var fi = new FileInfo(f);
                return (fi.Name, fi.Length, fi.LastWriteTimeUtc as DateTime?);
            })
            .ToList();
    }

    /* ========= 動作 ========= */

    public async Task UploadAsync(string virtualPath, IFormFile file, CancellationToken ct = default)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(absDir);

        var safeName = SanitizeName(file.FileName);
        if (string.IsNullOrWhiteSpace(safeName))
            throw new InvalidOperationException("檔名不合法");

        var target = EnsureUniqueFile(Path.Combine(absDir, safeName));
        await using var fs = File.Create(target);
        await file.CopyToAsync(fs, ct);
    }

    public void CreateFolder(string virtualPath, string folderName)
    {
        if (!CanAddHere(virtualPath))
            throw new InvalidOperationException("根目錄不允許新增");

        var absDir = GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(absDir);

        var safe = SanitizeName(folderName);
        if (string.IsNullOrWhiteSpace(safe))
            throw new InvalidOperationException("資料夾名稱不合法");

        var newDir = EnsureUniqueDirectory(Path.Combine(absDir, safe));
        Directory.CreateDirectory(newDir);
    }

    public void CreateEmptyFile(string virtualPath, string fileName)
    {
        if (!CanAddHere(virtualPath))
            throw new InvalidOperationException("根目錄不允許新增");

        var absDir = GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(absDir);

        var safe = SanitizeName(fileName);
        if (string.IsNullOrWhiteSpace(safe))
            throw new InvalidOperationException("檔名不合法");

        var target = EnsureUniqueFile(Path.Combine(absDir, safe));
        File.WriteAllBytes(target, Array.Empty<byte>());
    }

    public void DeleteFile(string virtualPath, string fileName)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        var safe = SanitizeName(fileName);
        var full = Path.Combine(absDir, safe);
        if (File.Exists(full)) File.Delete(full);
    }

    // 只允許刪「空」資料夾
    public void DeleteFolder(string virtualPath, string folderName)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        var safe = SanitizeName(folderName);
        var dir = Path.Combine(absDir, safe);
        if (Directory.Exists(dir))
        {
            if (Directory.EnumerateFileSystemEntries(dir).Any())
                throw new InvalidOperationException("請先清空資料夾內容，再刪除。");
            Directory.Delete(dir);
        }
    }

    public (Stream Stream, string FileName, string ContentType) OpenRead(string virtualPath, string fileName)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        var safe = SanitizeName(fileName);
        var full = Path.Combine(absDir, safe);
        if (!File.Exists(full)) throw new FileNotFoundException();

        var stream = File.OpenRead(full);
        var ct = Ct.TryGetContentType(safe, out var contentType) ? contentType : "application/octet-stream";
        return (stream, safe, ct);
    }

    /* ========= 私有工具（不暴露任何 model 類別） ========= */

    private string GetSafeAbsolutePath(string virtualPath)
    {
        Directory.CreateDirectory(_root);
        var normalized = NormalizePath(virtualPath);
        var combined = Path.GetFullPath(Path.Combine(_root, "." + normalized));
        if (!combined.StartsWith(_root, StringComparison.Ordinal))
            throw new InvalidOperationException("路徑不安全");
        return combined;
    }

    private static string SanitizeName(string raw)
    {
        var name = (raw ?? "").Trim();
        if (name is "." or "..") return "";
        name = InvalidChars.Replace(name, "");
        if (name.Length > 128) name = name[..128];
        return name;
    }

    private static string EnsureUniqueFile(string absPath)
    {
        if (!File.Exists(absPath)) return absPath;
        var dir = Path.GetDirectoryName(absPath)!;
        var file = Path.GetFileNameWithoutExtension(absPath);
        var ext = Path.GetExtension(absPath);
        var i = 1; string candidate;
        do { candidate = Path.Combine(dir, $"{file} ({i++}){ext}"); }
        while (File.Exists(candidate));
        return candidate;
    }

    private static string EnsureUniqueDirectory(string absDir)
    {
        if (!Directory.Exists(absDir)) return absDir;
        var parent = Path.GetDirectoryName(absDir)!;
        var name = Path.GetFileName(absDir);
        var i = 1; string candidate;
        do { candidate = Path.Combine(parent, $"{name} ({i++})"); }
        while (Directory.Exists(candidate));
        return candidate;
    }
}
