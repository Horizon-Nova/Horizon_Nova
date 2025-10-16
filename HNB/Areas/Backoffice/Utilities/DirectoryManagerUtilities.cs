using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace HNB.Areas.Backoffice.Utilities;

/// <summary>
/// 檔案管理服務層，負責處理檔案和資料夾的業務邏輯
/// 遵循統一架構模式：統一查詢 (Load**) + 基本 CRUD 操作 + ViewBag 設定
/// </summary>
public sealed class DirectoryManagerUtilities
{
    #region 統一的設定和欄位
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

    #region 建構子和初始化
    /// <summary>
    /// 建構子，初始化檔案管理服務
    /// </summary>
    /// <param name="cfg">配置物件</param>
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

    #region 統一查詢方法
    /// <summary>
    /// 載入資料夾清單
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <returns>資料夾清單</returns>
    public List<(string Name, DateTime? LastWriteUtc)> LoadFolders(string virtualPath)
    {
        var abs = GetSafeAbsolutePath(virtualPath);
        if (!Directory.Exists(abs)) return new();
        return Directory.EnumerateDirectories(abs)
            .OrderBy(Path.GetFileName)
            .Select(d => (Path.GetFileName(d), (DateTime?)Directory.GetLastWriteTimeUtc(d)))
            .Where(t => !IsProtected(virtualPath, t.Item1))
            .ToList();
    }

    /// <summary>
    /// 載入檔案清單
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <returns>檔案清單</returns>
    public List<(string Name, long Size, DateTime? LastWriteUtc)> LoadFiles(string virtualPath)
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

    /// <summary>
    /// 載入麵包屑導航
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <returns>麵包屑清單</returns>
    public List<(string Name, string VirtualPath)> LoadBreadcrumb(string virtualPath)
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

