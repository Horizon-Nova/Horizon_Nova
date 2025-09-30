using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace HNB.Areas.Backoffice.Utilities;

public sealed class DirectoryManagerUtilities
{
    #region 設定 & 欄位
    private readonly string _root;
    private readonly bool _ignoreCase;
    private readonly string[] _ignorePatternsFromConfig;
    private readonly string _ignoreFileName;
    private readonly List<Regex> _protectedRegexes = new();
    private static readonly Regex FolderAsciiRegex = new(@"^[A-Za-z0-9 _.\-]+$", RegexOptions.Compiled);
    private static readonly Regex InvalidChars = new(@"[\\/:*?""<>|]", RegexOptions.Compiled);
    private static readonly FileExtensionContentTypeProvider Ct = new();

    // 可線上檢視/編輯的純文字副檔名
    private static readonly HashSet<string> EditableExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",".md",".json",".jsonl",".js",".ts",".css",".scss",".sass",
        ".cs",".cshtml",".html",".htm",".xml",".yml",".yaml",".ini",".conf",".cfg",
        ".log",".py",".sh",".bat",".ps1",".sql",".env",".properties",".toml",".gitignore",".editorconfig"
    };
    #endregion

    #region 建構子
    public DirectoryManagerUtilities(IConfiguration cfg)
    {
        _root = Path.GetFullPath(cfg["Storage:Root"] ?? "/app/storage");
        _ignoreCase = cfg.GetValue<bool>("Storage:IgnoreCase", true);
        _ignoreFileName = cfg["Storage:IgnoreFileName"] ?? ".backofficeignore";
        _ignorePatternsFromConfig = cfg.GetSection("Storage:IgnorePatterns").Get<string[]>() ?? Array.Empty<string>();
        Directory.CreateDirectory(_root);
        BuildIgnoreRegexCache();
    }
    #endregion

    #region Path / Name Utilities
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
        var name = WebUtility.HtmlDecode(raw ?? string.Empty).Trim();
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

    // 資料夾命名：僅允許 英數/空白/.-_（不允許中文或非 ASCII）
    public void EnsureFolderAsciiOnlyOrThrow(string? rawName)
    {
        var name = WebUtility.HtmlDecode(rawName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("資料夾名稱不可為空");
        if (!FolderAsciiRegex.IsMatch(name))
            throw new InvalidOperationException("資料夾名稱僅限英文/數字/空白/.-_（不允許中文或其他非 ASCII 字元）");
    }
    #endregion

    #region Ignore / Protect 名單（.gitignore 風格）
    public bool IsProtected(string virtualPath, string? name = null)
    {
        if (_protectedRegexes.Count == 0) return false;

        var v = NormalizePath(virtualPath);
        var rel = (v == "/" ? "" : v.TrimStart('/') + "/") + (name ?? "");
        rel = rel.Trim('/');

        // 當作資料夾語意再試一次（尾斜線）
        var folderRel = rel.Length == 0 ? "" : rel + "/";

        foreach (var rx in _protectedRegexes)
            if (rx.IsMatch(rel) || (folderRel.Length > 0 && rx.IsMatch(folderRel)))
                return true;

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
            if (".$^+()[]{}|".Contains(c)) sb.Append('\\').Append(c);
            else if (c == '/') sb.Append('/');
            else sb.Append(c);
        }

        sb.Append('$');
        return new Regex(sb.ToString(), options);
    }
    #endregion

    #region 麵包屑 / 樹 / 清單
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
        if (!Directory.Exists(abs)) return new();
        return Directory.EnumerateDirectories(abs)
            .OrderBy(Path.GetFileName)
            .Select(d => (Path.GetFileName(d), (DateTime?)Directory.GetLastWriteTimeUtc(d)))
            .Where(t => !IsProtected(virtualPath, t.Item1))
            .ToList();
    }

    public List<(string Name, long Size, DateTime? LastWriteUtc)> ListFiles(string virtualPath)
    {
        var abs = GetSafeAbsolutePath(virtualPath);
        if (!Directory.Exists(abs)) return new();
        return Directory.EnumerateFiles(abs)
            .OrderBy(Path.GetFileName)
            .Select(f => {
                var fi = new FileInfo(f);
                return (fi.Name, fi.Length, (DateTime?)fi.LastWriteTimeUtc);
            })
            .Where(t => !IsProtected(virtualPath, t.Item1))
            .ToList();
    }
    #endregion

    #region 上傳目標計算（僅回傳安全目標路徑，實際寫入由上層處理）
    public (string AbsFilePath, string SafeFileName) PrepareSingleUploadTarget(string virtualPath, string originalFileName)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(absDir);

        var safeName = SanitizeName(originalFileName);
        if (string.IsNullOrWhiteSpace(safeName)) throw new InvalidOperationException("檔名不合法");
        if (IsProtected(virtualPath, safeName)) throw new InvalidOperationException("保護路徑，禁止上傳");

        var absTarget = EnsureUniqueFile(Path.Combine(absDir, safeName));
        return (absTarget, Path.GetFileName(absTarget));
    }

    // 支援「相對路徑」上傳（含子資料夾）
    public (string AbsFilePath, string SafeFileName, string TargetVirtualDir) PrepareBatchUploadTarget(string virtualPath, string relativePath)
    {
        var rel = (relativePath ?? "").Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(rel)) throw new InvalidOperationException("檔名不合法");
        rel = WebUtility.HtmlDecode(rel);

        var safeRel = SanitizePathSegments(rel);
        if (string.IsNullOrWhiteSpace(safeRel)) throw new InvalidOperationException("檔名不合法");

        var segs = safeRel.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < segs.Length - 1; i++) EnsureFolderAsciiOnlyOrThrow(segs[i]);

        var dirPart = Path.GetDirectoryName("/" + safeRel)?.Replace('\\', '/') ?? "/";
        var filePart = SanitizeName(Path.GetFileName(safeRel));

        var targetVDir = NormalizePath((virtualPath ?? "/").TrimEnd('/') + "/" + (dirPart?.Trim('/') ?? ""));
        if (IsProtected(targetVDir, filePart)) throw new InvalidOperationException("保護路徑，禁止上傳");

        var absDir = GetSafeAbsolutePath(targetVDir);
        Directory.CreateDirectory(absDir);

        var absTarget = EnsureUniqueFile(Path.Combine(absDir, filePart));
        return (absTarget, Path.GetFileName(absTarget), targetVDir);
    }
    #endregion

    #region 檔案/資料夾 操作（建立 / 刪除 / 重新命名）
    public void CreateFolder(string virtualPath, string folderName)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(absDir);

        EnsureFolderAsciiOnlyOrThrow(folderName);

        var safe = SanitizeName(folderName);
        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("資料夾名稱不合法");
        if (IsProtected(virtualPath, safe)) throw new InvalidOperationException("保護路徑，禁止建立資料夾");

        var newDir = EnsureUniqueDirectory(Path.Combine(absDir, safe));
        Directory.CreateDirectory(newDir);
    }

    public void CreateEmptyFile(string virtualPath, string fileName)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(absDir);

        var safe = SanitizeName(fileName);
        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("檔名不合法");
        if (IsProtected(virtualPath, safe)) throw new InvalidOperationException("保護路徑，禁止建立檔案");

        var target = EnsureUniqueFile(Path.Combine(absDir, safe));
        File.WriteAllBytes(target, Array.Empty<byte>());
    }

    public void DeleteFile(string virtualPath, string fileName)
    {
        if (IsProtected(virtualPath, fileName)) throw new InvalidOperationException("保護路徑，禁止刪除");
        var absDir = GetSafeAbsolutePath(virtualPath);
        var safe = SanitizeName(fileName);
        var full = Path.Combine(absDir, safe);
        if (File.Exists(full)) File.Delete(full);
    }

    public void DeleteFolder(string virtualPath, string folderName)
    {
        if (IsProtected(virtualPath, folderName)) throw new InvalidOperationException("保護路徑，禁止刪除");
        var absDir = GetSafeAbsolutePath(virtualPath);
        var safe = SanitizeName(folderName);
        var dir = Path.Combine(absDir, safe);
        if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
    }

    public void RenameFile(string virtualPath, string oldName, string newName)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        var src = Path.Combine(absDir, SanitizeName(oldName));
        var newSafe = SanitizeName(newName);
        if (string.IsNullOrWhiteSpace(newSafe)) throw new InvalidOperationException("新檔名不合法");
        var dst = Path.Combine(absDir, newSafe);

        if (!File.Exists(src)) throw new FileNotFoundException();
        if (IsProtected(virtualPath, oldName) || IsProtected(virtualPath, newSafe)) throw new InvalidOperationException("保護路徑，禁止重命名");
        if (File.Exists(dst)) throw new InvalidOperationException("目標檔名已存在");

        File.Move(src, dst);
    }

    public void RenameFolder(string virtualPath, string oldName, string newName)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        EnsureFolderAsciiOnlyOrThrow(newName);

        var src = Path.Combine(absDir, SanitizeName(oldName));
        var newSafe = SanitizeName(newName);
        if (string.IsNullOrWhiteSpace(newSafe)) throw new InvalidOperationException("新資料夾名稱不合法");
        var dst = Path.Combine(absDir, newSafe);

        if (!Directory.Exists(src)) throw new DirectoryNotFoundException();
        if (IsProtected(virtualPath, oldName) || IsProtected(virtualPath, newSafe)) throw new InvalidOperationException("保護路徑，禁止重命名");
        if (Directory.Exists(dst)) throw new InvalidOperationException("目標資料夾已存在");

        Directory.Move(src, dst);
    }
    #endregion

    #region 讀取 / 下載 / Zip
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

    public (Stream Stream, string ContentType) OpenRaw(string virtualPath, string fileName)
    {
        var (s, _, ct) = OpenRead(virtualPath, fileName);
        return (s, ct);
    }

    public (Stream Stream, string FileName, string ContentType) ZipFolder(string virtualPath, string folderName)
    {
        if (IsProtected(virtualPath, folderName)) throw new InvalidOperationException("保護路徑，禁止壓縮");

        var absParent = GetSafeAbsolutePath(virtualPath);
        var safe = SanitizeName(folderName);
        var absTargetDir = Path.Combine(absParent, safe);
        if (!Directory.Exists(absTargetDir)) throw new DirectoryNotFoundException("資料夾不存在");

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
                    if (IsProtected(vDir, name)) continue;
                    var nextV = vDir == "/" ? "/" + name : vDir + "/" + name;
                    AddDir(dir, nextV, rel + name + "/");
                }
                foreach (var file in Directory.EnumerateFiles(absDir))
                {
                    var name = Path.GetFileName(file);
                    if (IsProtected(vDir, name)) continue;
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
    #endregion

    #region 文字檔 Read / Save（含 BOM 偵測）
    public static bool IsEditable(string fileName) => EditableExt.Contains(Path.GetExtension(fileName));

    public (string Content, string EncodingName, DateTime? LastWriteUtc) ReadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
    {
        if (!IsEditable(fileName))
            throw new InvalidOperationException("此檔案類型不支援線上查看/編輯");

        var absDir = GetSafeAbsolutePath(virtualPath);
        var safe = SanitizeName(fileName);
        var full = Path.Combine(absDir, safe);
        if (!File.Exists(full)) throw new FileNotFoundException();

        var fi = new FileInfo(full);
        if (fi.Length > maxBytes)
            throw new InvalidOperationException($"檔案過大（{fi.Length:N0} bytes），超過上限 {maxBytes:N0}。");

        using var fs = File.OpenRead(full);
        var preamble = new byte[4];
        var read = fs.Read(preamble, 0, 4);
        fs.Position = 0;

        Encoding enc = Encoding.UTF8;
        if (read >= 3 && preamble[0] == 0xEF && preamble[1] == 0xBB && preamble[2] == 0xBF) enc = new UTF8Encoding(true);
        else if (read >= 2 && preamble[0] == 0xFF && preamble[1] == 0xFE) enc = Encoding.Unicode;
        else if (read >= 2 && preamble[0] == 0xFE && preamble[1] == 0xFF) enc = Encoding.BigEndianUnicode;

        using var sr = new StreamReader(fs, enc, detectEncodingFromByteOrderMarks: true);
        var text = sr.ReadToEnd();
        return (text, enc.WebName, fi.LastWriteTimeUtc);
    }

    public void SaveTextFile(string virtualPath, string fileName, string content, string? encodingName = "utf-8")
    {
        if (IsProtected(virtualPath, fileName))
            throw new InvalidOperationException("保護路徑，禁止寫入");
        if (!IsEditable(fileName))
            throw new InvalidOperationException("此檔案類型不勻線上編輯");

        var absDir = GetSafeAbsolutePath(virtualPath);
        var safe = SanitizeName(fileName);
        var full = Path.Combine(absDir, safe);
        if (!File.Exists(full)) throw new FileNotFoundException();

        var enc = encodingName?.Equals("utf-8-bom", StringComparison.OrdinalIgnoreCase) == true
            ? new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
            : Encoding.UTF8;

        File.WriteAllText(full, content ?? string.Empty, enc);
    }
    #endregion

}

