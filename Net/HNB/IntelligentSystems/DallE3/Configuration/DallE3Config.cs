namespace HNB.IntelligentSystems.DallE3.Configuration;

public class DallE3Config
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string Organization { get; set; } = "org-NUHCBKCXeZKeaLgi7IBLO6b1";
    public string Size { get; set; } = "1024x1024";
    public string Quality { get; set; } = "low";
    public string ImageModel { get; set; } = "gpt-image-1";
    public string Background { get; set; } = "transparent";
    public string Style { get; set; } = "natural";
    public string GridPolicy { get; set; } = "auto";
    public int MaxImagesPerBatch { get; set; } = 8;
    public int PreferredImagesPerBatch { get; set; } = 6;
}

