using HNB.Areas.HnbBackoffice.Dtos;
using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;
using System.Text.RegularExpressions;

public sealed class DirectoryManagerUtilities
{
    #region 設定 & 欄位
    private readonly string _root;
    private readonly bool _ignoreCase;
    private static readonly Regex InvalidChars = new(@"[\\/:*?""<>|]", RegexOptions.Compiled);
    private readonly HashSet<string> _ignoreNames;
    private readonly List<Regex> _ignoreGlobs = new();
    private static Regex GlobToRegex(string glob, bool ignoreCase)
    {
        var pat = Regex.Escape(glob)
            .Replace(@"\*\*", ".*")
            .Replace(@"\*", @"[^/]*")
            .Replace(@"\?", @"[^/]");
        return new Regex("^" + pat + "$", ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
    }
    private bool ShouldIgnore(string virtualPath, string name, bool isDir)
    {
        if (_ignoreNames.Contains(name)) return true;

        var vpath = NormalizePath(virtualPath);
        var full = (vpath == "/") ? (name) : (vpath.TrimStart('/') + "/" + name);
        foreach (var rx in _ignoreGlobs)
            if (rx.IsMatch(full) || rx.IsMatch(name)) return true;

        return false;
    }
    #endregion

    #region 建構子
    public DirectoryManagerUtilities(IConfiguration cfg)
    {
        _root = Path.GetFullPath(cfg["Storage:Root"]);
        _ignoreCase = cfg.GetValue<bool>("Storage:IgnoreCase", true);

        var names = cfg.GetSection("Storage:Ignore:Names").Get<string[]>() ?? Array.Empty<string>();
        _ignoreNames = new HashSet<string>(names, _ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        var globs = cfg.GetSection("Storage:Ignore:Globs").Get<string[]>() ?? Array.Empty<string>();
        foreach (var g in globs) _ignoreGlobs.Add(GlobToRegex(g ?? "", _ignoreCase));

        Directory.CreateDirectory(_root);
    }
    #endregion

    #region 通用小工具
    /// <summary> 功能：正規化虛擬路徑（/ 開頭）</summary>
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

    /// <summary> 功能：名稱淨化 </summary>
    private string SanitizeName(string raw)
    {
        var name = (raw ?? string.Empty).Trim();
        if (name is "." or "..") return "";
        name = InvalidChars.Replace(name, "");
        if (name.Length > 128) name = name[..128];
        return name;
    }

    /// <summary> 功能：將虛擬路徑轉為 Root 內的安全實體路徑 </summary>
    private bool TryGetSafeAbsolutePath(string virtualPath, out string abs, out string msg)
    {
        abs = "";
        msg = "";
        var v = NormalizePath(virtualPath);
        var maybe = Path.GetFullPath(Path.Combine(_root, "." + v));
        var cmp = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!maybe.StartsWith(_root, cmp)) { msg = "路徑不安全"; return false; }
        abs = maybe;
        return true;
    }

    /// <summary> 取得安全的絕對路徑（檔案） </summary>
    public (string? absPath, string? error) GetSafeFilePath(string virtualPath, string name)
    {
        if (!TryGetSafeAbsolutePath(virtualPath, out var baseAbs, out var msg)) return (null, msg);
        if (string.IsNullOrWhiteSpace(name)) return (null, "檔名不可為空白");
        var seg = name.Replace('\\', '/').Trim('/');
        if (string.IsNullOrEmpty(seg) || InvalidChars.IsMatch(seg) || seg.Contains('/')) return (null, "檔名不合法");
        var abs = Path.Combine(baseAbs, seg);
        if (!System.IO.File.Exists(abs)) return (null, "檔案不存在");
        return (abs, null);
    }

    /// <summary> 取得安全的絕對路徑（資料夾） </summary>
    public (string? absPath, string? error) GetSafeDirPath(string virtualPath, string folderName)
    {
        if (!TryGetSafeAbsolutePath(virtualPath, out var baseAbs, out var msg)) return (null, msg);
        if (string.IsNullOrWhiteSpace(folderName)) return (null, "資料夾名稱不可為空白");
        var seg = folderName.Replace('\\', '/').Trim('/');
        if (string.IsNullOrEmpty(seg) || InvalidChars.IsMatch(seg) || seg.Contains('/')) return (null, "資料夾名稱不合法");
        var abs = Path.Combine(baseAbs, seg);
        if (!Directory.Exists(abs)) return (null, "資料夾不存在");
        return (abs, null);
    }

    public string GetContentType(string fileName)
    {
        var provider = new FileExtensionContentTypeProvider();
        return provider.TryGetContentType(fileName, out var ct) ? ct : "application/octet-stream";
    }

    #endregion

    #region 通用指令
    /// <summary> 功能：建立檔案 </summary>
    private void CreateFile(string absPath)
        => File.WriteAllBytes(absPath, Array.Empty<byte>());

    /// <summary> 功能：建立資料夾</summary>
    private void CreateDirectory(string absPath)
        => Directory.CreateDirectory(absPath);

    /// <summary> 功能：刪除檔案</summary>
    private void DeleteFile(string absPath)
        => File.Delete(absPath);

    /// <summary> 功能：刪除資料夾（含內容）</summary>
    private void DeleteDirectory(string absPath)
        => Directory.Delete(absPath, true);

    /// <summary> 功能：檔案改名</summary>
    private void RenameFile(string absOld, string absNew)
        => File.Move(absOld, absNew);

    /// <summary> 功能：資料夾改名</summary>
    private void RenameDirectory(string absOld, string absNew)
        => Directory.Move(absOld, absNew);
    #endregion

    #region 功能區塊

    /// <summary> 功能：在 prefix 虛擬資料夾建立空檔</summary>
    public string CreateFileAt(string prefixVirtualDir, string fileName)
    {
        if (!TryGetSafeAbsolutePath(prefixVirtualDir, out var dirAbs, out var m)) return m;
        var safe = SanitizeName(fileName);
        if (string.IsNullOrWhiteSpace(safe)) return "檔名不合法";
        CreateDirectory(dirAbs);
        var full = Path.Combine(dirAbs, safe);
        if (File.Exists(full)) return "目標檔案已存在";
        CreateFile(full);
        return "";
    }

    /// <summary> 功能：在 prefix 虛擬資料夾建立子資料夾</summary>
    public string CreateDirectoryAt(string prefixVirtualDir, string folderName)
    {
        if (!TryGetSafeAbsolutePath(prefixVirtualDir, out var dirAbs, out var m)) return m;
        var safe = SanitizeName(folderName);
        if (string.IsNullOrWhiteSpace(safe)) return "資料夾名稱不合法";
        var full = Path.Combine(dirAbs, safe);
        if (Directory.Exists(full)) return "目標資料夾已存在";
        CreateDirectory(full);
        return "";
    }

    /// <summary> 功能：刪除檔案</summary>
    public string DeleteFileAt(string prefixVirtualDir, string nameOrRelPath)
    {
        if (!TryGetSafeAbsolutePath(prefixVirtualDir, out var baseAbs, out var msg)) return msg;
        if (string.IsNullOrWhiteSpace(nameOrRelPath)) return "檔名不可為空白";

        var rel = nameOrRelPath.Replace('\\', '/').Trim('/');
        if (string.IsNullOrEmpty(rel)) return "檔名不合法";
        var segs = rel.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var s in segs) { if (s is "." or ".." || InvalidChars.IsMatch(s)) return "路徑包含非法字元"; }

        var target = Path.Combine(baseAbs, Path.Combine(segs));
        if (!File.Exists(target)) return "檔案不存在";
        try { File.Delete(target); return ""; }
        catch (Exception ex) { return ex.Message; }
    }

