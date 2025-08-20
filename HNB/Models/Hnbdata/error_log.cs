using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

[Table("error_logs", Schema = "dbo")]
public partial class error_log
{
    /// <summary>
    /// 主鍵
    /// </summary>
    [Key]
    public int id { get; set; }

    /// <summary>
    /// 層級代碼：0=Middleware, 1=Filter, 2=ExceptionHandler, 3=Background
    /// </summary>
    public short stage { get; set; }

    /// <summary>
    /// 層級名稱：Middleware / Filter / ExceptionHandler / Background
    /// </summary>
    [StringLength(50)]
    public string layer { get; set; } = null!;

    /// <summary>
    /// 簡易函數描述（例如 Controller/Action）
    /// </summary>
    [StringLength(255)]
    public string function { get; set; } = null!;

    /// <summary>
    /// 方法全名，例如 Namespace.Controller.Action()
    /// </summary>
    public string? function_full { get; set; }

    /// <summary>
    /// 錯誤訊息（完整 ToString）
    /// </summary>
    public string message { get; set; } = null!;

    /// <summary>
    /// 堆疊追蹤
    /// </summary>
    public string? stack_trace { get; set; }

    /// <summary>
    /// 請求路徑（Request.Path）
    /// </summary>
    public string? path { get; set; }

    /// <summary>
    /// HTTP 方法（GET/POST/PUT/...）
    /// </summary>
    [StringLength(10)]
    public string? http_method { get; set; }

    /// <summary>
    /// 回應狀態碼（500/404/...）
    /// </summary>
    public int? status_code { get; set; }

    /// <summary>
    /// 使用者 ID（HttpContext.User.Identity.Name）
    /// </summary>
    [StringLength(64)]
    public string? user_id { get; set; }

    /// <summary>
    /// 請求唯一識別碼（HttpContext.TraceIdentifier）
    /// </summary>
    [StringLength(100)]
    public string? trace_id { get; set; }

    /// <summary>
    /// 附加資料（如 headers, query, ip, user-agent）
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? extra { get; set; }

    /// <summary>
    /// 紀錄建立時間
    /// </summary>
    public DateTime? created_at { get; set; }
}
