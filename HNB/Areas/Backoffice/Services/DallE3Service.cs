using HNB.IntelligentSystems.DallE3.Configuration;
using HNB.IntelligentSystems.DallE3.Core;

namespace HNB.Areas.Backoffice.Services;

public class DallE3Service
{
    private readonly DallE3Engine _engine;
    private readonly DallE3Config _config;

    public DallE3Service(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _config = new DallE3Config
        {
            ApiKey = configuration["DallE3:ApiKey"] ?? string.Empty,
            BaseUrl = configuration["DallE3:BaseUrl"] ?? "https://api.openai.com/v1",
            Organization = configuration["DallE3:Organization"] ?? string.Empty,
            Size = configuration["DallE3:Size"] ?? "1024x1024",
            Quality = configuration["DallE3:Quality"] ?? "standard",
            ImageModel = configuration["DallE3:ImageModel"] ?? "dall-e-3",
            Background = configuration["DallE3:Background"] ?? "transparent",
            Style = configuration["DallE3:Style"] ?? "natural",
            GridPolicy = configuration["DallE3:GridPolicy"] ?? "auto",
            MaxImagesPerBatch = int.Parse(configuration["DallE3:MaxImagesPerBatch"] ?? "8"),
            PreferredImagesPerBatch = int.Parse(configuration["DallE3:PreferredImagesPerBatch"] ?? "6")
        };

        var httpClient = httpClientFactory.CreateClient();
        _engine = new DallE3Engine(_config, httpClient);
    }

    public async Task<(bool success, string? imageBase64, string? error)> GenerateProductImage(
        string prompt,
        CancellationToken cancellationToken = default)
        => await _engine.GenerateImage(prompt, cancellationToken);

    public async Task<(bool success, string? localPath, string? error)> GenerateAndSaveProductImage(
        string prompt,
        string saveDirectory,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var (success, imageBytes, error) = await _engine.GenerateAndDownload(prompt, cancellationToken);
        if (!success || imageBytes == null)
        {
            return (false, null, error ?? "Failed to generate image");
        }

        Directory.CreateDirectory(saveDirectory);
        var filePath = Path.Combine(saveDirectory, fileName);
        await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

        return (true, filePath, null);
    }
}

