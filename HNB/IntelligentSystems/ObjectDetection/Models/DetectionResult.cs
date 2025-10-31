using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Models;

public class DetectionResult
{
    public Rect Box { get; set; }
    public string Label { get; set; } = string.Empty;
    public float Score { get; set; }
}

