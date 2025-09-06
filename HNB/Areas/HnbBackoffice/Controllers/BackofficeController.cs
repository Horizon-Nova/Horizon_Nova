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

    // ========== 小幫手：集中處理 Path 與錯誤回應 ==========
    #region Helpers
    private string V(string? path) => svc.NormalizePath(path ?? "/");

    private IActionResult TryDo(string? path, Func<string, IActionResult> action)
    {
        var v = V(path);
        try { return action(v); }
        catch (Exception ex) { return ErrorOrRedirect(v, ex); }
    }

    private async Task<IActionResult> TryDoAsync(string? path, Func<string, Task> action, object? okPayload = null)
    {
        var v = V(path);
        try { await action(v); return OkOrRedirect(v, okPayload); }
        catch (Exception ex) { return ErrorOrRedirect(v, ex); }
    }
    #endregion

    // ========== 畫面 ==========
    public IActionResult FileManager(string? path = "/")
    {
        var vPath = V(path);
        ViewData["CurrentPath"] = vPath;
        ViewData["CanAddHere"] = svc.CanAddHere(vPath);
        ViewData["Breadcrumb"] = svc.BuildBreadcrumb(vPath);                 // List<(string Name,string VirtualPath)>
        ViewData["Tree"] = svc.BuildTree();                                  // List<(string Name,string VirtualPath,int Depth)>
        ViewData["Folders"] = svc.ListFolders(vPath);                        // List<(string Name,DateTime? LastWriteUtc)>
        ViewData["Files"] = svc.ListFiles(vPath);                            // List<(string Name,long Size,DateTime? LastWriteUtc)>
        return View();
    }

    // ========== 上傳 ==========
    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(200_000_000)]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> Upload([Required] IFormFile file, string? path = "/")
        => TryDoAsync(path, v => svc.UploadAsync(v, file));

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(2_000_000_000)]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> UploadMany([Required] IReadOnlyList<IFormFile> files, string? path = "/")
        => TryDoAsync(path, v => svc.UploadManyAsync(v, files), new { count = files?.Count ?? 0 });

    // ========== 建立 ==========
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateFolder([Required] string folderName, string? path = "/")
        => TryDo(path, v => { svc.CreateFolder(v, folderName); return OkOrRedirect(v, new { name = folderName }); });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateEmptyFile([Required] string fileName, string? path = "/")
        => TryDo(path, v => { svc.CreateEmptyFile(v, fileName); return OkOrRedirect(v, new { name = fileName }); });

    // ========== 刪除 ==========
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteFile([Required] string name, string? path = "/")
        => TryDo(path, v => { svc.DeleteFile(v, name); return OkOrRedirect(v, new { name }); });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteFolder([Required] string name, string? path = "/")
        => TryDo(path, v => { svc.DeleteFolder(v, name); return OkOrRedirect(v, new { name }); });

    // ========== 重新命名 ==========
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RenameFile([Required] string oldName, [Required] string newName, string? path = "/")
        => TryDo(path, v => { svc.RenameFile(v, oldName, newName); return OkOrRedirect(v, new { oldName, newName }); });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RenameFolder([Required] string oldName, [Required] string newName, string? path = "/")
        => TryDo(path, v => { svc.RenameFolder(v, oldName, newName); return OkOrRedirect(v, new { oldName, newName }); });

    // ========== 下載 / 預覽 ==========
    [HttpGet]
    public IActionResult Download([Required] string name, string? path = "/")
    {
        var v = V(path);
        var (stream, fileName, contentType) = svc.OpenRead(v, name);
        return File(stream, contentType, fileName);
    }

    // 資料夾下載 ZIP
    [HttpGet]
    public IActionResult DownloadFolderZip([Required] string name, string? path = "/")
        => TryDo(path, v => {
            var (stream, fileName, ct) = svc.ZipFolder(v, name);
            return File(stream, ct, fileName);
        });

    // 原檔 inline（預覽用：<img src> / <video src> ...）
    [HttpGet]
    public IActionResult Raw([Required] string name, string? path = "/")
    {
        var v = V(path);
        var (stream, ct) = svc.OpenRaw(v, name);
        return File(stream, ct); // 不帶檔名 => inline
    }

    // ========== 文字檔 ==========
    [HttpGet]
    public IActionResult ReadText([Required] string name, string? path = "/")
        => TryDo(path, v => {
            var (content, encoding, last) = svc.ReadTextFile(v, name);
            return Json(new { ok = true, content, encoding, lastWriteUtc = last });
        });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SaveText([Required] string name, [Required] string content, string? encoding = "utf-8", string? path = "/")
        => TryDo(path, v => { svc.SaveTextFile(v, name, content, encoding); return OkOrRedirect(v, new { name }); });

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
