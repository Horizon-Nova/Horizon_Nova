using System;
using System.Collections.Generic;

namespace HNB.IntelligentSystems.GroundingDINO.Models;

public class ImageDetectionData
{
    public string ImageFileName { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string ResultImagePath { get; set; } = string.Empty;
    public int DetectionCount { get; set; }
    public List<DetectionResult> Detections { get; set; } = new List<DetectionResult>();
    public DateTime ProcessedAt { get; set; }
}

public class DetectionSessionData
{
    public DateTime SessionStartTime { get; set; }
    public DateTime SessionEndTime { get; set; }
    public string TextPrompt { get; set; } = string.Empty;
    public float BoxThreshold { get; set; }
    public float TextThreshold { get; set; }
    public int TotalImages { get; set; }
    public int ProcessedImages { get; set; }
    public List<ImageDetectionData> Images { get; set; } = new List<ImageDetectionData>();
}

public class GroundingDINORequest
{
    public List<string> ImagesBase64 { get; set; } = new();
    public string? TextPrompt { get; set; }
}

