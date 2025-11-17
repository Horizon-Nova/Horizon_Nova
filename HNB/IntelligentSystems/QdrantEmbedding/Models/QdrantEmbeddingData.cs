namespace HNB.IntelligentSystems.QdrantEmbedding.Models;

/// <summary>
/// 向量點資料模型
/// </summary>
public class VectorPoint
{
    /// <summary>
    /// 點 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 向量資料
    /// </summary>
    public List<float> Vector { get; set; } = new();
    
    /// <summary>
    /// 附加的 Payload 資料
    /// </summary>
    public Dictionary<string, object>? Payload { get; set; }
}

/// <summary>
/// 搜尋請求模型
/// </summary>
public class SearchRequest
{
    /// <summary>
    /// Collection 名稱
    /// </summary>
    public string CollectionName { get; set; } = string.Empty;
    
    /// <summary>
    /// 查詢向量
    /// </summary>
    public List<float> QueryVector { get; set; } = new();
    
    /// <summary>
    /// 搜尋結果數量限制
    /// </summary>
    public int Limit { get; set; } = 10;
    
    /// <summary>
    /// 分數閾值（可選）
    /// </summary>
    public float? ScoreThreshold { get; set; }
    
    /// <summary>
    /// 過濾條件（可選）
    /// </summary>
    public Dictionary<string, object>? Filter { get; set; }
}

/// <summary>
/// 搜尋結果模型
/// </summary>
public class SearchResult
{
    /// <summary>
    /// 點 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 相似度分數
    /// </summary>
    public float Score { get; set; }
    
    /// <summary>
    /// Payload 資料
    /// </summary>
    public Dictionary<string, object>? Payload { get; set; }
}

/// <summary>
/// 搜尋回應模型
/// </summary>
public class SearchResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 搜尋結果列表
    /// </summary>
    public List<SearchResult> Results { get; set; } = new();
    
    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// 建立 Collection 請求模型
/// </summary>
public class CreateCollectionRequest
{
    /// <summary>
    /// Collection 名稱
    /// </summary>
    public string CollectionName { get; set; } = string.Empty;
    
    /// <summary>
    /// 向量維度
    /// </summary>
    public int VectorSize { get; set; } = 512;
    
    /// <summary>
    /// 距離度量方式
    /// </summary>
    public string DistanceMetric { get; set; } = "Cosine";
}

/// <summary>
/// 插入向量請求模型
/// </summary>
public class UpsertVectorsRequest
{
    /// <summary>
    /// Collection 名稱
    /// </summary>
    public string CollectionName { get; set; } = string.Empty;
    
    /// <summary>
    /// 向量點列表
    /// </summary>
    public List<VectorPoint> Points { get; set; } = new();
}

/// <summary>
/// 通用回應模型
/// </summary>
public class QdrantResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 訊息
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? Error { get; set; }
}

