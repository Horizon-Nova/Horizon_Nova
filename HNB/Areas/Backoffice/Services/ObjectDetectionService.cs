using HNB.IntelligentSystems.ObjectDetection.Configuration;
using HNB.IntelligentSystems.ObjectDetection.Core;
using HNB.IntelligentSystems.ObjectDetection.Models;
using OpenCvSharp;

namespace HNB.Areas.Backoffice.Services;

public class ObjectDetectionService : IDisposable
{
    private readonly DetectionEngine _engine;
    private readonly ObjectDetectionConfig _config;

    public ObjectDetectionService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _config = new ObjectDetectionConfig
        {
            ModelPath = GetModelPath(environment.ContentRootPath, configuration),
            VocabPath = GetVocabPath(environment.ContentRootPath, configuration)
        };
        _engine = new DetectionEngine(_config);
    }

    private string GetModelPath(string contentRoot, IConfiguration config)
    {
        var storageRoot = config["Storage:Root"] ?? "Areas/Backoffice/storage";
        return Path.Combine(contentRoot, storageRoot, "AI", "groundingdino.onnx");
    }

    private string GetVocabPath(string contentRoot, IConfiguration config)
    {
        var storageRoot = config["Storage:Root"] ?? "Areas/Backoffice/storage";
        return Path.Combine(contentRoot, storageRoot, "AI", "vocab.txt");
    }

    public List<DetectionResult> DetectObjects(Mat image, string? textPrompt = null)
    {
        string prompt = textPrompt ?? _config.TextPrompt;
        return _engine.Detect(image, prompt);
    }

    public List<DetectionResult> DetectObjectsFromBytes(byte[] imageBytes, string? textPrompt = null)
    {
        using var mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);
        if (mat.Empty())
            throw new ArgumentException("Invalid image data");
        return DetectObjects(mat, textPrompt);
    }

    public ObjectDetectionConfig GetConfig() => _config;

    public void Dispose()
    {
        _engine?.Dispose();
    }
}

