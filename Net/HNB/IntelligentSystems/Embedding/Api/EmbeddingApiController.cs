using HNB.IntelligentSystems.Embedding.Models;
using HNB.IntelligentSystems.Embedding.Module;
using HNB.IntelligentSystems.GroundingDINO.Utils;
using Microsoft.AspNetCore.Mvc;

namespace HNB.IntelligentSystems.Embedding.Api;

/// <summary>
/// Embedding API Controller
/// 已停用 - AI 模組已關閉以節省記憶體
/// </summary>
/*
[ApiController]
[Route("api/[controller]")]
public class EmbeddingApiController(EmbeddingModule module) : ControllerBase
{
    /// <summary>
    /// 列出所有 Provider
    /// </summary>
    /// <returns>Provider 列表</returns>
    /// <remarks>端口：GET /api/EmbeddingApi/Providers</remarks>
    [HttpGet("Providers")]
    public IActionResult ListProviders()
    {
        var providers = module.ListProviders();
        return Ok(new
        {
            success = true,
            providers = providers,
            count = providers.Count
        });
    }

    /// <summary>
    /// 編碼文本
    /// </summary>
    /// <param name="text">文本內容</param>
    /// <param name="providerName">Provider 名稱（可選）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>嵌入向量</returns>
    /// <remarks>端口：POST /api/EmbeddingApi/EncodeText</remarks>
    [HttpPost("EncodeText")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded", "application/json")]
    public async Task<IActionResult> EncodeText([FromForm] string? text = null, [FromForm] string? providerName = null, CancellationToken cancellationToken = default)
    {
        string? requestText = text;
        string? requestProviderName = providerName;

        // 如果 Form-Data 為空，嘗試從 JSON body 讀取
        if (string.IsNullOrWhiteSpace(requestText))
        {
            var contentType = Request.ContentType?.ToLowerInvariant() ?? "";
            if (contentType.Contains("application/json"))
            {
                Request.EnableBuffering();
                Request.Body.Position = 0;
                using var reader = new StreamReader(Request.Body, leaveOpen: true);
                var bodyContent = await reader.ReadToEndAsync();
                Request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(bodyContent))
                {
                    var jsonRequest = System.Text.Json.JsonSerializer.Deserialize<EmbeddingTextRequest>(bodyContent);
                    if (jsonRequest != null)
                    {
                        requestText = jsonRequest.Text;
                        requestProviderName = jsonRequest.ProviderName;
                    }
                }
            }
        }

        if (string.IsNullOrWhiteSpace(requestText))
            return BadRequest(new { success = false, error = "請提供文本內容" });

        var response = await module.EncodeTextAsync(requestText, requestProviderName, cancellationToken);

        if (!response.Success)
            return BadRequest(new { success = false, error = response.ErrorMessage, providerName = response.ProviderName });

        return Ok(new
        {
            success = true,
            vector = response.Vector,
            vectorSize = response.VectorSize,
            providerName = response.ProviderName
        });
    }

    /// <summary>
    /// 編碼圖像（Base64）
    /// </summary>
    /// <param name="request">Base64 圖像請求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>嵌入向量</returns>
    /// <remarks>端口：POST /api/EmbeddingApi/EncodeImageBase64</remarks>
    [HttpPost("EncodeImageBase64")]
    public async Task<IActionResult> EncodeImageBase64([FromBody] EmbeddingImageBase64Request request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ImageBase64))
            return BadRequest(new { success = false, error = "請提供 base64 圖片資料" });

        var (parseSuccess, imageBytes, parseError) = ImageUtils.ParseBase64Image(request.ImageBase64);
        if (!parseSuccess || imageBytes == null)
            return BadRequest(new { success = false, error = parseError ?? "無法解析 base64 圖片資料" });

        var response = await module.EncodeImageAsync(imageBytes, request.ProviderName, cancellationToken);

        if (!response.Success)
            return BadRequest(new { success = false, error = response.ErrorMessage, providerName = response.ProviderName });

        return Ok(new
        {
            success = true,
            vector = response.Vector,
            vectorSize = response.VectorSize,
            providerName = response.ProviderName
        });
    }

    /// <summary>
    /// 編碼圖像（文件上傳）
    /// </summary>
    /// <param name="image">圖片文件</param>
    /// <param name="providerName">Provider 名稱（可選）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>嵌入向量</returns>
    /// <remarks>端口：POST /api/EmbeddingApi/EncodeImage</remarks>
    [HttpPost("EncodeImage")]
    public async Task<IActionResult> EncodeImage([FromForm] IFormFile image, [FromForm] string? providerName = null, CancellationToken cancellationToken = default)
    {
        if (image == null || image.Length == 0)
            return BadRequest(new { success = false, error = "請提供圖片文件" });

        var (readSuccess, imageBytes, readError) = await ImageUtils.ReadFormFileImage(image);
        if (!readSuccess || imageBytes == null)
            return BadRequest(new { success = false, error = readError ?? "無法讀取圖片文件" });

        var response = await module.EncodeImageAsync(imageBytes, providerName, cancellationToken);

        if (!response.Success)
            return BadRequest(new { success = false, error = response.ErrorMessage, providerName = response.ProviderName });

        return Ok(new
        {
            success = true,
            vector = response.Vector,
            vectorSize = response.VectorSize,
            providerName = response.ProviderName
        });
    }

    /// <summary>
    /// 統一處理請求
    /// </summary>
    /// <param name="request">嵌入請求（文本或圖像）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>嵌入向量</returns>
    /// <remarks>端口：POST /api/EmbeddingApi/Encode</remarks>
    [HttpPost("Encode")]
    public async Task<IActionResult> Encode([FromBody] EmbeddingRequest request, CancellationToken cancellationToken = default)
    {
        var response = await module.ProcessRequestAsync(request, cancellationToken);

        if (!response.Success)
            return BadRequest(new { success = false, error = response.ErrorMessage, providerName = response.ProviderName });

        return Ok(new
        {
            success = true,
            vector = response.Vector,
            vectorSize = response.VectorSize,
            providerName = response.ProviderName
        });
    }
}
*/

/// <summary>
/// 文本請求
/// </summary>
public class EmbeddingTextRequest
{
    public string Text { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
}

/// <summary>
/// Base64 圖像請求
/// </summary>
public class EmbeddingImageBase64Request
{
    public string ImageBase64 { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
}

