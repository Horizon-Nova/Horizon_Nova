using System.Text;
using System.Text.Json;
using HNB.IntelligentSystems.DallE3.Configuration;

namespace HNB.IntelligentSystems.DallE3.Core;

public class DallE3Engine
{
    private readonly HttpClient _httpClient;
    private readonly DallE3Config _config;

    public DallE3Engine(DallE3Config config, HttpClient httpClient)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
        if (!string.IsNullOrEmpty(_config.Organization))
        {
            _httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", _config.Organization);
        }
    }

    public async Task<(bool success, string? imageBase64, string? error)> GenerateImage(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = _config.ImageModel,
            prompt = prompt,
            size = _config.Size,
            quality = _config.Quality,
            style = _config.Style,
            response_format = "b64_json"  // 要求返回 base64 格式，避免 HTTP 下載問題
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_config.BaseUrl}/images/generations", content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return (false, null, $"API Error: {response.StatusCode} - {responseContent}");
        }

        var jsonDoc = JsonDocument.Parse(responseContent);
        var imageBase64 = jsonDoc.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString();

        return (true, imageBase64, null);
    }

    /// <summary>
    /// 將 base64 字串轉換為 byte 陣列
    /// </summary>
    public byte[]? ConvertBase64ToBytes(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return null;

        try
        {
            return Convert.FromBase64String(base64String);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool success, byte[]? imageBytes, string? error)> GenerateAndDownload(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var (success, imageBase64, error) = await GenerateImage(prompt, cancellationToken);
        if (!success || string.IsNullOrEmpty(imageBase64))
        {
            return (false, null, error ?? "Failed to generate image");
        }

        var imageBytes = ConvertBase64ToBytes(imageBase64);
        if (imageBytes == null)
        {
            return (false, null, "Failed to convert base64 to bytes");
        }

        return (true, imageBytes, null);
    }

    public DallE3Config GetConfig() => _config;
}

