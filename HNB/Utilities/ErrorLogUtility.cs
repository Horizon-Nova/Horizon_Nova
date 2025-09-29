using HNB.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Utilities;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LogErrorAttribute : ExceptionFilterAttribute
{
    public override async Task OnExceptionAsync(ExceptionContext context)
    {
        try
        {
            await using var scope = context.HttpContext.RequestServices.CreateAsyncScope();
            var logger = scope.ServiceProvider.GetRequiredService<ErrorLogService>();
            await logger.SaveAsync(context.HttpContext, context.Exception, "Filter", 1);
        }
        catch (Exception ex)
        {
            var fallback = context.HttpContext.RequestServices.GetRequiredService<ILogger<LogErrorAttribute>>();
            fallback.LogError(ex, "Filter logging failed");
        }

        context.Result = new JsonResult(new { error = "系統錯誤" }) { StatusCode = 500 };
        context.ExceptionHandled = true;
    }
}
