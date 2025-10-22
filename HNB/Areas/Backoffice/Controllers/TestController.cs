using HNB.Areas.Backoffice.Utilities;
using Microsoft.AspNetCore.Mvc;
using static HNB.Areas.Backoffice.Utilities.DirectoryManagerUtilities;

namespace HNB.Areas.Backoffice.Controllers;

/// <summary>
/// 測試控制器 - 集中管理所有測試功能
/// </summary>
[Area("Backoffice")]
public class TestController : Controller
{
    private readonly string _testStoragePath = @"D:\Private_project-GitHub-Horizon-Nova\Horizon_Nova\HNB\Areas\Backoffice\storage";

    /// <summary>
    /// 測試功能首頁 - 顯示所有可用的測試項目
    /// </summary>
    public IActionResult Index()
    {
        // 確保目錄存在
        if (!Directory.Exists(_testStoragePath))
        {
            Directory.CreateDirectory(_testStoragePath);
        }

        // 取得目錄中所有檔案
        var files = Directory.GetFiles(_testStoragePath)
            .Select(f =>
            {
                var owners = GetAppOwners(f);
                return new
                {
                    FileName = Path.GetFileName(f),
                    FullPath = f,
                    Size = new FileInfo(f).Length,
                    Owners = owners,
                    OwnersDisplay = GetAppOwnersDisplay(f),
                    CreatedAt = System.IO.File.GetCreationTime(f)
                };
            })
            .ToList();

        ViewBag.Files = files;
        ViewBag.StoragePath = _testStoragePath;
        return View();
    }

    /// <summary>
    /// 上傳檔案
    /// </summary>
    [HttpPost]
    public IActionResult UploadFile(IFormFile file, string newOwner)
    {
        try
        {
            // 除錯資訊
            Console.WriteLine($"[除錯] 收到上傳請求");
            Console.WriteLine($"[除錯] file is null: {file == null}");
            Console.WriteLine($"[除錯] newOwner: {newOwner ?? "null"}");
            Console.WriteLine($"[除錯] _testStoragePath: {_testStoragePath}");

            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "未選擇檔案" });

            Console.WriteLine($"[除錯] 檔案名稱: {file.FileName}, 大小: {file.Length}");

            // 確保目錄存在
            if (!Directory.Exists(_testStoragePath))
            {
                Console.WriteLine($"[除錯] 建立目錄: {_testStoragePath}");
                Directory.CreateDirectory(_testStoragePath);
            }

            // 儲存檔案
            var filePath = Path.Combine(_testStoragePath, file.FileName);
            Console.WriteLine($"[除錯] 準備儲存到: {filePath}");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            Console.WriteLine($"[除錯] 檔案已儲存");
            Console.WriteLine($"[除錯] User.Identity?.Name: {User.Identity?.Name ?? "null"}");
            Console.WriteLine($"[除錯] newOwner 參數: {newOwner ?? "null"}");

