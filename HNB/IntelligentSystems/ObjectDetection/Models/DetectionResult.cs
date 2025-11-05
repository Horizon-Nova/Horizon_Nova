using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Models;

public class DetectionResult
{
    public Rect Box { get; set; }
    public string Label { get; set; } = string.Empty;
    public float Score { get; set; }
}

/// <summary>
/// 檢測結果輸出模型（包含裁剪圖片資料）
/// </summary>
public class DetectionOutput
{
    public string Id { get; set; } = string.Empty;
    public int[] Box { get; set; } = new int[4]; // [x, y, width, height]
    public string Label { get; set; } = string.Empty;
    public float Score { get; set; }
    public byte[] CroppedImageBytes { get; set; } = Array.Empty<byte>(); // 裁剪圖片的原始位元組資料
}

/// <summary>
/// 圖片檢測結果輸出模型
/// </summary>
public class ImageDetectionOutput
{
    public string ImageId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<object> Detections { get; set; } = new();
    public int Count { get; set; }
    public string? AnnotatedImageUrl { get; set; }
}

/// <summary>
/// API 回應模型
/// </summary>
public class ObjectDetectionResponse
{
    public bool Success { get; set; }
    public List<ImageDetectionOutput> Images { get; set; } = new();
    public DetectionSummary Summary { get; set; } = new();
    public List<string>? Errors { get; set; }
}

/// <summary>
/// 檢測摘要資訊
/// </summary>
public class DetectionSummary
{
    public int TotalImages { get; set; }
    public int TotalDetections { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

