using System.Text;
using System.Text.Json;
using HNB.IntelligentSystems.Qdrant.Configuration;
using HNB.IntelligentSystems.Qdrant.Models;

namespace HNB.IntelligentSystems.Qdrant.Core;

/// <summary>
/// Qdrant 核心引擎
/// 負責與 Qdrant 向量資料庫進行互動
/// </summary>
public class QdrantEngine(QdrantConfig config, HttpClient httpClient)
{
    private readonly QdrantConfig _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    private string GetBaseUrl()
    {
        var protocol = _config.UseHttps ? "https" : "http";
        return $"{protocol}://{_config.Url.TrimEnd('/')}:{_config.Port}";
    }

    private void EnsureHeadersInitialized()
    {
        if (!string.IsNullOrEmpty(_config.ApiKey) && !_httpClient.DefaultRequestHeaders.Contains("api-key"))
        {
            _httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
        }
    }

    /// <summary>
    /// 檢查 Collection 是否存在
    /// </summary>
    public async Task<bool> CollectionExists(string collectionName, CancellationToken cancellationToken = default)
    {
        EnsureHeadersInitialized();
        var url = $"{GetBaseUrl()}/collections/{collectionName}";
        
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 建立 Collection
    /// </summary>
    public async Task<bool> CreateCollection(string collectionName, int vectorSize, string distanceMetric = "Cosine", CancellationToken cancellationToken = default)
    {
        EnsureHeadersInitialized();
        var url = $"{GetBaseUrl()}/collections/{collectionName}";
        
        var requestBody = new
        {
            vectors = new
            {
                size = vectorSize,
                distance = distanceMetric
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync(url, content, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// 列出所有 Collection
    /// </summary>
    public async Task<List<string>> ListCollections(CancellationToken cancellationToken = default)
    {
        EnsureHeadersInitialized();
        var url = $"{GetBaseUrl()}/collections";
        
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDoc = JsonDocument.Parse(content);
            
            if (jsonDoc.RootElement.TryGetProperty("result", out var result) && 
                result.TryGetProperty("collections", out var collections))
            {
                return collections.EnumerateArray()
                    .Select(c => c.TryGetProperty("name", out var name) ? name.GetString() : null)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Cast<string>()
                    .ToList();
            }
        }
        catch
        {
            // 忽略錯誤
        }
        
        return new List<string>();
    }

    /// <summary>
    /// 刪除 Collection
    /// </summary>
    public async Task<bool> DeleteCollection(string collectionName, CancellationToken cancellationToken = default)
    {
        EnsureHeadersInitialized();
        var url = $"{GetBaseUrl()}/collections/{collectionName}";
        
        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// 插入或更新向量點
    /// </summary>
    public async Task<bool> UpsertVectors(string collectionName, List<VectorPoint> points, CancellationToken cancellationToken = default)
    {
        EnsureHeadersInitialized();
        var url = $"{GetBaseUrl()}/collections/{collectionName}/points";
        
        var requestBody = new
        {
            points = points.Select(p => new
            {
                id = p.Id,
                vector = p.Vector,
                payload = p.Payload ?? new Dictionary<string, object>()
            }).ToList()
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync(url, content, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// 搜尋相似向量
    /// </summary>
    public async Task<List<SearchResult>> Search(string collectionName, List<float> queryVector, int limit = 10, float? scoreThreshold = null, Dictionary<string, object>? filter = null, CancellationToken cancellationToken = default)
    {
        EnsureHeadersInitialized();
        var url = $"{GetBaseUrl()}/collections/{collectionName}/points/search";
        
        var requestBody = new Dictionary<string, object>
        {
            ["vector"] = queryVector,
            ["limit"] = limit
        };

        if (scoreThreshold.HasValue)
        {
            requestBody["score_threshold"] = scoreThreshold.Value;
        }

        if (filter != null && filter.Count > 0)
        {
            requestBody["filter"] = filter;
        }

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"搜尋失敗: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonDoc = JsonDocument.Parse(responseContent);
        
        var results = new List<SearchResult>();
        
        if (jsonDoc.RootElement.TryGetProperty("result", out var result))
        {
            foreach (var item in result.EnumerateArray())
            {
                var searchResult = new SearchResult();
                
                if (item.TryGetProperty("id", out var id))
                {
                    searchResult.Id = id.ValueKind == JsonValueKind.String 
                        ? id.GetString() ?? string.Empty 
                        : id.GetRawText();
                }
                
                if (item.TryGetProperty("score", out var score))
                {
                    searchResult.Score = (float)score.GetDouble();
                }
                
                if (item.TryGetProperty("payload", out var payload))
                {
                    searchResult.Payload = JsonSerializer.Deserialize<Dictionary<string, object>>(payload.GetRawText());
                }
                
                results.Add(searchResult);
            }
        }
        
        return results;
    }

    /// <summary>
    /// 刪除所有向量點
    /// </summary>
    public async Task<bool> DeleteAllPoints(string collectionName, CancellationToken cancellationToken = default)
    {
        EnsureHeadersInitialized();
        var url = $"{GetBaseUrl()}/collections/{collectionName}/points/delete";
        
        var requestBody = new
        {
            filter = new
            {
                must = new object[] { }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// 取得配置資訊
    /// </summary>
    public QdrantConfig GetConfig() => _config;
}

