using HNB.IntelligentSystems.DallE3.Configuration;
using HNB.IntelligentSystems.DallE3.Core;
using HNB.IntelligentSystems.DallE3.Models;

namespace HNB.IntelligentSystems.DallE3.Module;

/// <summary>
/// DallE3 圖片編輯模組
/// 負責執行圖片編輯/組合，封裝 DallE3Engine 的邏輯
/// </summary>
public class DallE3Module(DallE3Config config, IHttpClientFactory httpClientFactory) : IDisposable
{
    // 參數驗證
    private readonly DallE3Config _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private DallE3Engine? _engine;

    #region 核心編輯方法

    /// <summary>
    /// 編輯/組合圖片（使用多張參考圖片）
    /// </summary>
    public async Task<(bool success, List<DallE3Output> outputs, string? error)> EditImage(
        string prompt,
        List<byte[]> referenceImages,
        string? model = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return (false, new List<DallE3Output>(), "提示詞不能為空");

        if (referenceImages == null || referenceImages.Count == 0)
            return (false, new List<DallE3Output>(), "至少需要提供一張參考圖片");

        var engine = GetOrCreateEngine();

        var (success, result, error) = await engine.EditImage(prompt, referenceImages, model, cancellationToken);

        if (!success || result == null || result.Data.Count == 0)
        {
            return (false, new List<DallE3Output>(), error ?? "圖片編輯失敗");
        }

        var outputs = new List<DallE3Output>();
        
        var firstImage = result.Data.FirstOrDefault(d => !string.IsNullOrEmpty(d.B64Json));
        if (firstImage != null)
        {
            var imageBytes = engine.ConvertBase64ToBytes(firstImage.B64Json);
            if (imageBytes != null)
            {
                var imageId = $"img_{Guid.NewGuid():N}";
                outputs.Add(new DallE3Output
                {
                    ImageId = imageId,
                    Prompt = prompt,
                    ImageBytes = imageBytes
                });
            }
        }

        if (outputs.Count == 0)
        {
            return (false, new List<DallE3Output>(), "無法轉換任何 base64 圖片資料");
        }

        return (true, outputs, null);
    }

    #endregion

    #region 配置方法

    /// <summary>
    /// 載入配置資訊
    /// </summary>
    public DallE3Config LoadConfig() => _config;

    #endregion

    #region 私有方法

    private DallE3Engine GetOrCreateEngine()
    {
        if (_engine == null)
        {
            var httpClient = _httpClientFactory.CreateClient();
            _engine = new DallE3Engine(_config, httpClient);
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

