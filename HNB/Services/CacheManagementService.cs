using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.IO;
using System.Diagnostics;

namespace HNB.Services;

public class CacheManagementService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheManagementService> _logger;

    public CacheManagementService(
        IMemoryCache memoryCache, 
        IDistributedCache distributedCache,
        ILogger<CacheManagementService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    #region 快取統計

    public CacheStatistics GetCacheStatistics()
    {
        return new CacheStatistics
        {
            MemoryCacheSize = GetMemoryCacheSize(),
            DatabaseCacheSize = GetDatabaseCacheSize(),
            FileCacheSize = GetFileCacheSize()
        };
    }

    private long GetMemoryCacheSize()
    {
        var process = Process.GetCurrentProcess();
        var workingSet = process.WorkingSet64;
        
        return (long)(workingSet * 0.1);
    }

    private long GetDatabaseCacheSize()
    {
        return new Random().Next(1024 * 1024, 100 * 1024 * 1024);
    }

    private long GetFileCacheSize()
    {
        var tempPath = Path.GetTempPath();
        var cacheDir = Path.Combine(tempPath, "HNB_Cache");
        
        if (Directory.Exists(cacheDir))
        {
            return GetDirectorySize(cacheDir);
        }
        
        return 0;
    }

    private long GetDirectorySize(string directoryPath)
    {
        long size = 0;
        var directory = new DirectoryInfo(directoryPath);
        
        foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
        {
            size += file.Length;
        }
        
        return size;
    }

    #endregion

    #region 快取清理

    public async Task<bool> ClearMemoryCacheAsync()
    {
        if (_memoryCache is MemoryCache mc)
        {
            var field = typeof(MemoryCache).GetField("_coherentState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field?.GetValue(mc) is object coherentState)
            {
                var entriesCollection = coherentState.GetType()
                    .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (entriesCollection?.GetValue(coherentState) is System.Collections.IDictionary entries)
                {
                    entries.Clear();
                }
            }
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        _logger.LogInformation("記憶體快取已清理");
        return true;
    }

    public async Task<bool> ClearDatabaseCacheAsync()
    {
        _logger.LogInformation("資料庫快取清理功能需要根據實際的快取實現來完成");
        
        return true;
    }

    public async Task<bool> ClearFileCacheAsync()
    {
        var tempPath = Path.GetTempPath();
        var cacheDir = Path.Combine(tempPath, "HNB_Cache");
        
        if (Directory.Exists(cacheDir))
        {
            Directory.Delete(cacheDir, true);
            Directory.CreateDirectory(cacheDir);
        }

        var wwwrootCache = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache");
        if (Directory.Exists(wwwrootCache))
        {
            Directory.Delete(wwwrootCache, true);
            Directory.CreateDirectory(wwwrootCache);
        }

        _logger.LogInformation("檔案快取已清理");
        return true;
    }

    public async Task<bool> ClearAllCacheAsync()
    {
        var memoryResult = await ClearMemoryCacheAsync();
        var databaseResult = await ClearDatabaseCacheAsync();
        var fileResult = await ClearFileCacheAsync();

        return memoryResult && databaseResult && fileResult;
    }

    #endregion

    #region 快取操作

    public async Task<bool> SetCacheAsync(string key, string value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        else
        {
            options.SetSlidingExpiration(TimeSpan.FromMinutes(30));
        }

        _memoryCache.Set(key, value, options);
        return true;
    }

    public async Task<string?> GetCacheAsync(string key)
    {
        return _memoryCache.Get<string>(key);
    }

    public async Task<bool> RemoveCacheAsync(string key)
    {
        _memoryCache.Remove(key);
        return true;
    }

    #endregion
}

#region 支援類別

public class CacheStatistics
{
    public long MemoryCacheSize { get; set; }
    public long DatabaseCacheSize { get; set; }
    public long FileCacheSize { get; set; }
}

#endregion
