using HNB.IIS.Core.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HNB.IIS.Core.Utilities;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LogErrorAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        using var scope = context.HttpContext.RequestServices.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ErrorLogService>();
        var success = logger.SaveError(context.HttpContext, context.Exception, "Filter", 1);
        
        if (!success)
        {
            var fallback = context.HttpContext.RequestServices.GetRequiredService<ILogger<LogErrorAttribute>>();
            fallback.LogError("Filter logging failed");
        }

        context.Result = new JsonResult(new { error = "系統錯誤" }) { StatusCode = 500 };
        context.ExceptionHandled = true;
    }
}

