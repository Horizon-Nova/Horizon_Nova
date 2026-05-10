using HNB.IntelligentSystems.Qdrant.Configuration;
using HNB.IntelligentSystems.Qdrant.Core;
using HNB.IntelligentSystems.Qdrant.Models;
using Microsoft.Extensions.Configuration;

namespace HNB.IntelligentSystems.Qdrant.Module;

/// <summary>
/// Qdrant 模組
/// 封裝 QdrantEngine 的邏輯，提供高階 API
/// </summary>
public class QdrantModule(IConfiguration configuration, IHttpClientFactory httpClientFactory) : IDisposable
{
    private readonly QdrantConfig _config = LoadConfig(configuration);
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    
    private static QdrantConfig LoadConfig(IConfiguration configuration)
    {
        var config = new QdrantConfig();
        configuration.GetSection("Qdrant").Bind(config);
        return config;
    }
    private QdrantEngine? _engine;

    #region Collection 管理

    /// <summary>
    /// 檢查 Collection 是否存在
    /// </summary>
    public async Task<bool> CollectionExists(string? collectionName = null, CancellationToken cancellationToken = default)
    {
        var name = collectionName ?? _config.DefaultCollectionName;
        var engine = GetOrCreateEngine();
        return await engine.CollectionExists(name, cancellationToken);
    }

    /// <summary>
    /// 建立 Collection
    /// </summary>
    public async Task<bool> CreateCollection(string? collectionName = null, int? vectorSize = null, string? distanceMetric = null, CancellationToken cancellationToken = default)
    {
        var name = collectionName ?? _config.DefaultCollectionName;
        var size = vectorSize ?? _config.DefaultVectorSize;
        var metric = distanceMetric ?? _config.DistanceMetric;
        
        var engine = GetOrCreateEngine();
        return await engine.CreateCollection(name, size, metric, cancellationToken);
    }

    /// <summary>
    /// 列出所有 Collection
    /// </summary>
    public async Task<List<string>> ListCollections(CancellationToken cancellationToken = default)
    {
        var engine = GetOrCreateEngine();
        return await engine.ListCollections(cancellationToken);
    }

    /// <summary>
    /// 刪除 Collection
    /// </summary>
    public async Task<bool> DeleteCollection(string collectionName, CancellationToken cancellationToken = default)
    {
        var engine = GetOrCreateEngine();
        return await engine.DeleteCollection(collectionName, cancellationToken);
    }

    #endregion

    #region 向量操作

    /// <summary>
    /// 插入或更新向量點
    /// </summary>
    public async Task<bool> UpsertVectors(string? collectionName, List<VectorPoint> points, CancellationToken cancellationToken = default)
    {
        if (points == null || points.Count == 0)
            throw new ArgumentException("向量點列表不能為空");

        var name = collectionName ?? _config.DefaultCollectionName;
        var engine = GetOrCreateEngine();
        
        // 確保 Collection 存在
        var exists = await engine.CollectionExists(name, cancellationToken);
        if (!exists)
        {
            // 使用第一個點的向量維度來建立 Collection
            var vectorSize = points.First().Vector?.Count ?? _config.DefaultVectorSize;
            await engine.CreateCollection(name, vectorSize, _config.DistanceMetric, cancellationToken);
        }
        
        return await engine.UpsertVectors(name, points, cancellationToken);
    }

    /// <summary>
    /// 搜尋相似向量
    /// </summary>
    public async Task<List<SearchResult>> Search(
        string? collectionName,
        List<float> queryVector,
        int? limit = null,
        float? scoreThreshold = null,
        Dictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (queryVector == null || queryVector.Count == 0)
            throw new ArgumentException("查詢向量不能為空");

        var name = collectionName ?? _config.DefaultCollectionName;
        var searchLimit = limit ?? _config.DefaultLimit;
        
        var engine = GetOrCreateEngine();
        return await engine.Search(name, queryVector, searchLimit, scoreThreshold, filter, cancellationToken);
    }

    /// <summary>
    /// 以 filter 捲動取回符合條件的所有點（不需向量查詢）
    /// </summary>
    public async Task<List<SearchResult>> Scroll(
        string? collectionName,
        Dictionary<string, object>? filter = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var name = collectionName ?? _config.DefaultCollectionName;
        var engine = GetOrCreateEngine();
        return await engine.Scroll(name, filter, limit, false, cancellationToken);
    }

    /// <summary>
    /// 刪除所有向量點
    /// </summary>
    public async Task<bool> DeleteAllPoints(string? collectionName = null, CancellationToken cancellationToken = default)
    {
        var name = collectionName ?? _config.DefaultCollectionName;
        var engine = GetOrCreateEngine();
        return await engine.DeleteAllPoints(name, cancellationToken);
    }

    #endregion

    #region 配置方法

    /// <summary>
    /// 載入配置資訊
    /// </summary>
    public QdrantConfig LoadConfig() => _config;

    #endregion

    #region 私有方法

    private QdrantEngine GetOrCreateEngine()
    {
        if (_engine == null)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
            _engine = new QdrantEngine(_config, httpClient);
        }
        return _engine;
    }

    #endregion

    #region 資源釋放

    public void Dispose()
    {
        // HttpClient 由 IHttpClientFactory 管理，不需要在這裡釋放
        _engine = null;
    }

    #endregion
}

