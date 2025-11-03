using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using HNB.IIS.Core.Dtos;

namespace HNB.IIS.Core.Services;

/// <summary>
/// 站台管理服務 - 提供站台部署、啟動、停止、刪除等功能
/// </summary>
public class SiteManagementService
{
    private readonly string _sitesPath;

    /// <summary>
    /// 初始化站台管理服務，自動定位 Sites 目錄
    /// </summary>
    public SiteManagementService()
    {
        var baseDir = Path.GetFullPath(AppContext.BaseDirectory);
        var dir = new DirectoryInfo(baseDir);
        
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Core.csproj")))
        {
            dir = dir.Parent;
        }
        
        _sitesPath = dir?.Parent != null ? Path.Combine(dir.Parent.FullName, "Sites") : "";
    }

    /// <summary>
    /// 載入所有站台清單
    /// </summary>
    /// <returns>站台資訊清單</returns>
    public List<SiteInfo> LoadSiteList()
    {
        var sites = new List<SiteInfo>();
        
        if (!Directory.Exists(_sitesPath))
            return sites;

        foreach (var dir in Directory.GetDirectories(_sitesPath))
        {
            var dirInfo = new DirectoryInfo(dir);
            var actualPath = ResolveActualSitePath(dir);
            
            EnsureKestrelConfiguration(actualPath, dirInfo.Name);
            
            sites.Add(new SiteInfo
            {
                Name = dirInfo.Name,
                Path = dir,
                AppPool = dirInfo.Name,
                Port = ParsePortFromAppSettings(actualPath),
                Status = ParseSiteStatus(dirInfo.Name),
                LastPublish = dirInfo.LastWriteTime,
                FileSize = CalculateDirectorySize(dir)
            });
        }

        return sites;
    }

    /// <summary>
    /// 取得站台運行狀態
    /// </summary>
    /// <param name="siteName">站台名稱</param>
    /// <returns>運行中 或 已停止</returns>
    public string ParseSiteStatus(string siteName)
        {
            var processes = Process.GetProcessesByName(siteName);
            return processes.Length > 0 ? "運行中" : "已停止";
        }

    /// <summary>
    /// 啟動站台
    /// </summary>
    /// <param name="siteName">站台名稱</param>
    /// <returns>是否成功啟動</returns>
    public bool StartSite(string siteName)
    {
        var sitePath = Path.Combine(_sitesPath, siteName);
        if (!Directory.Exists(sitePath))
            throw new DirectoryNotFoundException($"站台目錄不存在: {sitePath}");

        var actualSitePath = ResolveActualSitePath(sitePath);

        var exePath = Path.Combine(actualSitePath, $"{siteName}.exe");
        if (File.Exists(exePath))
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = actualSitePath,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            var process = Process.Start(processInfo);
            if (process == null)
                throw new InvalidOperationException($"無法啟動進程: {exePath}");
            
            return true;
        }

        var csprojPath = Path.Combine(actualSitePath, $"{siteName}.csproj");
        if (File.Exists(csprojPath))
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{csprojPath}\"",
                WorkingDirectory = actualSitePath,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            var process = Process.Start(processInfo);
            if (process == null)
                throw new InvalidOperationException($"無法啟動 dotnet 進程");
            
