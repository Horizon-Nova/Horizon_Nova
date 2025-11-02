using System.Diagnostics;
using Web.Models;

namespace Web.Services;

public class SiteManagementService
{
    private readonly string _sitesPath;

    public SiteManagementService()
    {
        _sitesPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "Sites");
    }

    public List<SiteInfo> LoadSiteList()
    {
        var sites = new List<SiteInfo>();
        
        if (!Directory.Exists(_sitesPath))
            return sites;

        foreach (var dir in Directory.GetDirectories(_sitesPath))
        {
            var dirInfo = new DirectoryInfo(dir);
            var webConfigPath = Path.Combine(dir, "web.config");
            
            sites.Add(new SiteInfo
            {
                Name = dirInfo.Name,
                Path = dir,
                AppPool = dirInfo.Name,
                Port = GetPortFromWebConfig(webConfigPath),
                Status = GetSiteStatus(dirInfo.Name),
                LastPublish = dirInfo.LastWriteTime,
                FileSize = GetDirectorySize(dir)
            });
        }

        return sites;
    }

    public string GetSiteStatus(string siteName)
    {
        try
        {
            var processes = Process.GetProcessesByName(siteName);
            return processes.Length > 0 ? "運行中" : "已停止";
        }
        catch
        {
            return "未知";
        }
    }

    public bool StopSite(string siteName)
    {
        try
        {
            var processes = Process.GetProcessesByName(siteName);
            foreach (var process in processes)
            {
                process.Kill();
                process.WaitForExit();
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DeleteSite(string siteName)
    {
        var sitePath = Path.Combine(_sitesPath, siteName);
        if (!Directory.Exists(sitePath))
            return false;

        StopSite(siteName);
        Directory.Delete(sitePath, true);
        return true;
    }

    private int GetPortFromWebConfig(string path)
    {
        return 5000;
    }

    private long GetDirectorySize(string path)
    {
        var dirInfo = new DirectoryInfo(path);
        return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
    }
}

