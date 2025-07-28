using HNB.Models;
using HNB.Repositories;

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
            Layer = layer,
            Function = function,
            FunctionFull = functionFull,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Path = context.Request.Path,
            HttpMethod = context.Request.Method,
            StatusCode = context.Response?.StatusCode,
            UserId = context.User.Identity?.Name ?? "Anonymous",
            TraceId = context.TraceIdentifier,
            Extra = null,
        };

        await _repo.InsertAsync(log);
    }
}
