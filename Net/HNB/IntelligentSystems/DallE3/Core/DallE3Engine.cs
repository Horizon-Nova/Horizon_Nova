using System.Text;
using System.Text.Json;
using HNB.IntelligentSystems.DallE3.Configuration;
using HNB.IntelligentSystems.DallE3.Models;

namespace HNB.IntelligentSystems.DallE3.Core;

public class DallE3Engine(DallE3Config config, HttpClient httpClient)
{
    private readonly DallE3Config _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    private void EnsureHeadersInitialized()
    {
        if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
            if (!string.IsNullOrEmpty(_config.Organization))
            {
                _httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", _config.Organization);
            }
        }
    }

    /// <summary>
    /// 將 base64 字串轉換為 byte 陣列
    /// </summary>
    public byte[]? ConvertBase64ToBytes(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return null;

        return Convert.FromBase64String(base64String);
    }

    /// <summary>
    /// 編輯/組合圖片（使用多張參考圖片）
    /// </summary>
    public async Task<DallE3Result> EditImage(
        string prompt,
        List<byte[]> referenceImages,
        string? model = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("提示詞不能為空");

        if (referenceImages == null || referenceImages.Count == 0)
            throw new ArgumentException("至少需要提供一張參考圖片");

        EnsureHeadersInitialized();

        var modelName = model ?? "gpt-image-1";

        using var formContent = new MultipartFormDataContent();

        formContent.Add(new StringContent(modelName), "model");
        formContent.Add(new StringContent(prompt), "prompt");
        formContent.Add(new StringContent("1"), "n");
        
        if (!string.IsNullOrEmpty(_config.Size))
        {
            formContent.Add(new StringContent(_config.Size), "size");
        }
        
        if (!string.IsNullOrEmpty(_config.Quality))
        {
            formContent.Add(new StringContent(_config.Quality), "quality");
        }

        for (int i = 0; i < referenceImages.Count; i++)
        {
            var imageBytes = referenceImages[i];
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formContent.Add(imageContent, "image[]", $"image_{i}.png");
        }

        var response = await _httpClient.PostAsync($"{_config.BaseUrl}/images/edits", formContent, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorMessage = $"API 錯誤 ({response.StatusCode})";
            
            try
            {
                var errorObj = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (errorObj.TryGetProperty("error", out var errorProp))
                {
                    if (errorProp.TryGetProperty("message", out var messageProp))
                    {
                        var message = messageProp.GetString();
                        if (!string.IsNullOrEmpty(message))
                        {
                            errorMessage = message;
                            
                            if (errorProp.TryGetProperty("code", out var codeProp))
                            {
                                var code = codeProp.GetString();
                                if (code == "billing_hard_limit_reached")
                                {
                                    errorMessage = "已達到計費上限，無法生成圖片。請檢查您的 OpenAI 帳戶計費設定。";
                                }
                                else if (code == "rate_limit_exceeded")
                                {
                                    errorMessage = "請求頻率過高，請稍後再試。";
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                errorMessage = $"API 錯誤 ({response.StatusCode}): {responseContent}";
            }
            
            throw new Exception(errorMessage);
        }

        var result = JsonSerializer.Deserialize<DallE3Result>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null || result.Data == null || result.Data.Count == 0)
            throw new Exception("API 回應格式錯誤");

        return result;
    }

    public DallE3Config GetConfig() => _config;
}

