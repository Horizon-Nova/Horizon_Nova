using HNB.IntelligentSystems.ObjectDetection.Api;
using HNB.IntelligentSystems.ObjectDetection.Configuration;
using HNB.IntelligentSystems.ObjectDetection.Module;

namespace HNB.Areas.Backoffice.Services;

public class ObjectDetectionService(IConfiguration configuration, IWebHostEnvironment environment) : IDisposable
{
    private readonly ObjectDetectionConfig _config = new ObjectDetectionConfig
    {
        ModelPath = GetModelPath(environment.ContentRootPath, configuration),
        VocabPath = GetVocabPath(environment.ContentRootPath, configuration)
    };
    private readonly ObjectDetectionModule _module = new ObjectDetectionModule(new ObjectDetectionConfig
    {
        ModelPath = GetModelPath(environment.ContentRootPath, configuration),
        VocabPath = GetVocabPath(environment.ContentRootPath, configuration)
    });
    private readonly ObjectDetectionApi _api = new ObjectDetectionApi(new ObjectDetectionModule(new ObjectDetectionConfig
    {
        ModelPath = GetModelPath(environment.ContentRootPath, configuration),
        VocabPath = GetVocabPath(environment.ContentRootPath, configuration)
    }));

    private static string GetModelPath(string contentRoot, IConfiguration config)
    {
        var storageRoot = config["Storage:Root"] ?? "Areas/Backoffice/storage";
        return Path.Combine(contentRoot, storageRoot, "AI", "groundingdino.onnx");
    }

    private static string GetVocabPath(string contentRoot, IConfiguration config)
    {
        var storageRoot = config["Storage:Root"] ?? "Areas/Backoffice/storage";
        return Path.Combine(contentRoot, storageRoot, "AI", "vocab.txt");
    }

    #region 模組端口方法（內部使用）

    public (bool IsReady, string Message) LoadModelStatus() => _module.LoadModelStatus();
    public bool IsModelReady() => _module.IsModelReady();
    public (bool success, List<DetectionResult> results, string? error) DetectObjects(OpenCvSharp.Mat image, string? textPrompt = null) 
        => _module.DetectObjects(image, textPrompt);
    public (bool success, List<DetectionResult> results, string? error) DetectObjectsFromBytes(byte[] imageBytes, string? textPrompt = null) 
        => _module.DetectObjectsFromBytes(imageBytes, textPrompt);
    public ObjectDetectionConfig LoadConfig() => _module.LoadConfig();

    #endregion

    #region API 端口方法（外部使用）

    public (bool IsAvailable, string Message) CheckServiceStatus() => _api.CheckServiceStatus();
    public (bool success, List<DetectionResult>? results, string? error) Detect(byte[] imageBytes, string? textPrompt = null) 
        => _api.Detect(imageBytes, textPrompt);
    public (bool success, List<DetectionResult>? results, string? error) DetectFromFile(string imagePath, string? textPrompt = null) 
        => _api.DetectFromFile(imagePath, textPrompt);
    
    /// <summary>
    /// 辨識圖片並返回處理後的圖片（支援圖片文件或 base64）
    /// </summary>
    public (bool success, List<DetectionResult>? detections, string? imageUrl, string? imageBase64, string? error) DetectImage(
        byte[] imageBytes,
        string? textPrompt = null,
        string? savePath = null,
        string? fileName = null,
        string? webPathPrefix = null)
        => _api.DetectImage(imageBytes, textPrompt, savePath, fileName, webPathPrefix);
    
    /// <summary>
    /// 從 base64 辨識圖片並返回處理後的圖片（base64 格式）
    /// </summary>
    public (bool success, List<DetectionResult>? detections, string? imageBase64, string? error) DetectImageFromBase64(
        string imageBase64,
        string? textPrompt = null)
        => _api.DetectImageFromBase64(imageBase64, textPrompt);

    #endregion

    public void Dispose()
    {
        _module?.Dispose();
    }
}