            // 決定應用程式擁有者：上傳者（登入用戶）永遠在第一位
            var uploader = User.Identity?.Name;
            List<string> additionalOwners = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(newOwner))
            {
                // 支援用逗號分隔多個擁有者
                var owners = newOwner.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(o => o.Trim())
                                     .Where(o => !string.IsNullOrWhiteSpace(o))
                                     .ToArray();
                additionalOwners.AddRange(owners);
            }

            Console.WriteLine($"[除錯] 上傳者: {uploader ?? "null"}");
            Console.WriteLine($"[除錯] 額外擁有者: {string.Join(", ", additionalOwners)}");

            // 設定應用程式擁有者（上傳者永遠在第一位）
            if (!string.IsNullOrWhiteSpace(uploader))
            {
                try
                {
                    UpsertAppOwnersWithActorFirst(filePath, uploader, additionalOwners.ToArray());
                    Console.WriteLine($"[除錯] 應用程式擁有者設定成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[除錯] 應用程式擁有者設定失敗: {ex.Message}");
                    return Json(new
                    {
                        success = false,
                        message = $"檔案已上傳，但設定擁有者失敗: {ex.Message}",
                        filePath = filePath
                    });
                }
            }
            else if (additionalOwners.Count > 0)
            {
                // 如果沒有登入用戶，但有指定其他擁有者
                try
                {
                    SetAppOwners(filePath, additionalOwners.ToArray());
                    Console.WriteLine($"[除錯] 應用程式擁有者設定成功（無登入用戶）");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[除錯] 應用程式擁有者設定失敗: {ex.Message}");
                }
            }

            // 取得最終擁有者
            var finalOwners = GetAppOwners(filePath);
            Console.WriteLine($"[除錯] 最終擁有者: {string.Join(", ", finalOwners)}");

            return Json(new
            {
                success = true,
                message = "檔案上傳成功",
                filePath = filePath,
                owners = finalOwners,
                ownersDisplay = GetAppOwnersDisplay(filePath)
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[除錯] 上傳失敗: {ex.Message}");
            Console.WriteLine($"[除錯] StackTrace: {ex.StackTrace}");
            return Json(new
            {
                success = false,
                message = $"上傳失敗: {ex.Message}",
                detail = ex.ToString()
            });
        }
    }

    /// <summary>
    /// 新增檔案擁有者
    /// </summary>
    [HttpPost]
    public IActionResult AddOwner(string fileName, string owner)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Json(new { success = false, message = "檔案名稱不可為空" });

        if (string.IsNullOrWhiteSpace(owner))
            return Json(new { success = false, message = "擁有者名稱不可為空" });

        try
        {
            var filePath = Path.Combine(_testStoragePath, fileName);

            if (!System.IO.File.Exists(filePath))
                return Json(new { success = false, message = "檔案不存在" });

            // 檢查是否已經是擁有者
            if (HasAppOwner(filePath, owner))
                return Json(new { success = false, message = "此擁有者已存在" });

            // 新增擁有者，新增的人永遠放第一位
            UpsertAppOwnersWithActorFirst(filePath, owner);
            var owners = GetAppOwners(filePath);

            return Json(new
            {
                success = true,
                message = "擁有者新增成功",
                owners = owners,
                ownersDisplay = GetAppOwnersDisplay(filePath)
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"新增失敗: {ex.Message}" });
        }
    }

    /// <summary>
    /// 移除檔案擁有者
    /// </summary>
    [HttpPost]
    public IActionResult RemoveOwner(string fileName, string owner)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Json(new { success = false, message = "檔案名稱不可為空" });

        if (string.IsNullOrWhiteSpace(owner))
            return Json(new { success = false, message = "擁有者名稱不可為空" });

        try
        {
            var filePath = Path.Combine(_testStoragePath, fileName);

            if (!System.IO.File.Exists(filePath))
                return Json(new { success = false, message = "檔案不存在" });

            RemoveAppOwner(filePath, owner);
            var owners = GetAppOwners(filePath);

            return Json(new
            {
                success = true,
                message = "擁有者移除成功",
                owners = owners,
                ownersDisplay = GetAppOwnersDisplay(filePath)
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"移除失敗: {ex.Message}" });
        }
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    [HttpPost]
    public IActionResult DeleteFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Json(new { success = false, message = "檔案名稱不可為空" });

        try
        {
            var filePath = Path.Combine(_testStoragePath, fileName);

            if (!System.IO.File.Exists(filePath))
                return Json(new { success = false, message = "檔案不存在" });

            System.IO.File.Delete(filePath);

            return Json(new { success = true, message = "檔案已刪除" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"刪除失敗: {ex.Message}" });
        }
    }

    /// <summary>
    /// 取得檔案擁有者列表
    /// </summary>
    [HttpGet]
    public IActionResult GetOwners(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Json(new { success = false, message = "檔案名稱不可為空" });

        try
        {
            var filePath = Path.Combine(_testStoragePath, fileName);

            if (!System.IO.File.Exists(filePath))
                return Json(new { success = false, message = "檔案不存在" });

            var owners = GetAppOwners(filePath);

            return Json(new
            {
                success = true,
                owners = owners,
                ownersDisplay = GetAppOwnersDisplay(filePath)
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"讀取失敗: {ex.Message}" });
        }
    }
}

