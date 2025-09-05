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
        ViewData["Breadcrumb"] = svc.BuildBreadcrumb(vPath);                 // List<(string Name,string VirtualPath)>
        ViewData["Tree"] = svc.BuildTree();                                  // List<(string Name,string VirtualPath,int Depth)>
        ViewData["Folders"] = svc.ListFolders(vPath);                        // List<(string Name,DateTime? LastWriteUtc)>
        ViewData["Files"] = svc.ListFiles(vPath);                            // List<(string Name,long Size,DateTime? LastWriteUtc)>
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(200_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([Required] IFormFile file, string? path = "/")
    {
        var vPath = svc.NormalizePath(path ?? "/");
        try
        {
            await svc.UploadAsync(vPath, file);
            return OkOrRedirect(vPath);
        }
        catch (Exception ex) { return ErrorOrRedirect(vPath, ex); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(2_000_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMany([Required] IReadOnlyList<IFormFile> files, string? path = "/")
    {
        var vPath = svc.NormalizePath(path ?? "/");
        try
        {
            await svc.UploadManyAsync(vPath, files);
            return OkOrRedirect(vPath, new { count = files?.Count ?? 0 });
        }
        catch (Exception ex) { return ErrorOrRedirect(vPath, ex); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateFolder([Required] string folderName, string? path = "/")
    {
        var vPath = svc.NormalizePath(path ?? "/");
        try
        {
            svc.CreateFolder(vPath, folderName);
            return OkOrRedirect(vPath, new { name = folderName });
        }
        catch (Exception ex) { return ErrorOrRedirect(vPath, ex); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateEmptyFile([Required] string fileName, string? path = "/")
    {
        var vPath = svc.NormalizePath(path ?? "/");
        try
        {
            svc.CreateEmptyFile(vPath, fileName);
            return OkOrRedirect(vPath, new { name = fileName });
        }
        catch (Exception ex) { return ErrorOrRedirect(vPath, ex); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteFile([Required] string name, string? path = "/")
    {
        var vPath = svc.NormalizePath(path ?? "/");
        try
        {
            svc.DeleteFile(vPath, name);
            return OkOrRedirect(vPath, new { name });
        }
        catch (Exception ex) { return ErrorOrRedirect(vPath, ex); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteFolder([Required] string name, string? path = "/")
    {
        var vPath = svc.NormalizePath(path ?? "/");
        try
        {
            svc.DeleteFolder(vPath, name);
            return OkOrRedirect(vPath, new { name });
        }
        catch (Exception ex) { return ErrorOrRedirect(vPath, ex); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RenameFile([Required] string oldName, [Required] string newName, string? path = "/")
    {
        var vPath = svc.NormalizePath(path ?? "/");
        try
        {
            svc.RenameFile(vPath, oldName, newName);
            return OkOrRedirect(vPath, new { oldName, newName });
        }
        catch (Exception ex) { return ErrorOrRedirect(vPath, ex); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RenameFolder([Required] string oldName, [Required] string newName, string? path = "/")
    {
        var vPath = svc.NormalizePath(path ?? "/");
        try
        {
            svc.RenameFolder(vPath, oldName, newName);
            return OkOrRedirect(vPath, new { oldName, newName });
        }
        catch (Exception ex) { return ErrorOrRedirect(vPath, ex); }
    }

    [HttpGet]
    public IActionResult Download([Required] string name, string? path = "/")
    {
        var vPath = svc.NormalizePath(path ?? "/");
        var (stream, fileName, contentType) = svc.OpenRead(vPath, name);
        return File(stream, contentType, fileName);
    }

    #endregion

    #region 私有方法
    private bool IsAjaxRequest()
    {
        var xrw = Request?.Headers["X-Requested-With"].ToString();
        if (!string.IsNullOrEmpty(xrw) && xrw.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase)) return true;
        var accept = Request?.Headers["Accept"].ToString() ?? "";
        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    private IActionResult OkOrRedirect(string path, object? payload = null)
    {
        if (IsAjaxRequest()) return Json(new { ok = true, path, data = payload });
        return RedirectToAction(nameof(FileManager), new { path });
    }

    private IActionResult ErrorOrRedirect(string path, Exception ex)
    {
        if (IsAjaxRequest()) return BadRequest(new { ok = false, path, error = ex.Message });
        TempData["Error"] = ex.Message;
        return RedirectToAction(nameof(FileManager), new { path });
    }
    #endregion

}
