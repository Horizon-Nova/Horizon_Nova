namespace HNB.IntelligentSystems.DallE3.Models;

/// <summary>
/// DallE3 圖片編輯請求模型（從 base64 參考圖片）
/// </summary>
public class DallE3EditRequest
{
    public string Prompt { get; set; } = string.Empty;
    public List<string> ImagesBase64 { get; set; } = new(); // base64 格式的參考圖片
    public string? Model { get; set; } = "gpt-image-1"; // 預設使用 gpt-image-1
}

/// <summary>
/// DallE3 圖片編輯結果輸出模型
/// </summary>
public class DallE3Output
{
    public string ImageId { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public byte[] ImageBytes { get; set; } = Array.Empty<byte>(); // 編輯後的圖片原始位元組資料
}

/// <summary>
/// 圖片編輯結果輸出模型
/// </summary>
public class ImageEditOutput
{
    public string ImageId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Prompt { get; set; }
    public string? ImageBase64 { get; set; } // base64 格式（用於 EditBase64）
    public string? ImageUrl { get; set; } // URL 格式（用於 EditImage）
}

/// <summary>
/// API 回應模型
/// </summary>
public class DallE3Response
{
    public bool Success { get; set; }
    public List<ImageEditOutput> Images { get; set; } = new();
    public EditSummary Summary { get; set; } = new();
    public List<string>? Errors { get; set; }
}

/// <summary>
/// 編輯摘要資訊
/// </summary>
public class EditSummary
{
    public int TotalImages { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

