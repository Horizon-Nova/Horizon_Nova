using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace HNB.Areas.Backoffice.Utilities;

public class CacheManagementUtilities(IMemoryCache memoryCache)
{
    public CacheStatistics LoadCacheStatistics()
    {
        var memoryCacheSize = GetMemoryCacheSize();
        var databaseCacheSize = 0;
        var fileCacheSize = GetFileCacheSize();

        return new CacheStatistics
        {
            MemoryCacheSize = memoryCacheSize,
            DatabaseCacheSize = databaseCacheSize,
            FileCacheSize = fileCacheSize
        };
    }

    public bool ClearMemoryCache(bool is30Days = false)
    {
        if (is30Days)
        {
            return true;
        }
        
        if (memoryCache is MemoryCache memoryCacheInstance)
        {
            var field = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field?.GetValue(memoryCacheInstance) is object coherentState)
            {
                var entriesCollection = coherentState.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                if (entriesCollection?.GetValue(coherentState) is System.Collections.IDictionary entries)
                {
                    entries.Clear();
                }
            }
        }
        return true;
    }

    public bool ClearDatabaseCache(bool is30Days = false)
    {
        return true;
    }

    public bool ClearFileCache(bool is30Days = false)
    {
        var cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache");
        if (Directory.Exists(cacheDir))
        {
            var files = Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (is30Days)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < DateTime.Now.AddDays(-30))
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    File.Delete(file);
                }
            }
        }
        return true;
    }

    public bool ClearAllCache()
    {
        ClearMemoryCache();
        ClearDatabaseCache();
        ClearFileCache();
        return true;
    }

    private long GetMemoryCacheSize()
    {
        if (memoryCache is MemoryCache memoryCacheInstance)
        {
            var field = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field?.GetValue(memoryCacheInstance) is object coherentState)
            {
                var entriesCollection = coherentState.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                if (entriesCollection?.GetValue(coherentState) is System.Collections.IDictionary entries)
                {
                    return entries.Count;
                }
            }
        }
        return 0;
    }

    private long GetFileCacheSize()
    {
        var cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache");
        if (Directory.Exists(cacheDir))
        {
            var files = Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories);
            return files.Sum(file => new FileInfo(file).Length);
        }
        return 0;
    }
}

public class CacheStatistics
{
    public long MemoryCacheSize { get; set; }
    public long DatabaseCacheSize { get; set; }
    public long FileCacheSize { get; set; }
}
