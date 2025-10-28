using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class DesignerController : Controller
{
    private const string StorageRoot = "Areas/Backoffice/storage/Designer";
    private readonly IWebHostEnvironment _env;

    public DesignerController(IWebHostEnvironment env)
    {
        _env = env;
    }

    // Designer 主畫面（新建或編輯）
    public IActionResult Designer(string? edit = null)
    {
        ViewBag.EditId = edit;
        return View();
    }

    // 儲存頁面（返回 GUID）
    [HttpPost]
    public async Task<IActionResult> SavePage([FromBody] SavePageRequest request)
    {
        var pageId = string.IsNullOrEmpty(request.pageId) 
            ? Guid.NewGuid().ToString() 
            : request.pageId;

        var storagePath = Path.Combine(_env.ContentRootPath, StorageRoot);
        
        // 確保 storage 目錄存在
        if (!Directory.Exists(storagePath))
        {
            Directory.CreateDirectory(storagePath);
        }

        var filePath = Path.Combine(storagePath, $"{pageId}.json");
        
        DateTime createdAt = DateTime.UtcNow;
        if (System.IO.File.Exists(filePath))
        {
            var existingJson = await System.IO.File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var existingData = JsonSerializer.Deserialize<JsonElement>(existingJson);
            if (existingData.TryGetProperty("CreatedAt", out var existingCreatedAt))
            {
                createdAt = existingCreatedAt.GetDateTime();
            }
        }

        var pageData = new
        {
            Id = pageId,
            Title = request.title ?? "Untitled Page",
            Html = request.html,
            Css = request.css,
            GjsData = request.gjsData,
            CreatedAt = createdAt,
            UpdatedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(pageData, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        await System.IO.File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

        return Json(new { success = true, pageId = pageId });
    }

    // 讀取頁面（返回 JSON）
    [HttpGet]
    public async Task<IActionResult> LoadPage(string id)
    {
        var storagePath = Path.Combine(_env.ContentRootPath, StorageRoot);
        var filePath = Path.Combine(storagePath, $"{id}.json");

        if (!System.IO.File.Exists(filePath))
        {
            return Json(new { success = false, error = "頁面不存在" });
        }

        var json = await System.IO.File.ReadAllTextAsync(filePath, Encoding.UTF8);
        var pageData = JsonSerializer.Deserialize<JsonElement>(json);

        // 轉換為 JS 期待的格式
        var response = new
        {
            success = true,
            page = new
            {
                pageId = pageData.GetProperty("Id").GetString(),
                title = pageData.GetProperty("Title").GetString(),
                html = pageData.TryGetProperty("Html", out var html) ? html.GetString() : null,
                css = pageData.TryGetProperty("Css", out var css) ? css.GetString() : null,
                gjsData = pageData.TryGetProperty("GjsData", out var gjsData) ? gjsData.GetString() : null
            }
        };

        return Json(response);
    }

    // 檢視頁面（渲染 HTML）
    [HttpGet]
    [Route("/Backoffice/Designer/View/{id}")]
    public async Task<IActionResult> ViewPage(string id)
    {
        var storagePath = Path.Combine(_env.ContentRootPath, StorageRoot);
        var filePath = Path.Combine(storagePath, $"{id}.json");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("頁面不存在");
        }

        var json = await System.IO.File.ReadAllTextAsync(filePath, Encoding.UTF8);
        var pageData = JsonSerializer.Deserialize<JsonElement>(json);

        // 直接從 GrapesJS 儲存的 Html 和 Css 取得內容
        var html = pageData.TryGetProperty("Html", out JsonElement htmlElement) 
            ? htmlElement.GetString() ?? "" 
            : "";
        var css = pageData.TryGetProperty("Css", out JsonElement cssElement) 
            ? cssElement.GetString() ?? "" 
            : "";
        var title = pageData.TryGetProperty("Title", out JsonElement titleElement) 
            ? titleElement.GetString() 
            : "Untitled Page";

        ViewBag.Html = html;
        ViewBag.Css = css;
        ViewBag.Title = title;
        ViewBag.PageId = id;

        return View("View");
    }

    // 列出所有頁面
    [HttpGet]
    public IActionResult ListPages()
    {
        var storagePath = Path.Combine(_env.ContentRootPath, StorageRoot);
        
        if (!Directory.Exists(storagePath))
        {
            return Json(new { success = true, pages = new List<object>() });
        }

        var files = Directory.GetFiles(storagePath, "*.json");
        var pages = new List<object>();

        foreach (var file in files)
        {
            var json = System.IO.File.ReadAllText(file, Encoding.UTF8);
            var pageData = JsonSerializer.Deserialize<JsonElement>(json);
            
            pages.Add(new
            {
                Id = pageData.GetProperty("Id").GetString(),
                Title = pageData.GetProperty("Title").GetString(),
                UpdatedAt = pageData.GetProperty("UpdatedAt").GetDateTime()
            });
        }

        return Json(new { success = true, pages = pages.OrderByDescending(p => ((dynamic)p).UpdatedAt) });
    }

    // 刪除頁面
    [HttpDelete]
    public IActionResult DeletePage(string id)
    {
        var storagePath = Path.Combine(_env.ContentRootPath, StorageRoot);
        var filePath = Path.Combine(storagePath, $"{id}.json");

        if (!System.IO.File.Exists(filePath))
        {
            return Json(new { success = false, error = "頁面不存在" });
        }

        System.IO.File.Delete(filePath);

        return Json(new { success = true });
    }
}

// Request Models
public class SavePageRequest
{
    public string? pageId { get; set; }
    public string? title { get; set; }
    public string? html { get; set; }
    public string? css { get; set; }
    public string? gjsData { get; set; }
}


