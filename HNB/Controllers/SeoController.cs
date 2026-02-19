using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Controllers;

/// <summary>
/// SEO 相關輸出（robots / sitemap）
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class SeoController : Controller
{
    /// <summary>
    /// robots.txt（依目前網域動態輸出，避免環境切換造成 Sitemap 網域不一致）
    /// </summary>
    [HttpGet("/robots.txt")]
    public IActionResult Robots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var robotsText =
$@"User-agent: *
Allow: /

# 後台/內部功能不開放索引
Disallow: /Backoffice/
Disallow: /MISSA/
Disallow: /storage/

Sitemap: {baseUrl}/sitemap.xml
";

        return Content(robotsText, "text/plain", Encoding.UTF8);
    }

    /// <summary>
    /// sitemap.xml（依目前網域動態輸出，列出主要公開頁面）
    /// </summary>
    [HttpGet("/sitemap.xml")]
    public IActionResult Sitemap()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var urlPaths = new[]
        {
            "/",
            "/HNB_WEB/NovaHome",
            "/HNB_WEB/Consultation"
        };

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var urlSet = new XElement(ns + "urlset",
            urlPaths.Select(path =>
                new XElement(ns + "url",
                    new XElement(ns + "loc", $"{baseUrl}{path}")
                )
            )
        );

        var document = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            urlSet
        );

        return Content(document.ToString(SaveOptions.DisableFormatting), "application/xml", Encoding.UTF8);
    }
}

