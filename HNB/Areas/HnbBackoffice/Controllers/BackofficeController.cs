using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;
using System.ComponentModel.DataAnnotations;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class BackofficeController(DbKeyJwtService DBsvc, BackofficeService svc, IConfiguration cfg) : Controller
{
    #region Dashboard (儀錶板)
    public IActionResult Dashboard()
        => View();
    #endregion

    #region Login/Logout (登入/登出)
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if (req?.Username == "admin" && req?.Password == "123456")
        {
            var (token, exp) = await DBsvc.IssueTokenAfterLoginAsync(
                ctx: HttpContext,
                keyComponents: req.Username,
                note: "登入產生",
                ct: ct);

            return Ok(new { ok = true, token, expires_at = exp });
        }

        return Unauthorized(new { ok = false, error = "invalid_credential" });
    }

    public record LoginRequest(string Username, string Password);
    #endregion

    #region FileManager (檔案總管) 

    public IActionResult FileManager(string? path = "/")
    {
        var vPath = svc.NormalizePath(path);
        ViewData["CurrentPath"] = vPath;
        ViewData["CanAddHere"] = svc.CanAddHere(vPath);
        ViewData["Breadcrumb"] = svc.BuildBreadcrumb(vPath);                          // List<(string Name, string VirtualPath)>
        ViewData["Tree"] = svc.BuildTree();                                     // List<(string Name, string VirtualPath, int Depth)>
        ViewData["Folders"] = svc.ListFolders(vPath);                              // List<(string Name, DateTime? LastWriteUtc)>
        ViewData["Files"] = svc.ListFiles(vPath);                                // List<(string Name, long Size, DateTime? LastWriteUtc)>
        return View();
    }

    [HttpPost, RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> Upload([Required] IFormFile file, string? path = "/")
    {
        await svc.UploadAsync(path ?? "/", file);
        return RedirectToAction(nameof(FileManager), new { path });
    }

    [HttpPost]
    public IActionResult CreateFolder([Required] string folderName, string? path = "/")
    {
        svc.CreateFolder(path ?? "/", folderName);
        return RedirectToAction(nameof(FileManager), new { path });
    }

    [HttpPost]
    public IActionResult CreateEmptyFile([Required] string fileName, string? path = "/")
    {
        svc.CreateEmptyFile(path ?? "/", fileName);
        return RedirectToAction(nameof(FileManager), new { path });
    }

    [HttpPost]
    public IActionResult DeleteFile([Required] string name, string? path = "/")
    {
        svc.DeleteFile(path ?? "/", name);
        return RedirectToAction(nameof(FileManager), new { path });
    }

    [HttpPost]
    public IActionResult DeleteFolder([Required] string name, string? path = "/")
    {
        svc.DeleteFolder(path ?? "/", name);
        return RedirectToAction(nameof(FileManager), new { path });
    }

    [HttpGet]
    public IActionResult Download([Required] string name, string? path = "/")
    {
        var (stream, fileName, contentType) = svc.OpenRead(path ?? "/", name);
        return File(stream, contentType, fileName);
    }

    #endregion

}