            return true;
        }

        throw new FileNotFoundException($"找不到 {siteName}.exe 或 {siteName}.csproj");
    }

    /// <summary>
    /// 取得實際站台路徑（支援舊的 publish 子目錄格式）
    /// </summary>
    /// <param name="sitePath">站台路徑</param>
    /// <returns>實際站台路徑</returns>
    private string ResolveActualSitePath(string sitePath)
    {
        var publishPath = Path.Combine(sitePath, "publish");
        return Directory.Exists(publishPath) ? publishPath : sitePath;
    }

    /// <summary>
    /// 停止站台
    /// </summary>
    /// <param name="siteName">站台名稱</param>
    /// <returns>是否成功停止</returns>
    public bool StopSite(string siteName)
        {
            var processes = Process.GetProcessesByName(siteName);
            foreach (var process in processes)
            {
                process.Kill();
                process.WaitForExit();
            }
            return true;
    }

    /// <summary>
    /// 刪除站台
    /// </summary>
    /// <param name="siteName">站台名稱</param>
    /// <returns>是否成功刪除</returns>
    public bool DeleteSite(string siteName)
    {
        var sitePath = Path.Combine(_sitesPath, siteName);
        if (!Directory.Exists(sitePath))
            return false;

        StopSite(siteName);
        Directory.Delete(sitePath, true);
        return true;
    }

    /// <summary>
    /// 建立新站台
    /// </summary>
    /// <param name="siteName">站台名稱</param>
    /// <param name="sitePath">專案路徑</param>
    /// <returns>是否成功建立</returns>
    public bool CreateSite(string siteName, string sitePath)
    {
        var targetPath = Path.Combine(_sitesPath, siteName);
        
        if (Directory.Exists(targetPath))
            return false;
        
        var sourceProjectPath = sitePath;
        
        if (!Directory.Exists(sourceProjectPath))
            return false;
        
        var csprojFiles = Directory.GetFiles(sourceProjectPath, "*.csproj");
        if (csprojFiles.Length == 0)
            return false;
        
        var csprojFile = csprojFiles[0];
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"publish \"{csprojFile}\" -c Release -o \"{targetPath}\"",
            WorkingDirectory = sourceProjectPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        
        var process = Process.Start(processInfo);
        if (process == null)
            return false;
        
        process.WaitForExit();
        
        if (process.ExitCode != 0)
            return false;
        
        var sourceLaunchSettings = Path.Combine(sourceProjectPath, "Properties", "launchSettings.json");
        if (File.Exists(sourceLaunchSettings))
        {
            var destPropertiesDir = Path.Combine(targetPath, "Properties");
            Directory.CreateDirectory(destPropertiesDir);
            File.Copy(sourceLaunchSettings, Path.Combine(destPropertiesDir, "launchSettings.json"), true);
        }
        
        var sourceAppSettings = Path.Combine(sourceProjectPath, "appsettings.json");
        if (File.Exists(sourceAppSettings))
        {
            File.Copy(sourceAppSettings, Path.Combine(targetPath, "appsettings.json"), true);
        }
        
        return true;
    }

    /// <summary>
    /// 從 ZIP 檔案建立新站台
    /// </summary>
    /// <param name="siteName">站台名稱</param>
    /// <param name="zipFile">上傳的 ZIP 檔案</param>
    /// <returns>是否成功建立</returns>
    public bool CreateSiteFromZip(string siteName, IFormFile zipFile)
    {
        var targetPath = Path.Combine(_sitesPath, siteName);
        
        if (Directory.Exists(targetPath))
            return false;

        var tempZipPath = Path.Combine(Path.GetTempPath(), $"{siteName}_{Guid.NewGuid()}.zip");
        
        using (var fileStream = new FileStream(tempZipPath, FileMode.Create))
        {
            zipFile.CopyTo(fileStream);
        }

        ZipFile.ExtractToDirectory(tempZipPath, targetPath);
        
        File.Delete(tempZipPath);
        
        ConfigurePortAfterExtract(targetPath);
        
        return true;
    }

    /// <summary>
    /// 在解壓縮後自動配置 port
    /// </summary>
    /// <param name="sitePath">站台路徑</param>
    private void ConfigurePortAfterExtract(string sitePath)
    {
        var dirInfo = new DirectoryInfo(sitePath);
        EnsureKestrelConfiguration(sitePath, dirInfo.Name);
    }

    private void EnsureKestrelConfiguration(string sitePath, string siteName)
    {
        var appSettingsPath = Path.Combine(sitePath, "appsettings.json");
        if (!File.Exists(appSettingsPath))
            return;
        
        var json = File.ReadAllText(appSettingsPath);
        var doc = JsonDocument.Parse(json);
        
        if (doc.RootElement.TryGetProperty("Kestrel", out _))
            return;
        
        var port = ParsePortFromAppSettings(sitePath);
        
        if (port == 0)
            port = ResolvePortBySiteName(siteName);
        
        if (port == 0)
            return;
        
        var tempPath = appSettingsPath + ".tmp";
        using (var stream = File.Create(tempPath))
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                writer.WritePropertyName(property.Name);
                property.Value.WriteTo(writer);
            }
            
            writer.WritePropertyName("Kestrel");
            writer.WriteStartObject();
            writer.WritePropertyName("Endpoints");
            writer.WriteStartObject();
            writer.WritePropertyName("Http");
            writer.WriteStartObject();
            writer.WriteString("Url", $"http://localhost:{port}");
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
            
            writer.WriteEndObject();
            writer.Flush();
        }
        
        File.Move(tempPath, appSettingsPath, true);
    }

    private int ResolvePortBySiteName(string siteName)
    {
        var portMap = new Dictionary<string, int>
        {
            { "TestWeb", 5149 },
            { "Admin", 5265 },
            { "HNB", 5255 }
        };
        
        return portMap.TryGetValue(siteName, out var port) ? port : 0;
    }

    /// <summary>
    /// 遞迴複製目錄
    /// </summary>
    /// <param name="sourceDir">來源目錄</param>
    /// <param name="destDir">目標目錄</param>
    private void CopyDirectory(string sourceDir, string destDir)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
            return;

        Directory.CreateDirectory(destDir);

        foreach (var file in dir.GetFiles())
        {
            var destFilePath = Path.Combine(destDir, file.Name);
            file.CopyTo(destFilePath, true);
        }

        foreach (var subDir in dir.GetDirectories())
        {
            var destSubDir = Path.Combine(destDir, subDir.Name);
            CopyDirectory(subDir.FullName, destSubDir);
        }
    }

    private int ParsePortFromAppSettings(string sitePath)
    {
        var launchSettingsPath = Path.Combine(sitePath, "Properties", "launchSettings.json");
        if (File.Exists(launchSettingsPath))
        {
            var json = File.ReadAllText(launchSettingsPath);
            var doc = JsonDocument.Parse(json);
            
            if (doc.RootElement.TryGetProperty("profiles", out var profiles))
            {
                if (profiles.TryGetProperty("http", out var httpProfile))
                {
                    if (httpProfile.TryGetProperty("applicationUrl", out var appUrl))
                    {
                        var url = appUrl.GetString();
                        if (!string.IsNullOrEmpty(url) && url.Contains(':'))
                        {
                            var port = url.Split(':').Last();
                            if (int.TryParse(port, out var portNum))
                                return portNum;
                        }
                    }
                }
                
                if (profiles.TryGetProperty("https", out var httpsProfile))
                {
                    if (httpsProfile.TryGetProperty("applicationUrl", out var appUrl))
                    {
                        var url = appUrl.GetString();
                        if (!string.IsNullOrEmpty(url) && url.Contains(':'))
                        {
                            var firstUrl = url.Split(';').First();
                            if (firstUrl.Contains(':'))
                            {
                                var port = firstUrl.Split(':').Last();
                                if (int.TryParse(port, out var portNum))
                                    return portNum;
                            }
                        }
                    }
                }
            }
        }
        
        var appSettingsPath = Path.Combine(sitePath, "appsettings.json");
        if (File.Exists(appSettingsPath))
        {
            var json = File.ReadAllText(appSettingsPath);
            var doc = JsonDocument.Parse(json);
            
            if (doc.RootElement.TryGetProperty("Kestrel", out var kestrel))
            {
                if (kestrel.TryGetProperty("Endpoints", out var endpoints))
                {
                    if (endpoints.TryGetProperty("Https", out var https))
                    {
                        if (https.TryGetProperty("Url", out var httpsUrl))
                        {
                            var url = httpsUrl.GetString();
                            if (!string.IsNullOrEmpty(url) && url.Contains(':'))
                            {
                                var port = url.Split(':').Last();
                                if (int.TryParse(port, out var portNum))
                                    return portNum;
                            }
                        }
                    }
                    
                    if (endpoints.TryGetProperty("Http", out var http))
                    {
                        if (http.TryGetProperty("Url", out var httpUrl))
                        {
                            var url = httpUrl.GetString();
                            if (!string.IsNullOrEmpty(url) && url.Contains(':'))
                            {
                                var port = url.Split(':').Last();
                                if (int.TryParse(port, out var portNum))
                                    return portNum;
                            }
                        }
                    }
                }
            }
        }
        
        return 0;
    }

    private long CalculateDirectorySize(string path)
    {
        var dirInfo = new DirectoryInfo(path);
        var excludeDirs = new[] { "bin", "obj", "node_modules", ".git" };
        return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(f => !excludeDirs.Any(exclude => f.FullName.Contains(Path.DirectorySeparatorChar + exclude + Path.DirectorySeparatorChar)))
            .Sum(file => file.Length);
    }
}

