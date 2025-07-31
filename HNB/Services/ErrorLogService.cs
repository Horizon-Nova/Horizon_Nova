using HNB.Models;
using HNB.Repositories;
using HNB.Helpers;

namespace HNB.Services;

public class ErrorLogService
{
    private readonly ErrorLogRepository _repo;

    public ErrorLogService(ErrorLogRepository repo)
    {
        _repo = repo;
    }

    public async Task SaveAsync(HttpContext context, Exception ex, string layer, short stage)
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

        var log = new ErrorLog
        {
            Stage = stage,
            Layer = LogSanitizer.Clean(layer),
            Function = LogSanitizer.Clean(context.Request.Path),
            FunctionFull = LogSanitizer.Clean(functionFull),
            Message = LogSanitizer.Clean(ex.Message),
            StackTrace = LogSanitizer.Clean(ex.StackTrace),
            Path = LogSanitizer.Clean(context.Request.Path),
            HttpMethod = LogSanitizer.Clean(context.Request.Method),
            StatusCode = context.Response?.StatusCode,
            UserId = LogSanitizer.Clean(context.User.Identity?.Name ?? "Anonymous"),
            TraceId = LogSanitizer.Clean(context.TraceIdentifier),
            Extra = null,
        };

        await _repo.InsertAsync(log);
    }

}
