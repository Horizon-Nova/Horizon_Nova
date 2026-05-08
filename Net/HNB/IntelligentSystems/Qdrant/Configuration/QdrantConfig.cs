namespace HNB.IntelligentSystems.Qdrant.Configuration;

/// <summary>
/// Qdrant 配置類別
/// </summary>
public class QdrantConfig
{
    /// <summary>
    /// Qdrant 服務 URL
    /// </summary>
    public string Url { get; set; } = "http://localhost:6333";
    
    /// <summary>
    /// Qdrant 服務 Port
    /// </summary>
    public int Port { get; set; } = 6333;
    
    /// <summary>
    /// 是否使用 HTTPS
    /// </summary>
    public bool UseHttps { get; set; } = false;
    
    /// <summary>
    /// API Key（可選）
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// 預設 Collection 名稱
    /// </summary>
    public string DefaultCollectionName { get; set; } = "default";
    
    /// <summary>
    /// 預設向量維度
    /// </summary>
    public int DefaultVectorSize { get; set; } = 512;
    
    /// <summary>
    /// 預設距離度量方式（Cosine, Euclidean, Dot）
    /// </summary>
    public string DistanceMetric { get; set; } = "Cosine";
    
    /// <summary>
    /// 預設搜尋結果數量
    /// </summary>
    public int DefaultLimit { get; set; } = 10;
    
    /// <summary>
    /// 請求逾時時間（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

