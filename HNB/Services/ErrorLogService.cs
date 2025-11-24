using HNB.Helpers;
using HNB.Repositories;
using Models.Hnb;

namespace HNB.Services;

/// <summary>
/// 錯誤日誌服務層，負責處理錯誤日誌的業務邏輯
/// </summary>
public class ErrorLogService(ErrorLogRepository rep)
{
    /// <summary>
    /// 儲存錯誤日誌到資料庫
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <param name="ex">例外物件</param>
    /// <param name="layer">發生錯誤的層級</param>
    /// <param name="stage">錯誤階段</param>
    /// <returns>成功返回 true，失敗返回 false</returns>
    public bool Save(HttpContext context, Exception ex, string layer, short stage)
    {
        var endpoint = context.GetEndpoint();
        string function = context.Request.Path;
        string? functionFull = null;

        if (endpoint != null)
        {
            var descriptor = endpoint.Metadata
                .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
                .FirstOrDefault();

            if (descriptor != null)
            {
                function = descriptor.ActionName;
                functionFull = $"{descriptor.ControllerTypeInfo.FullName}.{descriptor.ActionName}";
            }
        }

        var log = new error_log
        {
            stage = stage,
            layer = LogSanitizer.Clean(layer),
            function = LogSanitizer.Clean(context.Request.Path),
            function_full = LogSanitizer.Clean(functionFull),
            message = LogSanitizer.Clean(ex.Message),
            stack_trace = LogSanitizer.Clean(ex.StackTrace),
            path = LogSanitizer.Clean(context.Request.Path),
            http_method = LogSanitizer.Clean(context.Request.Method),
            status_code = context.Response?.StatusCode,
            user_id = LogSanitizer.Clean(context.User.Identity?.Name ?? "Anonymous"),
            trace_id = LogSanitizer.Clean(context.TraceIdentifier),
            extra = null,
        };

        return rep.Insert(log);
    }
}

