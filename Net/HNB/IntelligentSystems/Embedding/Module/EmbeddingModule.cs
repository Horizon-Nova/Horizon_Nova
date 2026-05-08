using HNB.IntelligentSystems.Embedding.Configuration;
using HNB.IntelligentSystems.Embedding.Core;
using HNB.IntelligentSystems.Embedding.Core.Providers;
using HNB.IntelligentSystems.Embedding.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HNB.IntelligentSystems.Embedding.Module;

/// <summary>
/// Embedding 模組
/// </summary>
public class EmbeddingModule : IDisposable
{
    private readonly EmbeddingEngine _engine;

    public EmbeddingModule(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var embeddingConfig = LoadConfig(configuration, environment);
        var providers = InitializeProviders(embeddingConfig, environment);
        _engine = new EmbeddingEngine(embeddingConfig, providers);
    }

    private static EmbeddingConfig LoadConfig(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var config = new EmbeddingConfig();
        configuration.GetSection("Embedding").Bind(config);

        var storageRoot = configuration["Storage:Root"] ?? "Areas/Backoffice/storage";
        var basePath = Path.Combine(environment.ContentRootPath, storageRoot, "Ming", "AI", "openai");

        if (!Path.IsPathRooted(config.CLIP.ViT_B_32.ModelPath))
            config.CLIP.ViT_B_32.ModelPath = Path.Combine(basePath, "clip-vit-base-patch32-onnx");

        if (!Path.IsPathRooted(config.CLIP.ViT_L_14.ModelPath))
            config.CLIP.ViT_L_14.ModelPath = Path.Combine(basePath, "clip-vit-large-patch14-onnx");

        return config;
    }

    private static Dictionary<string, IEmbeddingProvider> InitializeProviders(EmbeddingConfig config, IWebHostEnvironment environment)
    {
        var providers = new Dictionary<string, IEmbeddingProvider>();

        providers[config.CLIP.ViT_B_32.Name] = new CLIPEmbeddingProvider(config.CLIP.ViT_B_32, environment);
        providers[config.CLIP.ViT_L_14.Name] = new CLIPEmbeddingProvider(config.CLIP.ViT_L_14, environment);

        return providers;
    }

    /// <summary>
    /// 編碼文本
    /// </summary>
    public async Task<EmbeddingResponse> EncodeTextAsync(string text, string? providerName = null, CancellationToken cancellationToken = default)
    {
        return await _engine.EncodeTextAsync(text, providerName, cancellationToken);
    }

    /// <summary>
    /// 編碼圖像
    /// </summary>
    public async Task<EmbeddingResponse> EncodeImageAsync(byte[] imageBytes, string? providerName = null, CancellationToken cancellationToken = default)
    {
        return await _engine.EncodeImageAsync(imageBytes, providerName, cancellationToken);
    }

    /// <summary>
    /// 處理請求
    /// </summary>
    public async Task<EmbeddingResponse> ProcessRequestAsync(EmbeddingRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Text))
        {
            return await EncodeTextAsync(request.Text, request.ProviderName, cancellationToken);
        }

        if (request.ImageBytes != null && request.ImageBytes.Length > 0)
        {
            return await EncodeImageAsync(request.ImageBytes, request.ProviderName, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.ImageBase64))
        {
            var imageBytes = Convert.FromBase64String(request.ImageBase64.Split(',')[^1]);
            return await EncodeImageAsync(imageBytes, request.ProviderName, cancellationToken);
        }

        return new EmbeddingResponse
        {
            Success = false,
            ErrorMessage = "請求中必須包含文本或圖像"
        };
    }

    /// <summary>
    /// 列出所有 Provider
    /// </summary>
    public List<string> ListProviders()
    {
        return _engine.ListProviders();
    }

    public void Dispose()
    {
        _engine?.Dispose();
    }
}

