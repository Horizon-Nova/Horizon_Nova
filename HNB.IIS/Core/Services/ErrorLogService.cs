using HNB.IIS.Core.Helpers;
using HNB.IIS.Core.Models.Hnbdata;
using HNB.IIS.Core.Repositories;
using Microsoft.AspNetCore.Http;

namespace HNB.IIS.Core.Services;

public class ErrorLogService(ErrorLogRepository repo)
{
    public bool SaveError(HttpContext context, Exception ex, string layer, short stage)
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
            layer = LogSanitizer.Clean(layer) ?? "",
            function = LogSanitizer.Clean(context.Request.Path) ?? "",
            function_full = LogSanitizer.Clean(functionFull),
            message = LogSanitizer.Clean(ex.Message) ?? "",
            stack_trace = LogSanitizer.Clean(ex.StackTrace),
            path = LogSanitizer.Clean(context.Request.Path),
            http_method = LogSanitizer.Clean(context.Request.Method),
            status_code = context.Response?.StatusCode,
            user_id = LogSanitizer.Clean(context.User.Identity?.Name ?? "Anonymous"),
            trace_id = LogSanitizer.Clean(context.TraceIdentifier),
            extra = null,
        };

        return repo.InsertErrorLog(log);
    }

    public List<error_log> LoadErrorLogList(int count = 50) => repo.QueryErrorLogList(count);
}

