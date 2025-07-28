using System;
using System.Collections.Generic;

namespace HNB.Models;

public partial class ErrorLog
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 層級代碼：0=Middleware, 1=Filter, 2=ExceptionHandler, 3=Background
    /// </summary>
    public short Stage { get; set; }

    /// <summary>
    /// 層級名稱：Middleware / Filter / ExceptionHandler / Background
    /// </summary>
    public string Layer { get; set; } = null!;

    /// <summary>
    /// 簡易函數描述（例如 Controller/Action）
    /// </summary>
    public string Function { get; set; } = null!;

    /// <summary>
    /// 方法全名，例如 Namespace.Controller.Action()
    /// </summary>
    public string? FunctionFull { get; set; }

    /// <summary>
    /// 錯誤訊息（完整 ToString）
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// 堆疊追蹤
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// 請求路徑（Request.Path）
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// HTTP 方法（GET/POST/PUT/...）
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// 回應狀態碼（500/404/...）
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// 使用者 ID（HttpContext.User.Identity.Name）
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 請求唯一識別碼（HttpContext.TraceIdentifier）
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// 附加資料（如 headers, query, ip, user-agent）
    /// </summary>
    public string? Extra { get; set; }

    /// <summary>
    /// 紀錄建立時間
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
