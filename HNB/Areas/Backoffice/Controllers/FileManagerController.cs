using HNB.Areas.Backoffice.Dtos;
using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class FileManagerController(FileManagerServices svc) : BaseController
{
    #region 主頁面
    /// <summary>
    /// 檔案管理主頁面
    /// </summary>
    public IActionResult FileManager(string path = "/")
    {
        ViewBag.CurrentPath = path ?? "/";

        var allData = svc.LoadFileSystemItems(path ?? "/");
        ViewBag.Items = allData;
        
        ViewBag.Tree = svc.LoadTree();

        return View();
    }
    #endregion

    #region 動態載入部分視圖
    /// <summary>
    /// 載入檔案/資料夾詳細資料部分視圖
    /// </summary>
    [HttpGet]
    public IActionResult LoadDetail(string path, string name)
    {
        // 直接從檔案系統讀取單一項目
        var items = svc.LoadFileSystemItems(path);
        var item = items.FirstOrDefault(i => i.Name == name);

        if (item == null)
        {
            return Content("<div class=\"p-6\">找不到項目</div>", "text/html; charset=utf-8");
        }

        return PartialView("_FileManagerDetail", item);
    }
    #endregion

    #region 資料夾操作 API
    /// <summary>
    /// 建立資料夾
    /// </summary>
    [HttpPost]
    public IActionResult CreateFolder([FromBody] CreateItemRequest request)
    {
        svc.CreateFolder(request.Path, request.Name);
        return Json(new FileManagerResponse { Success = true, Message = "資料夾已建立" });
    }

    /// <summary>
    /// 刪除資料夾
    /// </summary>
    [HttpPost]
    public IActionResult DeleteFolder([FromBody] DeleteItemRequest request)
    {
        svc.DeleteFolder(request.Path, request.Name);
        return Json(new FileManagerResponse { Success = true, Message = "資料夾已刪除" });
    }

    /// <summary>
    /// 重新命名資料夾
    /// </summary>
    [HttpPost]
    public IActionResult RenameFolder([FromBody] RenameItemRequest request)
    {
        svc.RenameFolder(request.Path, request.OldName, request.NewName);
        return Json(new FileManagerResponse { Success = true, Message = "資料夾已重新命名" });
    }
    #endregion

    #region 檔案操作 API
    /// <summary>
    /// 建立檔案
    /// </summary>
    [HttpPost]
    public IActionResult CreateFile([FromBody] CreateItemRequest request)
    {
        svc.CreateFile(request.Path, request.Name);
        return Json(new FileManagerResponse { Success = true, Message = "檔案已建立" });
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    [HttpPost]
    public IActionResult DeleteFile([FromBody] DeleteItemRequest request)
    {
        svc.DeleteFile(request.Path, request.Name);
        return Json(new FileManagerResponse { Success = true, Message = "檔案已刪除" });
    }

    /// <summary>
    /// 重新命名檔案
    /// </summary>
    [HttpPost]
    public IActionResult RenameFile([FromBody] RenameItemRequest request)
    {
        svc.RenameFile(request.Path, request.OldName, request.NewName);
        return Json(new FileManagerResponse { Success = true, Message = "檔案已重新命名" });
    }

    /// <summary>
    /// 讀取文字檔案
    /// </summary>
    [HttpGet]
    public IActionResult ReadTextFile(string path, string name)
    {
        var (content, encoding, lastModified) = svc.LoadTextFile(path, name);
        return Json(new ReadTextFileResponse 
        { 
            Success = true, 
            Content = content, 
            Encoding = encoding, 
            LastModified = lastModified 
        });
    }

    /// <summary>
    /// 儲存文字檔案
    /// </summary>
    [HttpPost]
    public IActionResult SaveTextFile([FromBody] SaveTextFileRequest request)
    {
        svc.SaveTextFile(request.Path, request.Name, request.Content, request.Encoding);
        return Json(new FileManagerResponse { Success = true, Message = "檔案已儲存" });
    }
    #endregion

    #region 檔案上傳
    /// <summary>
    /// 上傳檔案（最大 4GB）
    /// </summary>
    [HttpPost,DisableRequestSizeLimit,RequestFormLimits(MultipartBodyLengthLimit = 4294967296),RequestSizeLimit(4294967296)] // 4GB
    public IActionResult Upload(string path, List<IFormFile> files, List<string>? keys)
    {
        if (files == null || files.Count == 0)
            return Json(new FileManagerResponse { Success = false, Message = "未選擇檔案" });

        var result = svc.UploadBatchFiles(path, files, keys);
        
        return Json(new UploadResponse
        {
            Success = result.success,
            Saved = result.savedCount,
            Failed = result.failedCount,
            Errors = result.errors,
            Message = result.success 
                ? $"成功上傳 {result.savedCount} 個檔案" 
                : $"上傳完成：{result.savedCount} 成功，{result.failedCount} 失敗"
        });
    }
    #endregion

    #region 檔案下載
    /// <summary>
    /// 下載檔案
    /// </summary>
    [HttpGet]
    public IActionResult Download(string path, string name)
    {
        var (stream, fileName, contentType) = svc.DownloadFile(path, name);
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// 下載資料夾（ZIP）
    /// </summary>
    [HttpGet]
    public IActionResult DownloadFolder(string path, string name)
    {
        var (stream, fileName, contentType) = svc.DownloadFolderAsZip(path, name);
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// 預覽檔案
    /// </summary>
    [HttpGet]
    public IActionResult Preview(string path, string name)
    {
        var (stream, contentType) = svc.PreviewFile(path, name);
        Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{Uri.EscapeDataString(name)}";
        return File(stream, contentType, enableRangeProcessing: true);
    }
    #endregion
}

