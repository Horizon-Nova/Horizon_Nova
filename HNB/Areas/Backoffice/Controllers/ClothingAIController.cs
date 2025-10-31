using Microsoft.AspNetCore.Mvc;
using HNB.Areas.Backoffice.Controllers;
using HNB.Areas.Backoffice.Dtos;
using HNB.Areas.Backoffice.Services;

namespace HNB.Areas.Backoffice.Controllers;

public class ClothingAIController(ClothingAIService svc) : BaseController
{
    public IActionResult Wardrobe()
    {
        var model = svc.LoadWardrobeList();
        return View(model);
    }

    public IActionResult Assistant()
        => View();

    [HttpPost]
    public async Task<IActionResult> SubmitUpload([FromForm] List<IFormFile> files, [FromForm] string? category, [FromForm] string? tags)
    {
        var (success, uploadedFiles, error) = await svc.UploadAndProcessClothing(files);
        
        if (!success)
            return Json(new { success = false, error });

        return Json(new { success = true, uploadedFiles, count = uploadedFiles.Count });
    }

    [HttpDelete]
    public IActionResult Delete(string fileName, string category)
    {
        var (success, error) = svc.DeleteClothing(fileName, category);
        return Json(new { success, message = success ? "刪除成功" : error });
    }

    [HttpGet]
    public IActionResult ListClothing()
    {
        var items = svc.LoadWardrobeList();
        return Json(new { success = true, items });
    }

    [HttpPost]
    public async Task<IActionResult> GenerateProductImages([FromBody] GenerateProductImagesRequest request)
    {
        if (request?.DetectedFileNames == null || request.DetectedFileNames.Count == 0)
            return Json(new { success = false, error = "請提供辨識檔名" });

        var (success, results, error) = await svc.GenerateProductImagesByFileNames(request.DetectedFileNames);
        
        if (!success)
            return Json(new { success = false, error });

        return Json(new { success = true, results });
    }
}
