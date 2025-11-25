using HNB.IntelligentSystems.DallE3.Models;
using HNB.IntelligentSystems.DallE3.Module;
using HNB.IntelligentSystems.GroundingDINO.Utils;
using Microsoft.AspNetCore.Mvc;

namespace HNB.IntelligentSystems.DallE3.Api;

/// <summary>
/// DallE3 圖片編輯 API Controller
/// 統一返回格式：所有 API 都返回 images 陣列格式
/// EditBase64 → base64 輸出，EditImage → URL 輸出
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DallE3ApiController(DallE3Module module, IWebHostEnvironment env) : ControllerBase
{
    private const string StorageRoot = "Areas/Backoffice/storage/DallE3";

    /// <summary>
    /// 簡介：編輯/組合圖片並返回 base64 格式（從 base64 參考圖片）
    /// 端口：POST /api/DallE3Api/EditBase64
    /// </summary>
    [HttpPost("EditBase64")]
    public async Task<IActionResult> EditBase64([FromBody] DallE3EditRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new { success = false, error = "請提供提示詞" });

        if (request.ImagesBase64 == null || request.ImagesBase64.Count == 0)
            return BadRequest(new { success = false, error = "請提供至少一張參考圖片" });

        var (referenceImages, parseErrors) = ImageUtils.ParseBase64Images(request.ImagesBase64);

        if (referenceImages.Count == 0)
            return BadRequest(new { success = false, error = "無法解析任何參考圖片", errors = parseErrors });

        var outputs = await module.EditImage(request.Prompt, referenceImages, request.Model);

        var imageResults = outputs.Select(output => new ImageEditOutput
        {
            ImageId = output.ImageId,
            Success = true,
            Prompt = output.Prompt,
            ImageBase64 = $"data:image/png;base64,{Convert.ToBase64String(output.ImageBytes)}"
        }).ToList();

        return Ok(BuildResponse(imageResults, new List<string>()));
    }

    /// <summary>
    /// 簡介：編輯/組合圖片並返回 URL 格式（從上傳的圖片文件）
    /// 端口：POST /api/DallE3Api/EditImage
    /// </summary>
    [HttpPost("EditImage")]
    public async Task<IActionResult> EditImage([FromForm(Name = "image[]")] List<IFormFile> images, [FromForm(Name = "prompts[]")] List<string> prompts)
    {
        if (images == null || images.Count == 0)
            return BadRequest(new { success = false, error = "請提供至少一張參考圖片" });

        if (prompts == null || prompts.Count == 0)
            return BadRequest(new { success = false, error = "請提供至少一個提示詞" });

        var (referenceImages, readErrors) = await ImageUtils.ReadFormFileImages(images);

        if (referenceImages.Count == 0)
            return BadRequest(new { success = false, error = "無法讀取任何參考圖片", errors = readErrors });

        var prompt = prompts[0];
        const string fixedModel = "gpt-image-1";

        var outputs = await module.EditImage(prompt, referenceImages, fixedModel);

        var dateStr = DateTime.Now.ToString("yyyy_MM_dd");
        var savePath = Path.Combine(env.ContentRootPath, StorageRoot, dateStr);
        Directory.CreateDirectory(savePath);

        var imageResults = new List<ImageEditOutput>();

        foreach (var output in outputs)
        {
            var fileName = $"{output.ImageId}.png";
            var filePath = Path.Combine(savePath, fileName);

            ImageUtils.SaveImageFromBytes(output.ImageBytes, filePath);

            var imageUrl = $"/storage/DallE3/{dateStr}/{fileName}";
            imageResults.Add(new ImageEditOutput
            {
                ImageId = output.ImageId,
                Success = true,
                Prompt = output.Prompt,
                ImageUrl = imageUrl
            });
        }

        return Ok(BuildResponse(imageResults, new List<string>()));
    }

    /// <summary>
    /// 簡介：建立統一的回應格式
    /// </summary>
    private object BuildResponse(List<ImageEditOutput> imageResults, List<string> errors)
    {
        var successCount = imageResults.Count(r => r.Success);
        var failureCount = imageResults.Count(r => !r.Success);

        return new DallE3Response
        {
            Success = errors.Count == 0,
            Images = imageResults,
            Summary = new EditSummary
            {
                TotalImages = imageResults.Count,
                SuccessCount = successCount,
                FailureCount = failureCount
            },
            Errors = errors.Count > 0 ? errors : null
        };
    }
}

