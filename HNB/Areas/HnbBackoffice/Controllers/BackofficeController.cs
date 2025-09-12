using HNB.Areas.HnbBackoffice.Filters;
using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Models.HnbHnbBackoffice;
using System.ComponentModel.DataAnnotations;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
[OperationPermission(requireIpMatch: true, verifyDb: true)]
public class BackofficeController(BackofficeService svc, IConfiguration cfg) : Controller
{
    #region Dashboard (儀錶板)
    public IActionResult Dashboard()
        => View();
    #endregion

    #region File Manager (檔案總管)
    public IActionResult FileManager() => View();

    /// <summary>檔案清單 Partial</summary>
    public IActionResult ListPartial(string path = "/")
    {
        var vm = svc.BuildFileListVm(path);
        return PartialView("_FileList", vm);
    }

    /// <summary> 目錄樹 Partial </summary>
    public IActionResult TreePartial(string? selectedPath = null)
    {
        var nodes = svc.BuildTree("/");
        ViewBag.SelectedPath = selectedPath ?? "/";
        return PartialView("_Tree", nodes);
    }

    /// <summary> 手動新增：資料夾 </summary>
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult CreateFolder([Required] string path, [Required, MaxLength(128)] string name)
    {
        var msg = svc.CreateFolder(path, name?.Trim() ?? "");
        var ok = string.IsNullOrEmpty(msg);
        return Json(new { ok, message = ok ? "資料夾已建立。" : msg });
    }

    /// <summary> 手動新增：檔案 </summary>
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult CreateFile([Required] string path, [Required, MaxLength(128)] string name)
    {
        var msg = svc.CreateEmptyFile(path, name?.Trim() ?? "");
        var ok = string.IsNullOrEmpty(msg);
        return Json(new { ok, message = ok ? "檔案已建立。" : msg });
    }

    /// <summary> 上傳（多檔單請求，前端停留頁面） </summary>
    [HttpPost, ValidateAntiForgeryToken, DisableRequestSizeLimit]
    public IActionResult Upload([Required] string path, [FromForm][Required] List<IFormFile> files, [FromForm] List<string>? keys)
    {
        var list = files ?? [];
        if (list.Count == 0) return Json(new { ok = false, message = "未選擇檔案" });

        keys ??= list.Select(f => f.FileName).ToList();
        if (keys.Count != list.Count) return Json(new { ok = false, message = "keys 與 files 數量不符" });

        var saved = 0;
        var errs = new List<object>();
        for (int i = 0; i < list.Count; i++)
        {
            var msg = svc.SaveUploadedFile(path, keys[i], list[i]);
            if (string.IsNullOrEmpty(msg)) saved++;
            else errs.Add(new { name = list[i].FileName, message = msg });
        }
        var ok = errs.Count == 0;
        return Json(new { ok, saved, failed = errs.Count, errors = errs });
    }

    /// <summary> 刪除檔案 </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteFile([Required] string path, [Required] string name)
    {
        var msg = svc.DeleteFile(path, name);
        var ok = string.IsNullOrEmpty(msg);
        return Json(new { ok, message = ok ? "檔案已刪除。" : msg });
    }

    /// <summary> 刪除資料夾 </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteFolder([Required] string path, [Required] string name)
    {
        var msg = svc.DeleteFolder(path, name);
        var ok = string.IsNullOrEmpty(msg);
        return Json(new { ok, message = ok ? "資料夾已刪除。" : msg });
    }

    /// <summary> 重新命名檔案 </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RenameFile([Required] string path, [Required] string oldName, [Required, MaxLength(255)] string newName)
    {
        var msg = svc.RenameFile(path, oldName, newName);
        var ok = string.IsNullOrEmpty(msg);
        return Json(new { ok, message = ok ? "檔案已重新命名。" : msg, newName });
    }

    /// <summary> 重新命名資料夾 </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RenameFolder([Required] string path, [Required] string oldName, [Required, MaxLength(255)] string newName)
    {
        var msg = svc.RenameFolder(path, oldName, newName);
        var ok = string.IsNullOrEmpty(msg);
        return Json(new { ok, message = ok ? "資料夾已重新命名。" : msg, newName });
    }

    /// <summary> 下載單檔 </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Download([Required] string path, [Required] string name)
    {
        var (abs, err, ct) = svc.ResolveFileForDownload(path, name);
        if (err != null) return BadRequest(err);

        return PhysicalFile(abs!, ct, fileDownloadName: name, enableRangeProcessing: true);
    }

    /// <summary> 下載資料夾 ZIP </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DownloadFolderZip([Required] string path, [Required] string name)
    {
        var (zipAbs, err, downloadName) = svc.BuildFolderZip(path, name);
        if (err != null) return BadRequest(err);

        var fs = new FileStream(zipAbs!, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, FileOptions.DeleteOnClose);
        return File(fs, "application/zip", fileDownloadName: downloadName, enableRangeProcessing: true);
    }

    /// <summary> Preview（檔案預覽，用於 <img>/<video>/<iframe>） </summary>
    [HttpGet]
    public IActionResult Preview([Required] string path, [Required] string name)
    {
        var (abs, err, ct) = svc.ResolveFileForDownload(path, name);
        if (err != null) return BadRequest(err);
        Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{Uri.EscapeDataString(name)}";
        return PhysicalFile(abs!, ct, enableRangeProcessing: true);
    }

    /// <summary> 讀取文字檔內容 </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ReadText([Required] string path, [Required] string name)
    {
        var (txt, err) = svc.ReadText(path, name);
        if (!string.IsNullOrEmpty(err)) return BadRequest(err);
        return Content(txt ?? "", "text/plain; charset=utf-8");
    }

    /// <summary> 寫入文字檔內容（覆寫） </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SaveText([Required] string path, [Required] string name, [Required] string content)
    {
        var err = svc.WriteText(path, name, content);
        var ok = string.IsNullOrEmpty(err);
        return Json(new { ok, message = ok ? "已儲存。" : err });
    }
    #endregion

}