    /// <summary> 功能：刪除資料夾（含內容）</summary>
    public string DeleteDirectoryAt(string prefixVirtualDir, string nameOrRelPath, bool recursive = true)
    {
        if (!TryGetSafeAbsolutePath(prefixVirtualDir, out var baseAbs, out var msg)) return msg;
        if (string.IsNullOrWhiteSpace(nameOrRelPath)) return "資料夾名稱不可為空白";

        var rel = nameOrRelPath.Replace('\\', '/').Trim('/');
        if (string.IsNullOrEmpty(rel)) return "資料夾名稱不合法";
        var segs = rel.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var s in segs) { if (s is "." or ".." || InvalidChars.IsMatch(s)) return "路徑包含非法字元"; }

        var target = Path.Combine(baseAbs, Path.Combine(segs));
        if (!Directory.Exists(target)) return "資料夾不存在";
        try { Directory.Delete(target, recursive); return ""; }
        catch (Exception ex) { return ex.Message; }
    }

    /// <summary> 功能：檔案改名（同層內）</summary>
    public string RenameFileAt(string prefixVirtualDir, string oldName, string newName)
    {
        if (!TryGetSafeAbsolutePath(prefixVirtualDir, out var baseAbs, out var msg)) return msg;
        oldName = (oldName ?? "").Trim();
        newName = (newName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName)) return "名稱不可為空白";
        if (string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase)) return "新舊名稱相同";
        if (InvalidChars.IsMatch(newName) || newName.Contains('/') || newName.Contains('\\')) return "名稱含非法字元";

        var src = Path.Combine(baseAbs, oldName);
        var dst = Path.Combine(baseAbs, newName);
        if (!System.IO.File.Exists(src)) return "來源檔案不存在";
        if (System.IO.File.Exists(dst) || Directory.Exists(dst)) return "目標名稱已存在";

        try { System.IO.File.Move(src, dst); return ""; }
        catch (Exception ex) { return ex.Message; }
    }

    /// <summary> 功能：資料夾改名（同層內）</summary>
    public string RenameDirectoryAt(string prefixVirtualDir, string oldName, string newName)
    {
        if (!TryGetSafeAbsolutePath(prefixVirtualDir, out var baseAbs, out var msg)) return msg;
        oldName = (oldName ?? "").Trim();
        newName = (newName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName)) return "名稱不可為空白";
        if (string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase)) return "新舊名稱相同";
        if (InvalidChars.IsMatch(newName) || newName.Contains('/') || newName.Contains('\\')) return "名稱含非法字元";

        var src = Path.Combine(baseAbs, oldName);
        var dst = Path.Combine(baseAbs, newName);
        if (!Directory.Exists(src)) return "來源資料夾不存在";
        if (Directory.Exists(dst) || System.IO.File.Exists(dst)) return "目標名稱已存在";

        try { Directory.Move(src, dst); return ""; }
        catch (Exception ex) { return ex.Message; }
    }

    /// <summary> 建立 ZIP（回傳暫存檔路徑；大目錄避免吃記憶體） </summary>
    public (string? zipPath, string? error, string downloadName) CreateZipOfDirectory(string virtualPath, string folderName)
    {
        var (srcDir, err) = GetSafeDirPath(virtualPath, folderName);
        if (err != null) return (null, err, folderName + ".zip");

        var tmpDir = Path.Combine(Path.GetTempPath(), "HNB_Zips");
        Directory.CreateDirectory(tmpDir);
        var zipName = $"{folderName}_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";
        var zipAbs = Path.Combine(tmpDir, zipName);

        using (var fs = new FileStream(zipAbs, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        using (var zip = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: false))
        {
            var baseLen = srcDir.TrimEnd(Path.DirectorySeparatorChar).Length + 1;
            foreach (var file in Directory.EnumerateFiles(srcDir, "*", SearchOption.AllDirectories))
            {
                var rel = file.Substring(baseLen).Replace(Path.DirectorySeparatorChar, '/');
                zip.CreateEntryFromFile(file, folderName.Trim('/') + "/" + rel, CompressionLevel.Optimal);
            }
        }
        return (zipAbs, null, folderName + ".zip");
    }

    #endregion


    /// <summary>
    /// 目錄樹查詢：由起點虛擬路徑建立目錄樹（不丟例外；無法解析時回傳目前已累積結果）
    /// </summary>
    public List<FileTreeNodeDto> BuildTree(string startVirtualPath = "/")
    {
        var result = new List<FileTreeNodeDto>();

        var startV = NormalizePath(startVirtualPath);
        var rootName = startV == "/" ? "根目錄" : Path.GetFileName(startV);
        result.Add(new FileTreeNodeDto { Name = rootName, VirtualPath = startV, Depth = 0 });

        if (!TryGetSafeAbsolutePath(startV, out var absStart, out _))
            return result;

        Dfs(startV, absStart, 1);
        return result;

        void Dfs(string curV, string curAbs, int depth)
        {
            foreach (var dirAbs in Directory.EnumerateDirectories(curAbs).OrderBy(Path.GetFileName))
            {
                var name = Path.GetFileName(dirAbs);
                if (ShouldIgnore(curV, name, isDir: true)) continue;

                var childV = curV == "/" ? "/" + name : curV + "/" + name;
                result.Add(new FileTreeNodeDto { Name = name, VirtualPath = childV, Depth = depth });
                Dfs(childV, dirAbs, depth + 1);
            }
        }
    }

    /// <summary>列出資料夾</summary>
    public IEnumerable<(string Name, DateTime? LastWriteUtc)> ListFolders(string virtualPath)
    {
        var v = NormalizePath(virtualPath);
        if (!TryGetSafeAbsolutePath(v, out var abs, out _)) return Enumerable.Empty<(string, DateTime?)>();
        if (!Directory.Exists(abs)) return Enumerable.Empty<(string, DateTime?)>();

        return Directory.EnumerateDirectories(abs)
            .Select(d => Path.GetFileName(d))
            .Where(n => !ShouldIgnore(v, n, isDir: true))
            .OrderBy(n => n)
            .Select(n => (n, (DateTime?)Directory.GetLastWriteTimeUtc(Path.Combine(abs, n))));
    }

    /// <summary>列出檔案</summary>
    public IEnumerable<(string Name, long Size, DateTime? LastWriteUtc)> ListFiles(string virtualPath)
    {
        var v = NormalizePath(virtualPath);
        if (!TryGetSafeAbsolutePath(v, out var abs, out _)) return Enumerable.Empty<(string, long, DateTime?)>();
        if (!Directory.Exists(abs)) return Enumerable.Empty<(string, long, DateTime?)>();

        return Directory.EnumerateFiles(abs)
            .Select(f => new FileInfo(f))
            .Where(fi => !ShouldIgnore(v, fi.Name, isDir: false))
            .OrderBy(fi => fi.Name)
            .Select(fi => (fi.Name, fi.Length, (DateTime?)fi.LastWriteTimeUtc));
    }

    /// <summary>上傳：一次性儲存 IFormFile（含相對路徑）</summary>
    public string SaveFormFileAt(string prefixVirtualDir, string relativePath, IFormFile file)
    {
        if (!TryGetSafeAbsolutePath(prefixVirtualDir, out var baseAbs, out var m)) return m;

        var rel = (relativePath ?? file.FileName).Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(rel)) return "檔名不合法";

        var segs = rel.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var seg in segs)
        {
            if (seg is "." or "..") return "路徑不合法";
            if (InvalidChars.IsMatch(seg)) return "名稱含非法字元";
        }

        var dstAbs = Path.Combine(baseAbs, Path.Combine(segs));
        Directory.CreateDirectory(Path.GetDirectoryName(dstAbs)!);

        using var rs = file.OpenReadStream();
        using var fs = new FileStream(dstAbs, FileMode.Create, FileAccess.Write, FileShare.None);
        rs.CopyTo(fs);
        return "";
    }

    /// <summary>讀寫純文字 UTF-8（Utilities）</summary>
    public (string? text, string? error) ReadTextAt(string prefixVirtualDir, string name)
    {
        var (abs, err) = GetSafeFilePath(prefixVirtualDir, name);
        if (err != null) return (null, err);
        try { return (System.IO.File.ReadAllText(abs!, System.Text.Encoding.UTF8), null); }
        catch (Exception ex) { return (null, ex.Message); }
    }

    /// <summary>編輯純文字 UTF-8（Utilities）</summary>
    public string WriteTextAt(string prefixVirtualDir, string name, string content)
    {
        var (abs, err) = GetSafeFilePath(prefixVirtualDir, name);
        if (err != null) return err;
        try { System.IO.File.WriteAllText(abs!, content ?? "", System.Text.Encoding.UTF8); return ""; }
        catch (Exception ex) { return ex.Message; }
    }

}
