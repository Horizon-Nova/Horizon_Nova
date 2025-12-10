using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using HNB.Areas.Backoffice.Dtos;

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

    // 延遲載入 HttpContextAccessor（取得當前使用者）
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region 建構子和初始化
    /// <summary>
    /// 建構子，初始化檔案管理服務
    /// </summary>
    /// <param name="cfg">配置物件</param>
    /// <param name="serviceProvider">服務提供者</param>
    public DirectoryManagerUtilities(IConfiguration cfg, IServiceProvider serviceProvider)
    {
        _root = Path.GetFullPath(cfg["Storage:Root"] ?? "/app/storage");
        _ignoreCase = cfg.GetValue<bool>("Storage:IgnoreCase", true);
        _ignoreFileName = cfg["Storage:IgnoreFileName"] ?? ".backofficeignore";
        _ignorePatternsFromConfig = cfg.GetSection("Storage:IgnorePatterns").Get<string[]>() ?? Array.Empty<string>();
        _serviceProvider = serviceProvider;
        Directory.CreateDirectory(_root);
        BuildIgnoreRegexCache();
    }
    #endregion

    #region 查詢方法

    /// <summary>
    /// 直接從檔案系統載入檔案管理項目（不依賴資料庫）
    /// 只回傳當前用戶有權限的項目
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <returns>檔案管理項目清單</returns>
    public List<FileSystemEntry> LoadFileSystemItems(string virtualPath)
    {
        var items = new List<FileSystemEntry>();
        var absDir = GetSafeAbsolutePath(virtualPath);

        if (!Directory.Exists(absDir)) return items;

        var currentUser = GetCurrentUserOrNull();

        // 載入資料夾
        foreach (var dir in Directory.GetDirectories(absDir))
        {
            var folderName = Path.GetFileName(dir);
            if (IsProtected(virtualPath, folderName)) continue;

            var owners = GetAppOwners(dir);

            // 使用統一權限檢查
            if (string.IsNullOrWhiteSpace(currentUser) || !HasUserPermission(dir, currentUser))
                continue;

            var primaryOwner = owners.Length > 0 ? owners[0] : "";
            var dirInfo = new DirectoryInfo(dir);

            items.Add(new FileSystemEntry
            {
                Name = folderName,
                Type = "folder",
                Size = null,
                MimeType = null,
                Owner = primaryOwner,
                SharedUsers = owners.Length > 0 ? owners.ToList() : new List<string>(),
                CreatedAt = dirInfo.CreationTimeUtc,
                UpdatedAt = dirInfo.LastWriteTimeUtc,
                VirtualPath = virtualPath
            });
        }

        // 載入檔案
        foreach (var file in Directory.GetFiles(absDir))
        {
            var fileName = Path.GetFileName(file);
            if (IsProtected(virtualPath, fileName)) continue;

            var owners = GetAppOwners(file);

            // 使用統一權限檢查
            if (string.IsNullOrWhiteSpace(currentUser) || !HasUserPermission(file, currentUser))
                continue;

            var primaryOwner = owners.Length > 0 ? owners[0] : "";
            var fileInfo = new FileInfo(file);

            items.Add(new FileSystemEntry
            {
                Name = fileName,
                Type = "file",
                Size = fileInfo.Length,
                MimeType = GetMimeType(fileName),
                Owner = primaryOwner,
                SharedUsers = owners.Length > 0 ? owners.ToList() : new List<string>(),
                CreatedAt = fileInfo.CreationTimeUtc,
                UpdatedAt = fileInfo.LastWriteTimeUtc,
                VirtualPath = virtualPath
            });
        }

        return items.OrderBy(i => i.Type).ThenBy(i => i.Name).ToList();
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
    /// 載入目錄樹結構（含權限控制）
    /// 只回傳當前用戶有權限的資料夾
    /// </summary>
    /// <returns>目錄樹清單</returns>
    public List<(string Name, string VirtualPath, int Depth)> LoadTree()
    {
        var tree = new List<(string, string, int)>();
        var currentUser = GetCurrentUserOrNull();

        try
        {
            Dfs("/", 1);
        }
        catch (Exception)
        {
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

                    // 權限控制：檢查是否為擁有者
                    var owners = GetAppOwners(dir);
                    if (owners.Length > 0 && currentUser != null)
                    {
                        if (!owners.Any(o => o.Equals(currentUser, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue; // 不是擁有者，跳過
                        }
                    }

                    var childV = curV == "/" ? "/" + name : curV + "/" + name;
                    tree.Add((name, childV, depth));
                    Dfs(childV, depth + 1);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (Exception)
            {
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
            throw new InvalidOperationException("此檔案類型不支援線上編輯");

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
    public void InsertFolder(FileSystemEntry entry)
    {
        var absDir = GetSafeAbsolutePath(entry.VirtualPath);
        Directory.CreateDirectory(absDir);
        var safe = SanitizeName(entry.Name);
        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("資料夾名稱不合法");
        if (IsProtected(entry.VirtualPath, safe)) throw new InvalidOperationException("保護路徑，禁止建立資料夾");

        var newDir = EnsureUniqueDirectory(Path.Combine(absDir, safe));
        Directory.CreateDirectory(newDir);

        // 設定擁有者
        var owner = GetCurrentUserOrNull();
        if (!string.IsNullOrWhiteSpace(owner))
        {
            SetAppOwners(newDir, new[] { owner });
        }

        // 檢查父資料夾是否有待刪除標記（誰先誰得 + 強制同步）
        var parentMarked = IsParentMarkedForDeletion(newDir);
        if (!string.IsNullOrEmpty(parentMarked))
        {
            MarkForDeletion(newDir);
        }
    }

    /// <summary>
    /// 建立空檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    public void InsertFile(FileSystemEntry entry)
    {
        var absDir = GetSafeAbsolutePath(entry.VirtualPath);
        Directory.CreateDirectory(absDir);

        var safe = SanitizeName(entry.Name);
        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("檔名不合法");
        if (IsProtected(entry.VirtualPath, safe)) throw new InvalidOperationException("保護路徑，禁止建立檔案");

        var target = EnsureUniqueFile(Path.Combine(absDir, safe));
        File.WriteAllBytes(target, Array.Empty<byte>());

        // 設定擁有者
        var owner = GetCurrentUserOrNull();
        if (!string.IsNullOrWhiteSpace(owner))
        {
            SetAppOwners(target, new[] { owner });
        }

        // 檢查父資料夾是否有待刪除標記（誰先誰得 + 強制同步）
        var parentMarked = IsParentMarkedForDeletion(target);
        if (!string.IsNullOrEmpty(parentMarked))
        {
            MarkForDeletion(target);
        }
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    public void DeleteFile(FileSystemEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Name)) throw new InvalidOperationException("檔案名稱不可為空");
        if (IsProtected(entry.VirtualPath, entry.Name)) throw new InvalidOperationException("保護路徑，禁止刪除");

        var absDir = GetSafeAbsolutePath(entry.VirtualPath);
        var safe = SanitizeName(entry.Name);

        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("檔案名稱不合法");

        var full = Path.Combine(absDir, safe);

        if (!full.EndsWith(safe)) throw new InvalidOperationException("路徑驗證失敗");
        if (!File.Exists(full)) throw new FileNotFoundException($"檔案不存在: {entry.Name}");

        const int maxRetries = 3;
        const int retryDelayMs = 500;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                File.Delete(full);
                UnmarkForDeletion(full);
                return;
            }
            catch (IOException) when (attempt < maxRetries - 1)
            {
                MarkForDeletion(full);
                System.Threading.Thread.Sleep(retryDelayMs);
            }
            catch (UnauthorizedAccessException) when (attempt < maxRetries - 1)
            {
                MarkForDeletion(full);
                System.Threading.Thread.Sleep(retryDelayMs);
            }
        }

        MarkForDeletion(full);
        throw new IOException($"檔案正在使用中，無法刪除: {entry.Name}。已標記為待刪除，將在背景服務中重試。");
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="folderName">資料夾名稱</param>
    public void DeleteFolder(FileSystemEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Name)) throw new InvalidOperationException("資料夾名稱不可為空");
        if (IsProtected(entry.VirtualPath, entry.Name)) throw new InvalidOperationException("保護路徑，禁止刪除");

        var absDir = GetSafeAbsolutePath(entry.VirtualPath);
        var safe = SanitizeName(entry.Name);

        if (string.IsNullOrWhiteSpace(safe)) throw new InvalidOperationException("資料夾名稱不合法");

        var dir = Path.Combine(absDir, safe);

        if (!dir.EndsWith(safe)) throw new InvalidOperationException("路徑驗證失敗");
        if (!Directory.Exists(dir)) throw new DirectoryNotFoundException($"資料夾不存在: {entry.Name}");

        if (dir == _root || dir.Length <= _root.Length) throw new InvalidOperationException("禁止刪除根目錄");

        const int maxRetries = 3;
        const int retryDelayMs = 500;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                Directory.Delete(dir, recursive: true);
                UnmarkForDeletion(dir);
                return;
            }
            catch (IOException) when (attempt < maxRetries - 1)
            {
                MarkForDeletion(dir);
                System.Threading.Thread.Sleep(retryDelayMs);
            }
            catch (UnauthorizedAccessException) when (attempt < maxRetries - 1)
            {
                MarkForDeletion(dir);
                System.Threading.Thread.Sleep(retryDelayMs);
            }
        }

        MarkForDeletion(dir);
        throw new IOException($"資料夾正在使用中，無法刪除: {entry.Name}。已標記為待刪除，將在背景服務中重試。");
    }

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="oldName">舊檔名</param>
    /// <param name="newName">新檔名</param>
    public void UpdateFile(FileSystemEntry entry, string newName)
    {
        var absDir = GetSafeAbsolutePath(entry.VirtualPath);
        var src = Path.Combine(absDir, SanitizeName(entry.Name));
        var newSafe = SanitizeName(newName);
        if (string.IsNullOrWhiteSpace(newSafe)) throw new InvalidOperationException("新檔名不合法");
        var dst = Path.Combine(absDir, newSafe);

        if (!File.Exists(src)) throw new FileNotFoundException();
        if (IsProtected(entry.VirtualPath, entry.Name) || IsProtected(entry.VirtualPath, newSafe)) throw new InvalidOperationException("保護路徑，禁止重命名");
        if (File.Exists(dst)) throw new InvalidOperationException("目標檔名已存在");

        File.Move(src, dst);
        // 應用程式擁有者儲存在 ADS/xattr，會隨檔案一起移動
    }

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="oldName">舊資料夾名</param>
    /// <param name="newName">新資料夾名</param>
    public void UpdateFolder(FileSystemEntry entry, string newName)
    {
        var absDir = GetSafeAbsolutePath(entry.VirtualPath);

        var src = Path.Combine(absDir, SanitizeName(entry.Name));
        var newSafe = SanitizeName(newName);
        if (string.IsNullOrWhiteSpace(newSafe)) throw new InvalidOperationException("新資料夾名稱不合法");
        var dst = Path.Combine(absDir, newSafe);

        if (!Directory.Exists(src)) throw new DirectoryNotFoundException();
        if (IsProtected(entry.VirtualPath, entry.Name) || IsProtected(entry.VirtualPath, newSafe)) throw new InvalidOperationException("保護路徑，禁止重命名");
        if (Directory.Exists(dst)) throw new InvalidOperationException("目標資料夾已存在");

        Directory.Move(src, dst);
        // 應用程式擁有者儲存在 ADS/xattr，會隨資料夾一起移動
    }

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="fileName">檔案名稱</param>
    /// <param name="content">檔案內容</param>
    /// <param name="encodingName">編碼名稱</param>
    public void UpdateTextFile(FileSystemEntry entry, string content, string? encodingName = "utf-8")
    {
        if (IsProtected(entry.VirtualPath, entry.Name))
            throw new InvalidOperationException("保護路徑，禁止寫入");
        if (!IsEditable(entry.Name))
            throw new InvalidOperationException("此檔案類型不支援線上編輯");

        var absDir = GetSafeAbsolutePath(entry.VirtualPath);
        var safe = SanitizeName(entry.Name);
        var full = Path.Combine(absDir, safe);
        if (!File.Exists(full)) throw new FileNotFoundException();

        var enc = encodingName?.Equals("utf-8-bom", StringComparison.OrdinalIgnoreCase) == true
            ? new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
            : Encoding.UTF8;

        File.WriteAllText(full, content ?? string.Empty, enc);
        // 編輯檔案不影響擁有者（ADS/xattr 會保留）
    }

    /// <summary>
    /// 載入目錄統計資訊
    /// </summary>
    public (int FolderCount, int FileCount, long TotalSize, DateTime? LastModified) LoadStatistics(string virtualPath)
    {
        var items = LoadFileSystemItems(virtualPath);
        var folders = items.Where(i => i.Type == "folder").ToList();
        var files = items.Where(i => i.Type == "file").ToList();

        var folderCount = folders.Count;
        var fileCount = files.Count;
        var totalSize = files.Sum(f => f.Size ?? 0L);
        var lastModified = files.Any() || folders.Any()
            ? files.Select(f => f.UpdatedAt).Concat(folders.Select(f => f.UpdatedAt))
                   .Where(d => d != default).DefaultIfEmpty().Max()
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
    /// 上傳單個檔案（包含設定擁有者和檢查待刪除標記）
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="relativePath">相對路徑</param>
    /// <param name="fileStream">檔案串流</param>
    /// <param name="owners">擁有者列表</param>
    public void UploadFile(string virtualPath, string relativePath, Stream fileStream, string[] owners)
    {
        var (absPath, _, _) = PrepareBatchUploadTarget(virtualPath, relativePath);
        
        using (var stream = new FileStream(absPath, FileMode.Create))
        {
            fileStream.CopyTo(stream);
        }
        
        if (owners != null && owners.Length > 0)
        {
            SetAppOwners(absPath, owners);
        }

        var parentMarked = IsParentMarkedForDeletion(absPath);
        if (!string.IsNullOrEmpty(parentMarked))
        {
            MarkForDeletion(absPath);
        }
    }

    /// <summary>
    /// 取得 chunk 暫存目錄路徑
    /// </summary>
    private string GetChunkTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "FileManagerChunks");
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    /// <summary>
    /// 儲存檔案分塊到暫存目錄
    /// </summary>
    /// <param name="uploadId">上傳唯一識別碼</param>
    /// <param name="chunkIndex">分塊索引</param>
    /// <param name="chunkStream">分塊串流</param>
    public void SaveChunk(string uploadId, int chunkIndex, Stream chunkStream)
    {
        var tempDir = GetChunkTempDirectory();
        var chunkPath = Path.Combine(tempDir, $"{uploadId}_{chunkIndex}.chunk");
        
        using (var fileStream = new FileStream(chunkPath, FileMode.Create, FileAccess.Write))
        {
            chunkStream.CopyTo(fileStream);
        }
    }

    /// <summary>
    /// 檢查所有分塊是否都已上傳
    /// </summary>
    /// <param name="uploadId">上傳唯一識別碼</param>
    /// <param name="totalChunks">總分塊數</param>
    /// <returns>已接收的分塊數</returns>
    public int GetReceivedChunksCount(string uploadId, int totalChunks)
    {
        var tempDir = GetChunkTempDirectory();
        var count = 0;
        
        for (int i = 0; i < totalChunks; i++)
        {
            var chunkPath = Path.Combine(tempDir, $"{uploadId}_{i}.chunk");
            if (File.Exists(chunkPath))
            {
                count++;
            }
        }
        
        return count;
    }

    /// <summary>
    /// 合併所有分塊成完整檔案並上傳到目標位置
    /// </summary>
    /// <param name="uploadId">上傳唯一識別碼</param>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="relativePath">相對路徑</param>
    /// <param name="totalChunks">總分塊數</param>
    /// <param name="owners">擁有者列表</param>
    public void MergeChunks(string uploadId, string virtualPath, string relativePath, int totalChunks, string[] owners)
    {
        var tempDir = GetChunkTempDirectory();
        var (absPath, _, _) = PrepareBatchUploadTarget(virtualPath, relativePath);
        
        try
        {
            using (var outputStream = new FileStream(absPath, FileMode.Create, FileAccess.Write))
            {
                for (int i = 0; i < totalChunks; i++)
                {
                    var chunkPath = Path.Combine(tempDir, $"{uploadId}_{i}.chunk");
                    if (!File.Exists(chunkPath))
                    {
                        throw new FileNotFoundException($"分塊 {i} 不存在: {chunkPath}");
                    }
                    
                    using (var chunkStream = new FileStream(chunkPath, FileMode.Open, FileAccess.Read))
                    {
                        chunkStream.CopyTo(outputStream);
                    }
                }
            }
            
            if (owners != null && owners.Length > 0)
            {
                SetAppOwners(absPath, owners);
            }

            var parentMarked = IsParentMarkedForDeletion(absPath);
            if (!string.IsNullOrEmpty(parentMarked))
            {
                MarkForDeletion(absPath);
            }
        }
        finally
        {
            // 清理所有分塊檔案
            for (int i = 0; i < totalChunks; i++)
            {
                var chunkPath = Path.Combine(tempDir, $"{uploadId}_{i}.chunk");
                try
                {
                    if (File.Exists(chunkPath))
                    {
                        File.Delete(chunkPath);
                    }
                }
                catch
                {
                    // 忽略清理錯誤
                }
            }
        }
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
    /// 取得目前使用者名稱（若無登入則回傳 null）
    /// </summary>
    private string? GetCurrentUserOrNull()
    {
        var accessor = _serviceProvider.GetService(typeof(Microsoft.AspNetCore.Http.IHttpContextAccessor)) as Microsoft.AspNetCore.Http.IHttpContextAccessor;
        var user = accessor?.HttpContext?.User?.Identity?.Name;
        return string.IsNullOrWhiteSpace(user) ? null : user;
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

    /// <summary>
    /// 取得 MIME 類型
    /// </summary>
    private string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            ".md" => "text/markdown",
            ".csv" => "text/csv",
            _ => "application/octet-stream"
        };
    }


    #region 應用程式擁有者管理（跨平台）

    /// <summary>
    /// Linux: 設定應用程式擁有者（使用 xattr）
    /// </summary>
    private static void SetAppOwnerLinux(string path, string jsonData)
    {
        try
        {
            // 使用 Mono.Posix.NETStandard 套件設定 extended attribute
            var bytes = Encoding.UTF8.GetBytes(jsonData);
            var result = Mono.Unix.Native.Syscall.setxattr(
                path,
                "user.AppOwner",
                bytes,
                (ulong)bytes.Length,
                0 // 0 = create or replace; avoid invalid combination of CREATE|REPLACE
            );

            if (result != 0)
            {
                var errno = Mono.Unix.Native.Stdlib.GetLastError();
                throw new InvalidOperationException($"Linux 設定應用程式擁有者失敗: {errno}");
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Linux 設定應用程式擁有者失敗: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Linux: 取得應用程式擁有者（使用 xattr）
    /// </summary>
    private static string GetAppOwnerLinux(string path)
    {
        try
        {
            // 先取得屬性大小
            var size = Mono.Unix.Native.Syscall.getxattr(path, "user.AppOwner", null, 0);
            if (size < 0)
            {
                // 屬性不存在
                return null;
            }

            // 讀取屬性值
            var buffer = new byte[size];
            var result = Mono.Unix.Native.Syscall.getxattr(path, "user.AppOwner", buffer, (ulong)size);

            if (result < 0)
            {
                return null;
            }

            var value = Encoding.UTF8.GetString(buffer, 0, (int)result);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        catch
        {
            // 讀取失敗時返回 null，不拋出異常
            return null;
        }
    }

    /// <summary>
    /// 設定應用程式層級的擁有者（多個擁有者，不影響系統擁有者）
    /// Windows: 使用 Alternate Data Stream (ADS)
    /// Linux: 使用 Extended Attributes (xattr)
    /// </summary>
    /// <param name="path">檔案或資料夾路徑</param>
    /// <param name="appOwners">應用程式擁有者名稱列表</param>
    public static void SetAppOwners(string path, string[] appOwners)
    {
        if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
            throw new FileNotFoundException($"路徑不存在: {path}");

        if (appOwners == null || appOwners.Length == 0)
            throw new ArgumentException("擁有者列表不可為空");

        // 使用 JSON 序列化多個擁有者
        var json = JsonSerializer.Serialize(appOwners);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: 使用 Alternate Data Stream 儲存
            var adsPath = $"{path}:AppOwners";
            System.IO.File.WriteAllText(adsPath, json);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux: 使用 Extended Attributes (xattr) 儲存
            SetAppOwnerLinux(path, json);
        }
        else
        {
            throw new PlatformNotSupportedException("此平台不支援應用程式擁有者功能");
        }
    }

    /// <summary>
    /// 取得應用程式層級的擁有者列表
    /// </summary>
    /// <param name="path">檔案或資料夾路徑</param>
    /// <returns>應用程式擁有者名稱列表，如果未設定則返回空陣列</returns>
    public static string[] GetAppOwners(string path)
    {
        if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
            throw new FileNotFoundException($"路徑不存在: {path}");

        try
        {
            string json = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var adsPath = $"{path}:AppOwners";
                if (System.IO.File.Exists(adsPath))
                {
                    json = System.IO.File.ReadAllText(adsPath).Trim();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                json = GetAppOwnerLinux(path);
            }

            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<string>();

            // 嘗試解析 JSON
            var owners = JsonSerializer.Deserialize<string[]>(json);
            return owners ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// 取得擁有者顯示字串（用逗號分隔）
    /// </summary>
    /// <param name="path">檔案或資料夾路徑</param>
    /// <returns>擁有者顯示字串</returns>
    public static string GetAppOwnersDisplay(string path)
    {
        var owners = GetAppOwners(path);
        return owners.Length > 0 ? string.Join(", ", owners) : "未設定";
    }

    #endregion

    #region 待刪除標記管理（跨平台）

    /// <summary>
    /// Linux: 設定待刪除標記（使用 xattr）
    /// </summary>
    private static void SetMarkedForDeletionLinux(string path, string timestamp)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(timestamp);
            var result = Mono.Unix.Native.Syscall.setxattr(
                path,
                "user.MarkedForDeletion",
                bytes,
                (ulong)bytes.Length,
                0
            );

            if (result != 0)
            {
                var errno = Mono.Unix.Native.Stdlib.GetLastError();
                throw new InvalidOperationException($"Linux 設定待刪除標記失敗: {errno}");
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Linux 設定待刪除標記失敗: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Linux: 取得待刪除標記（使用 xattr）
    /// </summary>
    private static string GetMarkedForDeletionLinux(string path)
    {
        try
        {
            var size = Mono.Unix.Native.Syscall.getxattr(path, "user.MarkedForDeletion", null, 0);
            if (size < 0)
            {
                return null;
            }

            var buffer = new byte[size];
            var result = Mono.Unix.Native.Syscall.getxattr(path, "user.MarkedForDeletion", buffer, (ulong)size);

            if (result < 0)
            {
                return null;
            }

            var value = Encoding.UTF8.GetString(buffer, 0, (int)result);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 標記檔案或資料夾為「待刪除」
    /// Windows: 使用 Alternate Data Stream (ADS)
    /// Linux: 使用 Extended Attributes (xattr)
    /// </summary>
    /// <param name="path">檔案或資料夾路徑</param>
    public static void MarkForDeletion(string path)
    {
        if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
            throw new FileNotFoundException($"路徑不存在: {path}");

        var timestamp = DateTime.UtcNow.ToString("O");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var adsPath = $"{path}:MarkedForDeletion";
            System.IO.File.WriteAllText(adsPath, timestamp);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            SetMarkedForDeletionLinux(path, timestamp);
        }
        else
        {
            throw new PlatformNotSupportedException("此平台不支援待刪除標記功能");
        }
    }

    /// <summary>
    /// 檢查檔案或資料夾是否被標記為「待刪除」
    /// </summary>
    /// <param name="path">檔案或資料夾路徑</param>
    /// <returns>如果被標記為待刪除則返回標記時間（ISO 8601 格式），否則返回 null</returns>
    public static string IsMarkedForDeletion(string path)
    {
        if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
            return null;

        try
        {
            string timestamp = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var adsPath = $"{path}:MarkedForDeletion";
                if (System.IO.File.Exists(adsPath))
                {
                    timestamp = System.IO.File.ReadAllText(adsPath).Trim();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                timestamp = GetMarkedForDeletionLinux(path);
            }

            return string.IsNullOrWhiteSpace(timestamp) ? null : timestamp;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 移除待刪除標記
    /// </summary>
    /// <param name="path">檔案或資料夾路徑</param>
    public static void UnmarkForDeletion(string path)
    {
        if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
            return;

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var adsPath = $"{path}:MarkedForDeletion";
                if (System.IO.File.Exists(adsPath))
                {
                    System.IO.File.Delete(adsPath);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Mono.Unix.Native.Syscall.removexattr(path, "user.MarkedForDeletion");
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 檢查父資料夾是否被標記為「待刪除」（遞迴檢查所有父資料夾）
    /// </summary>
    /// <param name="path">檔案或資料夾路徑</param>
    /// <returns>如果任何父資料夾被標記為待刪除則返回標記時間，否則返回 null</returns>
    public static string IsParentMarkedForDeletion(string path)
    {
        try
        {
            var dir = System.IO.Directory.Exists(path) ? path : Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir))
                return null;

            var currentDir = new DirectoryInfo(dir);
            while (currentDir != null && currentDir.Exists)
            {
                var marked = IsMarkedForDeletion(currentDir.FullName);
                if (!string.IsNullOrEmpty(marked))
                    return marked;

                currentDir = currentDir.Parent;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 處理待刪除項目（背景服務）
    /// 掃描所有被標記為「待刪除」的檔案和資料夾，嘗試執行實際刪除
    /// </summary>
    /// <param name="rootPath">根目錄路徑</param>
    /// <param name="maxItems">每次處理的最大項目數（避免一次處理太多）</param>
    /// <returns>處理結果（成功數、失敗數、錯誤列表）</returns>
    public (int successCount, int failedCount, List<string> errors) ProcessPendingDeletions(string rootPath, int maxItems = 100)
    {
        var successCount = 0;
        var failedCount = 0;
        var errors = new List<string>();

        try
        {
            if (!Directory.Exists(rootPath))
                return (0, 0, errors);

            var markedItems = new List<(string Path, bool IsDirectory)>();
            ScanForMarkedItems(rootPath, markedItems, maxItems);

            foreach (var (itemPath, isDirectory) in markedItems)
            {
                try
                {
                    if (isDirectory)
                    {
                        if (Directory.Exists(itemPath))
                        {
                            Directory.Delete(itemPath, recursive: true);
                            UnmarkForDeletion(itemPath);
                            successCount++;
                        }
                        else
                        {
                            UnmarkForDeletion(itemPath);
                            successCount++;
                        }
                    }
                    else
                    {
                        if (File.Exists(itemPath))
                        {
                            File.Delete(itemPath);
                            UnmarkForDeletion(itemPath);
                            successCount++;
                        }
                        else
                        {
                            UnmarkForDeletion(itemPath);
                            successCount++;
                        }
                    }
                }
                catch (IOException ex)
                {
                    failedCount++;
                    errors.Add($"{itemPath}: {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    failedCount++;
                    errors.Add($"{itemPath}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    failedCount++;
                    errors.Add($"{itemPath}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"掃描待刪除項目時發生錯誤: {ex.Message}");
        }

        return (successCount, failedCount, errors);
    }

    /// <summary>
    /// 遞迴掃描目錄，找出所有被標記為「待刪除」的項目
    /// </summary>
    private void ScanForMarkedItems(string directory, List<(string Path, bool IsDirectory)> markedItems, int maxItems)
    {
        if (markedItems.Count >= maxItems)
            return;

        try
        {
            if (!Directory.Exists(directory))
                return;

            foreach (var dir in Directory.GetDirectories(directory))
            {
                if (markedItems.Count >= maxItems)
                    break;

                var marked = IsMarkedForDeletion(dir);
                if (!string.IsNullOrEmpty(marked))
                {
                    markedItems.Add((dir, true));
                }
                else
                {
                    ScanForMarkedItems(dir, markedItems, maxItems);
                }
            }

            foreach (var file in Directory.GetFiles(directory))
            {
                if (markedItems.Count >= maxItems)
                    break;

                var marked = IsMarkedForDeletion(file);
                if (!string.IsNullOrEmpty(marked))
                {
                    markedItems.Add((file, false));
                }
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 統一的檔案系統查詢方法 - 查詢用戶有權限的檔案/資料夾
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="currentUser">當前用戶</param>
    /// <returns>用戶有權限的檔案系統項目</returns>
    public List<FileSystemEntry> LoadUserFileSystemItems(string virtualPath, string currentUser)
    {
        var items = new List<FileSystemEntry>();
        var absDir = GetSafeAbsolutePath(virtualPath);

        if (!Directory.Exists(absDir)) return items;

        // 載入資料夾（權限控制 + 若無擁有者則回填為當前使用者）
        foreach (var dir in Directory.GetDirectories(absDir))
        {
            var folderName = Path.GetFileName(dir);
            if (IsProtected(virtualPath, folderName)) continue;

            // 過濾掉被標記為待刪除的項目（樂觀刪除：UI 看不到）
            if (!string.IsNullOrEmpty(IsMarkedForDeletion(dir))) continue;

            var owners = GetAppOwners(dir);

            // 統一權限檢查
            if (!HasUserPermission(dir, currentUser)) continue;
            var primaryOwner = owners.Length > 0 ? owners[0] : currentUser;
            var dirInfo = new DirectoryInfo(dir);

            items.Add(new FileSystemEntry
            {
                Name = folderName,
                Type = "folder",
                Size = null,
                LastWriteUtc = dirInfo.LastWriteTimeUtc,
                VirtualPath = virtualPath,
                Owners = owners,
                PrimaryOwner = primaryOwner,
                Owner = primaryOwner,
                SharedUsers = owners.Length > 0 ? owners.ToList() : new List<string> { primaryOwner },
                CreatedAt = dirInfo.CreationTimeUtc,
                UpdatedAt = dirInfo.LastWriteTimeUtc
            });
        }

        // 載入檔案（權限控制 + 若無擁有者則回填為當前使用者）
        foreach (var file in Directory.GetFiles(absDir))
        {
            var fileName = Path.GetFileName(file);
            if (IsProtected(virtualPath, fileName)) continue;

            // 過濾掉被標記為待刪除的項目（樂觀刪除：UI 看不到）
            if (!string.IsNullOrEmpty(IsMarkedForDeletion(file))) continue;

            var owners = GetAppOwners(file);

            // 統一權限檢查
            if (!HasUserPermission(file, currentUser)) continue;
            var primaryOwner = owners.Length > 0 ? owners[0] : currentUser;
            var fileInfo = new FileInfo(file);

            items.Add(new FileSystemEntry
            {
                Name = fileName,
                Type = "file",
                Size = fileInfo.Length,
                MimeType = GetMimeType(fileName),
                LastWriteUtc = fileInfo.LastWriteTimeUtc,
                VirtualPath = virtualPath,
                Owners = owners,
                PrimaryOwner = primaryOwner,
                Owner = primaryOwner,
                SharedUsers = owners.Length > 0 ? owners.ToList() : new List<string> { primaryOwner },
                CreatedAt = fileInfo.CreationTimeUtc,
                UpdatedAt = fileInfo.LastWriteTimeUtc
            });
        }

        return items.OrderBy(f => f.Type).ThenBy(f => f.Name).ToList();
    }

    /// <summary>
    /// 統一的檔案詳細查詢方法
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="name">檔案/資料夾名稱</param>
    /// <param name="currentUser">當前用戶</param>
    /// <returns>檔案詳細資訊</returns>
    public FileSystemEntry? LoadFileSystemDetail(string virtualPath, string name, string currentUser)
    {
        var absDir = GetSafeAbsolutePath(virtualPath);
        var safeName = SanitizeName(name);
        var fullPath = Path.Combine(absDir, safeName);

        if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            return null;

        // 權限檢查
        if (!HasUserPermission(fullPath, currentUser))
            return null;

        var owners = GetAppOwners(fullPath);
        var primaryOwner = owners.Length > 0 ? owners[0] : currentUser;
        var isDirectory = Directory.Exists(fullPath);
        var fileInfo = isDirectory ? null : new FileInfo(fullPath);
        var dirInfo = isDirectory ? new DirectoryInfo(fullPath) : null;
        var fileSize = isDirectory ? null : fileInfo?.Length;
        var formattedSize = FormatFileSize(fileSize);
        var fullVirtualPath = virtualPath == "/" ? $"/{name}" : $"{virtualPath}/{name}";

        return new FileSystemEntry
        {
            Name = name,
            Type = isDirectory ? "folder" : "file",
            Size = fileSize,
            MimeType = isDirectory ? null : GetMimeType(name),
            FullPath = fullVirtualPath,
            FormattedSize = formattedSize,
            LastWriteUtc = isDirectory ? dirInfo?.LastWriteTimeUtc : fileInfo?.LastWriteTimeUtc,
            CreatedAt = isDirectory ? dirInfo?.CreationTimeUtc : fileInfo?.CreationTimeUtc,
            UpdatedAt = isDirectory ? dirInfo?.LastWriteTimeUtc : fileInfo?.LastWriteTimeUtc,
            VirtualPath = virtualPath,
            Owners = owners,
            PrimaryOwner = primaryOwner,
            SharedUsers = owners.Length > 0 ? owners.ToList() : new List<string> { primaryOwner },
            HasPermission = true
        };
    }

    /// <summary>
    /// 格式化檔案大小
    /// </summary>
    private static string? FormatFileSize(long? bytes)
    {
        if (bytes == null) return null;

        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes.Value;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// 統一的權限檢查方法
    /// </summary>
    /// <param name="path">檔案或資料夾路徑</param>
    /// <param name="currentUser">當前用戶</param>
    /// <returns>是否有權限</returns>
    private bool HasUserPermission(string path, string currentUser)
    {
        if (string.IsNullOrEmpty(currentUser)) return false;

        var owners = GetAppOwners(path);

        // 沒有擁有者設定 = 公開存取
        if (owners.Length == 0) return true;

        // 檢查是否為擁有者
        return owners.Any(o => o.Equals(currentUser, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 更新資料夾擁有者
    /// </summary>
    /// <param name="virtualPath">虛擬路徑</param>
    /// <param name="folderName">資料夾名稱</param>
    /// <param name="newOwners">新的擁有者列表</param>
    public void UpdateFolderOwners(FileSystemEntry entry, string[] newOwners)
    {
        var absDir = GetSafeAbsolutePath(entry.VirtualPath);
        var folderPath = Path.Combine(absDir, SanitizeName(entry.Name));

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException("資料夾不存在");

        if (newOwners == null || newOwners.Length == 0)
            throw new ArgumentException("擁有者列表不可為空");

        SetAppOwners(folderPath, newOwners);
    }

    /// <summary>
    /// 更新檔案或資料夾擁有者（依據虛擬路徑與名稱判斷類型）
    /// </summary>
    public void UpdateItemOwners(FileSystemEntry entry, string[] newOwners)
    {
        if (newOwners == null || newOwners.Length == 0)
            throw new ArgumentException("擁有者列表不可為空");

        var absDir = GetSafeAbsolutePath(entry.VirtualPath);
        var safeName = SanitizeName(entry.Name);
        var fullPath = Path.Combine(absDir, safeName);

        if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            throw new FileNotFoundException("檔案或資料夾不存在");

        if (Directory.Exists(fullPath))
        {
            // 將資料夾的共享設定遞迴套用至其所有子資料夾與檔案，保留各子項目的原始 PrimaryOwner
            SetOwnersRecursivePreservePrimary(fullPath, newOwners);
        }
        else
        {
            SetAppOwners(fullPath, newOwners);
        }
    }

    /// <summary>
    /// 遞迴設定資料夾及其所有子項目的應用程式擁有者
    /// </summary>
    private void SetOwnersRecursivePreservePrimary(string directoryPath, string[] folderOwners)
    {
        var folderPrimary = (folderOwners != null && folderOwners.Length > 0) ? folderOwners[0] : null;

        // 目錄本身：保留其原有 PrimaryOwner（如不存在則使用資料夾的 Primary）
        var dirOwners = GetAppOwners(directoryPath);
        var dirPrimary = dirOwners.Length > 0 ? dirOwners[0] : folderPrimary;
        var newDirOwners = new List<string>();
        if (!string.IsNullOrWhiteSpace(dirPrimary)) newDirOwners.Add(dirPrimary);
        if (folderOwners != null)
        {
            foreach (var u in folderOwners)
            {
                if (!string.Equals(u, dirPrimary, StringComparison.OrdinalIgnoreCase) && !newDirOwners.Contains(u, StringComparer.OrdinalIgnoreCase))
                    newDirOwners.Add(u);
            }
        }
        SetAppOwners(directoryPath, newDirOwners.ToArray());

        // 子資料夾
        foreach (var dir in Directory.EnumerateDirectories(directoryPath))
        {
            SetOwnersRecursivePreservePrimary(dir, folderOwners);
        }

        // 檔案：同樣保留原始 PrimaryOwner
        foreach (var file in Directory.EnumerateFiles(directoryPath))
        {
            var fileOwners = GetAppOwners(file);
            var filePrimary = fileOwners.Length > 0 ? fileOwners[0] : folderPrimary;
            var newFileOwners = new List<string>();
            if (!string.IsNullOrWhiteSpace(filePrimary)) newFileOwners.Add(filePrimary);
            if (folderOwners != null)
            {
                foreach (var u in folderOwners)
                {
                    if (!string.Equals(u, filePrimary, StringComparison.OrdinalIgnoreCase) && !newFileOwners.Contains(u, StringComparer.OrdinalIgnoreCase))
                        newFileOwners.Add(u);
                }
            }
            SetAppOwners(file, newFileOwners.ToArray());
        }
    }

    #endregion
}