    /// <summary>
    /// 載入目錄樹結構
    /// </summary>
    /// <returns>目錄樹清單</returns>
    public List<(string Name, string VirtualPath, int Depth)> LoadTree()
    {
        var tree = new List<(string, string, int)> { ("根目錄", "/", 0) };
        try
        {
            Dfs("/", 1);
        }
        catch (Exception)
        {
            // 如果載入失敗，返回只有根目錄的列表
            // 避免整個頁面因為目錄樹載入失敗而崩潰
        }
        return tree;

        void Dfs(string curV, int depth)
        {
            var curAbs = GetSafeAbsolutePath(curV);
            if (!Directory.Exists(curAbs)) return;
            
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(curAbs).OrderBy(Path.GetFileName))
                {
                    var name = Path.GetFileName(dir);
                    if (IsProtected(curV, name)) continue;

                    var childV = curV == "/" ? "/" + name : curV + "/" + name;
                    tree.Add((name, childV, depth));
                    Dfs(childV, depth + 1);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 如果沒有權限訪問某個目錄，跳過它
            }
            catch (Exception)
            {
                // 其他錯誤也跳過，避免整個樹載入失敗
            }
        }
    }

    /// <summary>
    /// 載入文字檔案內容
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <param name="maxBytes">最大位元組數</param>
    /// <returns>檔案內容、編碼名稱和最後修改時間</returns>
    public (string Content, string EncodingName, DateTime? LastWriteUtc) LoadTextFile(string virtualPath, string fileName, long maxBytes = 1_048_576)
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
    #endregion

    #region 基本 CRUD 操作
    /// <summary>
    /// 建立資料夾
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="folderName">資料夾名稱</param>
    public void CreateFolder(string virtualPath, string folderName)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        Directory.CreateDirectory(absDir);

        // 移除 ASCII 限制，允許中文資料夾名稱
        // EnsureFolderAsciiOnlyOrThrow(folderName);

        var safe = SanitizeName(folderName);
        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("資料夾名稱不合法");
        if (IsProtected(virtualPath, safe)) throw new InvalidOperationException("保護路徑，禁止建立資料夾");

        var newDir = EnsureUniqueDirectory(Path.Combine(absDir, safe));
        Directory.CreateDirectory(newDir);
    }

    /// <summary>
    /// 建立空檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
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

    /// <summary>
    /// 刪除檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    public void DeleteFile(string virtualPath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new InvalidOperationException("檔案名稱不可為空");
        if (IsProtected(virtualPath, fileName)) throw new InvalidOperationException("保護路徑，禁止刪除");
        
        var absDir = GetSafeAbsolutePath(virtualPath);
        var safe = SanitizeName(fileName);
        
        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("檔案名稱不合法");
        
        var full = Path.Combine(absDir, safe);
        
        // 安全檢查：確保刪除的是檔案，且路徑包含檔案名稱
        if (!full.EndsWith(safe)) throw new InvalidOperationException("路徑驗證失敗");
        if (!File.Exists(full)) throw new FileNotFoundException($"檔案不存在: {fileName}");
        
        File.Delete(full);
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="folderName">資料夾名稱</param>
    public void DeleteFolder(string virtualPath, string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName)) throw new InvalidOperationException("資料夾名稱不可為空");
        if (IsProtected(virtualPath, folderName)) throw new InvalidOperationException("保護路徑，禁止刪除");
        
        var absDir = GetSafeAbsolutePath(virtualPath);
        var safe = SanitizeName(folderName);
        
        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("資料夾名稱不合法");
        
        var dir = Path.Combine(absDir, safe);
        
        // 安全檢查：確保刪除的是資料夾，且路徑包含資料夾名稱
        if (!dir.EndsWith(safe)) throw new InvalidOperationException("路徑驗證失敗");
        if (!Directory.Exists(dir)) throw new DirectoryNotFoundException($"資料夾不存在: {folderName}");
        
        // 額外安全檢查：確保不是刪除根目錄或父目錄
        if (dir == _root || dir.Length <= _root.Length) throw new InvalidOperationException("禁止刪除根目錄");
        
        Directory.Delete(dir, recursive: true);
    }

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="oldName">舊檔名</param>
    /// <param name="newName">新檔名</param>
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

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="oldName">舊資料夾名</param>
    /// <param name="newName">新資料夾名</param>
    public void RenameFolder(string virtualPath, string oldName, string newName)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        // 移除 ASCII 限制，允許中文資料夾名稱
        // EnsureFolderAsciiOnlyOrThrow(newName);

        var src = Path.Combine(absDir, SanitizeName(oldName));
        var newSafe = SanitizeName(newName);
        if (string.IsNullOrWhiteSpace(newSafe)) throw new InvalidOperationException("新資料夾名稱不合法");
        var dst = Path.Combine(absDir, newSafe);

        if (!Directory.Exists(src)) throw new DirectoryNotFoundException();
        if (IsProtected(virtualPath, oldName) || IsProtected(virtualPath, newSafe)) throw new InvalidOperationException("保護路徑，禁止重命名");
        if (Directory.Exists(dst)) throw new InvalidOperationException("目標資料夾已存在");

        Directory.Move(src, dst);
    }

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <param name="content">檔案內容</param>
    /// <param name="encodingName">編碼名稱</param>
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

    /// <summary>
    /// 查詢目錄統計資訊
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <returns>統計資訊 (資料夾數量, 檔案數量, 總大小, 最後修改時間)</returns>
    public (int FolderCount, int FileCount, long TotalSize, DateTime? LastModified) QueryStatistics(string virtualPath)
    {
        var folders = LoadFolders(virtualPath);
        var files = LoadFiles(virtualPath);
        
        var folderCount = folders.Count;
        var fileCount = files.Count;
        var totalSize = files.Sum(f => f.Size);
        var lastModified = files.Any() || folders.Any()
            ? files.Select(f => f.LastWriteUtc).Concat(folders.Select(f => f.LastWriteUtc))
                   .Where(d => d.HasValue).Max()
            : (DateTime?)null;
        
        return (folderCount, fileCount, totalSize, lastModified);
    }
    #endregion

    #region 上傳和下載
    /// <summary>
    /// 準備單一檔案上傳目標
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="originalFileName">原始檔名</param>
    /// <returns>絕對路徑和安全檔名</returns>
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

    /// <summary>
    /// 準備批量上傳目標（支援相對路徑）
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="relativePath">相對路徑</param>
    /// <returns>絕對路徑、安全檔名和目標虛擬目錄</returns>
    public (string AbsFilePath, string SafeFileName, string TargetVirtualDir) PrepareBatchUploadTarget(string virtualPath, string relativePath)
    {
        var rel = (relativePath ?? "").Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(rel)) throw new InvalidOperationException("檔名不合法");
        rel = WebUtility.HtmlDecode(rel);

        var safeRel = SanitizePathSegments(rel);
        if (string.IsNullOrWhiteSpace(safeRel)) throw new InvalidOperationException("檔名不合法");

        var segs = safeRel.Split('/', StringSplitOptions.RemoveEmptyEntries);
        // 移除 ASCII 限制，允許中文資料夾名稱
        // for (int i = 0; i < segs.Length - 1; i++) EnsureFolderAsciiOnlyOrThrow(segs[i]);

        var dirPart = Path.GetDirectoryName("/" + safeRel)?.Replace('\\', '/') ?? "/";
        var filePart = SanitizeName(Path.GetFileName(safeRel));

        var targetVDir = NormalizePath((virtualPath ?? "/").TrimEnd('/') + "/" + (dirPart?.Trim('/') ?? ""));
        if (IsProtected(targetVDir, filePart)) throw new InvalidOperationException("保護路徑，禁止上傳");

        var absDir = GetSafeAbsolutePath(targetVDir);
        Directory.CreateDirectory(absDir);

        var absTarget = EnsureUniqueFile(Path.Combine(absDir, filePart));
        return (absTarget, Path.GetFileName(absTarget), targetVDir);
    }

    /// <summary>
    /// 開啟檔案讀取
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>檔案串流、檔名和內容類型</returns>
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

    /// <summary>
    /// 開啟原始檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>檔案串流和內容類型</returns>
    public (Stream Stream, string ContentType) OpenRaw(string virtualPath, string fileName)
    {
        var (s, _, ct) = OpenRead(virtualPath, fileName);
        return (s, ct);
    }

    /// <summary>
    /// 壓縮資料夾為ZIP
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="folderName">資料夾名稱</param>
    /// <returns>ZIP串流、檔名和內容類型</returns>
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

    #region 輔助方法
    /// <summary>
    /// 檢查檔案是否可編輯
    /// </summary>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>是否可編輯</returns>
    public static bool IsEditable(string fileName) => EditableExt.Contains(Path.GetExtension(fileName));

    /// <summary>
    /// 標準化路徑
    /// </summary>
    /// <param name="path">原始路徑</param>
    /// <returns>標準化後的路徑</returns>
    private string NormalizePath(string? path)
    {
        var p = (path ?? "/").Replace('\\', '/').Trim();
        if (string.IsNullOrEmpty(p) || p == "/") return "/";
        if (!p.StartsWith('/')) p = "/" + p;
        while (p.Contains("//")) p = p.Replace("//", "/");
        var segs = p.Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => s != "." && s != "..");
        return "/" + string.Join('/', segs);
    }

    /// <summary>
    /// 取得安全的絕對路徑
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <returns>安全的絕對路徑</returns>
    private string GetSafeAbsolutePath(string virtualPath)
    {
        Directory.CreateDirectory(_root);
        var normalized = NormalizePath(virtualPath);
        var combined = Path.GetFullPath(Path.Combine(_root, "." + normalized));
        var cmp = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!combined.StartsWith(_root, cmp))
            throw new InvalidOperationException("路徑不安全");
        return combined;
    }

    /// <summary>
    /// 清理檔案/資料夾名稱
    /// </summary>
    /// <param name="raw">原始名稱</param>
    /// <returns>清理後的名稱</returns>
    private string SanitizeName(string raw)
    {
        var name = WebUtility.HtmlDecode(raw ?? string.Empty).Trim();
        if (name is "." or "..") return "";
        name = InvalidChars.Replace(name, "");
        if (name.Length > 128) name = name[..128];
        return name;
    }

    /// <summary>
    /// 清理路徑片段
    /// </summary>
    /// <param name="relPath">相對路徑</param>
    /// <returns>清理後的路徑</returns>
    private string SanitizePathSegments(string relPath)
    {
        var parts = relPath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                           .Select(SanitizeName)
                           .Where(s => !string.IsNullOrWhiteSpace(s));
        return string.Join('/', parts);
    }

    /// <summary>
    /// 確保檔案名稱唯一
    /// </summary>
    /// <param name="absPath">絕對路徑</param>
    /// <returns>唯一的檔案路徑</returns>
    private string EnsureUniqueFile(string absPath)
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

    /// <summary>
    /// 確保資料夾名稱唯一
    /// </summary>
    /// <param name="absDir">絕對路徑</param>
    /// <returns>唯一的資料夾路徑</returns>
    private string EnsureUniqueDirectory(string absDir)
    {
        if (!Directory.Exists(absDir)) return absDir;
        var parent = Path.GetDirectoryName(absDir)!;
        var name = Path.GetFileName(absDir);
        var i = 1; string candidate;
        do { candidate = Path.Combine(parent, $"{name} ({i++})"); }
        while (Directory.Exists(candidate));
        return candidate;
    }

    /// <summary>
    /// 確保資料夾名稱符合ASCII規範
    /// </summary>
    /// <param name="rawName">原始名稱</param>
    /// <exception cref="InvalidOperationException">當名稱不符合規範時拋出</exception>
    private void EnsureFolderAsciiOnlyOrThrow(string? rawName)
    {
        var name = WebUtility.HtmlDecode(rawName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("資料夾名稱不可為空");
        if (!FolderAsciiRegex.IsMatch(name))
            throw new InvalidOperationException("資料夾名稱僅限英文/數字/空白/.-_（不允許中文或其他非 ASCII 字元）");
    }

    /// <summary>
    /// 檢查路徑是否受保護
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="name">檔案或資料夾名稱</param>
    /// <returns>是否受保護</returns>
    private bool IsProtected(string virtualPath, string? name = null)
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

    /// <summary>
    /// 建立忽略規則快取
    /// </summary>
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

    /// <summary>
    /// 建立Glob規則的Regex
    /// </summary>
    /// <param name="pattern">Glob模式</param>
    /// <param name="options">Regex選項</param>
    /// <returns>編譯後的Regex</returns>
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
}

