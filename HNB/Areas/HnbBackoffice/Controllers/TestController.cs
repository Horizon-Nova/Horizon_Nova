using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class TestController : Controller
{
    // GET: /HnbBackoffice/Test
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("請選擇一個檔案");

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "32676b967fefa81cab8b49c655979038");

        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var filePath = Path.Combine(uploadsDir, file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { message = "上傳成功", path = $"/32676b967fefa81cab8b49c655979038/{file.FileName}" });
    }
}
