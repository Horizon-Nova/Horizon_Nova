using System.Text.Json.Serialization;

namespace HNB.IntelligentSystems.DallE3.Models;

/// <summary>
/// DallE3 API 回應結果模型（對應官方 API 格式）
/// </summary>
public class DallE3Result
{
    public long Created { get; set; }
    public List<DallE3ImageData> Data { get; set; } = new();
    public DallE3Usage? Usage { get; set; }
}

/// <summary>
/// 圖片資料
/// </summary>
public class DallE3ImageData
{
    [JsonPropertyName("b64_json")]
    public string? B64Json { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    [JsonPropertyName("revised_prompt")]
    public string? RevisedPrompt { get; set; }
}

/// <summary>
/// Token 使用資訊
/// </summary>
public class DallE3Usage
{
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
    
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }
    
    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }
    
    [JsonPropertyName("input_tokens_details")]
    public DallE3InputTokensDetails? InputTokensDetails { get; set; }
}

/// <summary>
/// 輸入 Token 詳細資訊
/// </summary>
public class DallE3InputTokensDetails
{
    [JsonPropertyName("text_tokens")]
    public int TextTokens { get; set; }
    
    [JsonPropertyName("image_tokens")]
    public int ImageTokens { get; set; }
}

