using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class DesignerController : Controller
{
    private const string GeneratedViewsRoot = "Areas/Backoffice/Views/Generated";
    private const string GeneratedCssRoot = "wwwroot/generated";

    [HttpGet]
    public IActionResult Index(string? name = null)
    {
        ViewBag.Name = name ?? "NewPage";
        return View();
    }

    public record SavePayload(string name, string? html, string? css, string? layout);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Save([FromBody] SavePayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload?.name))
            return Json(new { success = false, message = "Name is required." });

        Directory.CreateDirectory(GeneratedViewsRoot);
        Directory.CreateDirectory(GeneratedCssRoot);

        var safeName = GetSafeFileName(payload.name);
        var viewPath = Path.Combine(GeneratedViewsRoot, safeName + ".cshtml");
        var cssFileName = safeName + ".css";
        var cssPath = Path.Combine(GeneratedCssRoot, cssFileName);

        // Write CSS
        if (!string.IsNullOrEmpty(payload.css))
        {
            System.IO.File.WriteAllText(cssPath, payload.css, Encoding.UTF8);
        }

        // Compose cshtml
        var builder = new StringBuilder();
        builder.AppendLine("@{ Layout = \"_Layout\"; }");
        if (!string.IsNullOrEmpty(payload.css))
        {
            builder.AppendLine($"<link rel=\"stylesheet\" href=\"/generated/{cssFileName}\" />");
        }
        builder.AppendLine(payload.html ?? string.Empty);

        System.IO.File.WriteAllText(viewPath, builder.ToString(), Encoding.UTF8);

        return Json(new { success = true, message = "Saved", view = $"/Backoffice/Designer/Preview/{safeName}" });
    }

    [HttpGet]
    public IActionResult Preview(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return NotFound();
        var safeName = GetSafeFileName(name);
        var viewVirtualPath = $"~/Areas/Backoffice/Views/Generated/{safeName}.cshtml";
        return View(viewVirtualPath);
    }

    private static string GetSafeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "GeneratedPage" : cleaned;
    }
}


