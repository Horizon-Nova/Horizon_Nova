using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace HNB.Areas.HnbBackoffice.Utilities;

public sealed class DirectoryManagerUtilities
{
    /* ===== 設定 & 欄位 ===== */
    private readonly string _root;
    private readonly bool _ignoreCase;
    private readonly string[] _ignorePatternsFromConfig;
    private readonly string _ignoreFileName;
    private readonly List<Regex> _protectedRegexes = new();
    private static readonly Regex FolderAsciiRegex = new(@"^[A-Za-z0-9 _.\-]+$", RegexOptions.Compiled);

    private static readonly Regex InvalidChars = new(@"[\\/:*?""<>|]", RegexOptions.Compiled);

    public DirectoryManagerUtilities(IConfiguration cfg)
    {
        _root = Path.GetFullPath(cfg["Storage:Root"] ?? "/app/storage");
        _ignoreCase = cfg.GetValue<bool>("Storage:IgnoreCase", true);
        _ignoreFileName = cfg["Storage:IgnoreFileName"] ?? ".backofficeignore";
        _ignorePatternsFromConfig = cfg.GetSection("Storage:IgnorePatterns").Get<string[]>() ?? Array.Empty<string>();

        Directory.CreateDirectory(_root);
        BuildIgnoreRegexCache();
    }

    /* ====== Path / Name Utilities ====== */

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

    public string GetSafeAbsolutePath(string virtualPath)
    {
        Directory.CreateDirectory(_root);
        var normalized = NormalizePath(virtualPath);
        var combined = Path.GetFullPath(Path.Combine(_root, "." + normalized));
        var cmp = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!combined.StartsWith(_root, cmp))
            throw new InvalidOperationException("路徑不安全");
        return combined;
    }

    public string SanitizeName(string raw)
    {
        var name = System.Net.WebUtility.HtmlDecode(raw ?? string.Empty).Trim();
        if (name is "." or "..") return "";
        name = InvalidChars.Replace(name, "");
        if (name.Length > 128) name = name[..128];
        return name;
    }

    public string SanitizePathSegments(string relPath)
    {
        var parts = relPath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                           .Select(SanitizeName)
                           .Where(s => !string.IsNullOrWhiteSpace(s));
        return string.Join('/', parts);
    }

    public string EnsureUniqueFile(string absPath)
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

    public string EnsureUniqueDirectory(string absDir)
    {
        if (!Directory.Exists(absDir)) return absDir;
        var parent = Path.GetDirectoryName(absDir)!;
        var name = Path.GetFileName(absDir);
        var i = 1; string candidate;
        do { candidate = Path.Combine(parent, $"{name} ({i++})"); }
        while (Directory.Exists(candidate));
        return candidate;
    }

    // 資料夾命名判斷：不允許中文/非 ASCII，允許 英數、空白、. _ -
    public void EnsureFolderAsciiOnlyOrThrow(string? rawName)
    {
        var name = WebUtility.HtmlDecode(rawName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("資料夾名稱不可為空");
        if (!FolderAsciiRegex.IsMatch(name))
            throw new InvalidOperationException("資料夾名稱僅限英文/數字/空白/.-_（不允許中文或其他非 ASCII 字元）");
    }

    /* ====== Ignore / Protect 名單 ====== */

    /// <summary>
    /// 是否命中保護名單（.gitignore 風格）：
    /// - pattern 例： "lost+found/**", ".git/**", "*.env", ".DS_Store"
    /// - 不以 "/" 開頭者，等同 "**/pattern"
    /// </summary>
    public bool IsProtected(string virtualPath, string? name = null)
    {
        if (_protectedRegexes.Count == 0) return false;

        var v = NormalizePath(virtualPath);
        var rel = (v == "/" ? "" : v.TrimStart('/') + "/") + (name ?? "");
        rel = rel.Trim('/');

        // 當作資料夾語意再試一次（尾斜線）
        var folderRel = rel.Length == 0 ? "" : rel + "/";

        foreach (var rx in _protectedRegexes)
        {
            if (rx.IsMatch(rel) || (folderRel.Length > 0 && rx.IsMatch(folderRel)))
                return true;
        }
        return false;
    }

    private void BuildIgnoreRegexCache()
    {
        _protectedRegexes.Clear();

        var patterns = new List<string>(_ignorePatternsFromConfig);

        // 從根目錄讀 ignore 檔
        var ignoreFile = Path.Combine(_root, _ignoreFileName);
        if (File.Exists(ignoreFile))
        {
            foreach (var ln in File.ReadAllLines(ignoreFile))
            {
                var t = (ln ?? "").Trim();
                if (t.Length == 0 || t.StartsWith("#")) continue;
                patterns.Add(t);
            }
        }

        // 預設也保護 ignore 檔本身
        if (!patterns.Contains(_ignoreFileName)) patterns.Add(_ignoreFileName);

        var rxOpt = (_ignoreCase ? RegexOptions.IgnoreCase : 0) | RegexOptions.Compiled;
        foreach (var p in patterns)
        {
            var rx = BuildGlobRegex(p, rxOpt);
            if (rx is not null) _protectedRegexes.Add(rx);
        }
    }

    /// <summary>glob ➜ Regex（支援 **、*、?；未以 "/" 開頭者允許任意前綴）</summary>
    private static Regex? BuildGlobRegex(string? pattern, RegexOptions options)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return null;

        var p = pattern.Replace('\\', '/').Trim();

        var anchored = p.StartsWith('/');
        if (anchored) p = p.TrimStart('/');

        var sb = new StringBuilder();
        if (!anchored) sb.Append(@"^(?:.*/)?"); else sb.Append('^');

        for (int i = 0; i < p.Length; i++)
        {
            var c = p[i];
            if (c == '*')
            {
                var isDouble = (i + 1 < p.Length && p[i + 1] == '*');
                if (isDouble) { sb.Append(".*"); i++; }
                else { sb.Append(@"[^/]*"); }
                continue;
            }
            if (c == '?') { sb.Append(@"[^/]"); continue; }

            // 正規表達式特殊字元跳脫
            if (".$^+()[]{}|".Contains(c)) sb.Append('\\').Append(c);
            else if (c == '/') sb.Append('/');
            else sb.Append(c);
        }

        sb.Append('$');
        return new Regex(sb.ToString(), options);
    }

    /* ====== 麵包屑 / 樹 / 清單 ====== */

    public List<(string Name, string VirtualPath)> BuildBreadcrumb(string virtualPath)
    {
        var v = NormalizePath(virtualPath);
        var list = new List<(string, string)> { ("/", "/") };
        if (v == "/") return list;

        var segs = v.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var acc = "";
        foreach (var s in segs)
        {
            acc = string.IsNullOrEmpty(acc) ? "/" + s : acc + "/" + s;
            list.Add((s, acc));
        }
        return list;
    }

    /// <summary>扁平的目錄樹（含 Depth）；根目錄固定第一筆。</summary>
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

                if (IsProtected(curV, name)) continue;

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
            .Where(t => !IsProtected(virtualPath, t.Item1))
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
                return (fi.Name, fi.Length, (DateTime?)fi.LastWriteTimeUtc);
            })
            .Where(t => !IsProtected(virtualPath, t.Item1))
            .ToList();
    }
}
