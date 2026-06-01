namespace HNB.IntelligentSystems.Embedding.Models;

/// <summary>
/// 嵌入請求
/// </summary>
public class EmbeddingRequest
{
    /// <summary>
    /// 文本內容（可選）
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 圖像內容（Base64 編碼，可選）
    /// </summary>
    public string? ImageBase64 { get; set; }

    /// <summary>
    /// 圖像內容（Byte 陣列，可選）
    /// </summary>
    public byte[]? ImageBytes { get; set; }

    /// <summary>
    /// 指定使用的 Provider 名稱（可選，使用預設 Provider）
    /// </summary>
    public string? ProviderName { get; set; }
}

/// <summary>
/// 嵌入回應
/// </summary>
public class EmbeddingResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 嵌入向量
    /// </summary>
    public List<float>? Vector { get; set; }

    /// <summary>
    /// 向量維度
    /// </summary>
    public int VectorSize { get; set; }

    /// <summary>
    /// 使用的 Provider 名稱
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// 錯誤訊息（如果失敗）
    /// </summary>
    public string? ErrorMessage { get; set; }
}

