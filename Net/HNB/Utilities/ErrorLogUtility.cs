using HNB.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Utilities;

/// <summary>
/// 記錄例外錯誤的過濾器屬性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LogErrorAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        using var scope = context.HttpContext.RequestServices.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ErrorLogService>();
        var success = logger.Save(context.HttpContext, context.Exception, "Filter", 1);
        
        if (!success)
        {
            var fallback = context.HttpContext.RequestServices.GetRequiredService<ILogger<LogErrorAttribute>>();
            fallback.LogError("Filter logging failed");
        }

        context.Result = new JsonResult(new { error = "系統錯誤" }) { StatusCode = 500 };
        context.ExceptionHandled = true;
    }
}
