using HNB.IIS.Core.Filters;
using HNB.IIS.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.IIS.Core.Controllers;

/// <summary>
/// 錯誤日誌管理控制器
/// </summary>
[PermissionFilter("ErrorLog")]
public class ErrorLogController(ErrorLogService errorLogService) : Controller
{
    /// <summary>
    /// 錯誤日誌列表頁面
    /// </summary>
    public IActionResult Index()
    {
        var logs = errorLogService.GetErrorLogs(100);
        ViewBag.TotalCount = errorLogService.GetErrorLogCount();
        return View(logs);
    }

    /// <summary>
    /// 測試錯誤 - 拋出未處理的異常（僅供測試）
    /// </summary>
    public IActionResult TestException()
    {
        throw new InvalidOperationException("這是一個測試異常，用於驗證錯誤捕捉系統是否正常運作。");
    }

    /// <summary>
    /// 測試不同類型的錯誤
    /// </summary>
    public IActionResult TestErrorType(string type)
    {
        switch (type.ToLower())
        {
            case "null":
                throw new ArgumentNullException("testParameter", "測試 ArgumentNullException");
            
            case "notfound":
                throw new KeyNotFoundException("測試 KeyNotFoundException - 找不到指定的資源");
            
            case "unauthorized":
                throw new UnauthorizedAccessException("測試 UnauthorizedAccessException - 未授權存取");
            
            case "argument":
                throw new ArgumentException("測試 ArgumentException - 無效的參數");
            
            case "divide":
                var zero = 0;
                var result = 100 / zero; // 除以零
                return Ok(result);
            
            case "outofrange":
                var array = new int[5];
                var value = array[10]; // 陣列越界
                return Ok(value);
            
            case "nested":
                try
                {
                    try
                    {
                        throw new InvalidOperationException("Level 3 - 最內層異常");
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException("Level 2 - 中間層異常", ex);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Level 1 - 外層異常", ex);
                }
            
            default:
                throw new NotImplementedException($"未知的測試類型: {type}");
        }
    }

    /// <summary>
    /// 測試正常回應（不應產生錯誤日誌）
    /// </summary>
    public IActionResult TestNormal()
    {
        return Json(new
        {
            success = true,
            message = "正常執行，沒有錯誤",
            timestamp = DateTime.Now
        });
    }
}

