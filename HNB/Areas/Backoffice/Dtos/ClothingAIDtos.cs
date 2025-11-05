namespace HNB.Areas.Backoffice.Dtos;

/// <summary>
/// 服裝辨識請求
/// </summary>
public class ObjectDetectionRequest
{
    public string? ImageBase64 { get; set; }
    public string? TextPrompt { get; set; }
}

/// <summary>
/// 批次服裝辨識請求
/// </summary>
public class BatchDetectionRequest
{
    public List<ImageData> Images { get; set; } = new();
    public string? TextPrompt { get; set; }
}

/// <summary>
/// 圖片資料
/// </summary>
public class ImageData
{
    public string FileName { get; set; } = string.Empty;
    public string Base64 { get; set; } = string.Empty;
    public string? Prompt { get; set; }
}

/// <summary>
/// 衣櫃項目
/// </summary>
public class WardrobeItem
{
    public string FileName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string WebPath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// 上傳服裝響應
/// </summary>
public class UploadClothingResponse
{
    public bool Success { get; set; }
    public List<UploadedClothingInfo> UploadedFiles { get; set; } = new();
    public int Count { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// 上傳成功的服裝資訊
/// </summary>
public class UploadedClothingInfo
{
    public string FileName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Error { get; set; }
}

/// <summary>
/// 刪除服裝響應
/// </summary>
public class DeleteClothingResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// 列出服裝響應
/// </summary>
public class ListClothingResponse
{
    public bool Success { get; set; }
    public List<WardrobeItem> Items { get; set; } = new();
}

/// <summary>
/// 商品圖生成響應
/// </summary>
public class GenerateProductImagesResponse
{
    public bool Success { get; set; }
    public List<ProductImageInfo> Results { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// 商品圖資訊
/// </summary>
public class ProductImageInfo
{
    public string SourcePath { get; set; } = string.Empty;
    public string? ProductPath { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// 商品圖生成請求
/// </summary>
public class GenerateProductImagesRequest
{
    /// <summary>
    /// 辨識檔名列表，格式：{日期}/{類別}/{檔名}
    /// 例如：2025_10_31/clothes/abc123_clothes_0.png
    /// </summary>
    public List<string> DetectedFileNames { get; set; } = new();
}

/// <summary>
/// 批量刪除請求
/// </summary>
public class BatchDeleteRequest
{
    public List<BatchDeleteItem> Items { get; set; } = new();
}

/// <summary>
/// 批量刪除項目
/// </summary>
public class BatchDeleteItem
{
    public string FileName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// 批量刪除響應
/// </summary>
public class BatchDeleteResponse
{
    public bool Success { get; set; }
    public List<BatchDeleteItemResult> Results { get; set; } = new();
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// 批量刪除項目結果
/// </summary>
public class BatchDeleteItemResult
{
    public string FileName { get; set; } = string.Empty;
    public bool Success { get; set; }
}

