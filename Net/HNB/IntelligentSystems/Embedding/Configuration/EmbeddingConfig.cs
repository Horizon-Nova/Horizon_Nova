namespace HNB.IntelligentSystems.Embedding.Configuration;

/// <summary>
/// Embedding 模組配置
/// </summary>
public class EmbeddingConfig
{
    /// <summary>
    /// 預設使用的 Provider 名稱
    /// </summary>
    public string DefaultProvider { get; set; } = "CLIP_ViT_B_32";

    /// <summary>
    /// CLIP 模型配置
    /// </summary>
    public CLIPEmbeddingConfig CLIP { get; set; } = new();
}

/// <summary>
/// CLIP 模型配置
/// </summary>
public class CLIPEmbeddingConfig
{
    /// <summary>
    /// CLIP ViT-B/32 配置
    /// </summary>
    public CLIPModelConfig ViT_B_32 { get; set; } = new()
    {
        Name = "CLIP_ViT_B_32",
        VectorSize = 512,
        CollectionName = "clip_vit_base_patch32"
    };

    /// <summary>
    /// CLIP ViT-L/14 配置
    /// </summary>
    public CLIPModelConfig ViT_L_14 { get; set; } = new()
    {
        Name = "CLIP_ViT_L_14",
        VectorSize = 768,
        CollectionName = "clip_vit_large_patch14"
    };
}

/// <summary>
/// CLIP 單一模型配置
/// </summary>
public class CLIPModelConfig
{
    /// <summary>
    /// 模型名稱（用於識別）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ONNX 模型路徑（相對於 ContentRoot）
    /// </summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>
    /// 向量維度
    /// </summary>
    public int VectorSize { get; set; } = 512;

    /// <summary>
    /// Qdrant Collection 名稱
    /// </summary>
    public string CollectionName { get; set; } = "default";
}

