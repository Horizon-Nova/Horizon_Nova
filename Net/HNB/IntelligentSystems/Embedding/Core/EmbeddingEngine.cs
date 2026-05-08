using HNB.IntelligentSystems.Embedding.Configuration;
using HNB.IntelligentSystems.Embedding.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HNB.IntelligentSystems.Embedding.Core;

/// <summary>
/// Embedding 引擎
/// </summary>
public class EmbeddingEngine : IDisposable
{
    private readonly EmbeddingConfig _config;
    private readonly Dictionary<string, IEmbeddingProvider> _providers;
    private readonly IEmbeddingProvider _defaultProvider;

    public EmbeddingEngine(EmbeddingConfig config, Dictionary<string, IEmbeddingProvider> providers)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));

        if (!_providers.TryGetValue(_config.DefaultProvider, out var defaultProvider))
            throw new ArgumentException($"預設 Provider '{_config.DefaultProvider}' 不存在", nameof(providers));

        _defaultProvider = defaultProvider;
    }

    /// <summary>
    /// 取得指定的 Provider
    /// </summary>
    public IEmbeddingProvider GetProvider(string? providerName = null)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return _defaultProvider;

        return _providers.TryGetValue(providerName, out var provider)
            ? provider
            : _defaultProvider;
    }

    /// <summary>
    /// 編碼文本
    /// </summary>
    public async Task<EmbeddingResponse> EncodeTextAsync(string text, string? providerName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = GetProvider(providerName);
            
            if (!provider.SupportsText)
                return new EmbeddingResponse
                {
                    Success = false,
                    ErrorMessage = $"Provider '{provider.Name}' 不支援文本編碼",
                    ProviderName = provider.Name
                };

            var vector = await provider.EncodeTextAsync(text, cancellationToken);

            return new EmbeddingResponse
            {
                Success = true,
                Vector = vector,
                VectorSize = vector.Count,
                ProviderName = provider.Name
            };
        }
        catch (Exception ex)
        {
            return new EmbeddingResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProviderName = providerName ?? _config.DefaultProvider
            };
        }
    }

    /// <summary>
    /// 編碼圖像
    /// </summary>
    public async Task<EmbeddingResponse> EncodeImageAsync(byte[] imageBytes, string? providerName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = GetProvider(providerName);
            
            if (!provider.SupportsImage)
                return new EmbeddingResponse
                {
                    Success = false,
                    ErrorMessage = $"Provider '{provider.Name}' 不支援圖像編碼",
                    ProviderName = provider.Name
                };

            var vector = await provider.EncodeImageAsync(imageBytes, cancellationToken);

            return new EmbeddingResponse
            {
                Success = true,
                Vector = vector,
                VectorSize = vector.Count,
                ProviderName = provider.Name
            };
        }
        catch (Exception ex)
        {
            return new EmbeddingResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProviderName = providerName ?? _config.DefaultProvider
            };
        }
    }

    /// <summary>
    /// 列出所有 Provider
    /// </summary>
    public List<string> ListProviders()
    {
        return _providers.Keys.ToList();
    }

    public void Dispose()
    {
        foreach (var provider in _providers.Values)
        {
            if (provider is IDisposable disposable)
                disposable.Dispose();
        }
    }
}

