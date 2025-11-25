namespace HNB.IntelligentSystems.Embedding.Core;

/// <summary>
/// 嵌入提供者介面
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Provider 名稱（用於識別和切換）
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 向量維度
    /// </summary>
    int VectorSize { get; }

    /// <summary>
    /// 是否支援文本嵌入
    /// </summary>
    bool SupportsText { get; }

    /// <summary>
    /// 是否支援圖像嵌入
    /// </summary>
    bool SupportsImage { get; }

    /// <summary>
    /// 編碼文本為向量
    /// </summary>
    /// <param name="text">文本內容</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>嵌入向量</returns>
    Task<List<float>> EncodeTextAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// 編碼圖像為向量
    /// </summary>
    /// <param name="imageBytes">圖像位元組陣列</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>嵌入向量</returns>
    Task<List<float>> EncodeImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default);
}

