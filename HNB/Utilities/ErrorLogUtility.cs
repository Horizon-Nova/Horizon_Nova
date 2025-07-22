using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using HNB.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HNB.Utilities
{
    /// <summary>
    /// 當 Controller 或 Action 發生未處理例外時，自動記錄錯誤資訊到資料庫
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LogErrorAttribute : ExceptionFilterAttribute
    {
        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            var ex = context.Exception;
            var fullText = ex.ToString();            // 包含 inner exception

            try
            {
                await using var scope = context.HttpContext.RequestServices.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<HnbdataContext>();

                db.ErrorLogs.Add(new ErrorLog
                {
                    Function = actionName,
                    Message = fullText.Length > 4000 ? fullText[..4000] : fullText,
                });
                await db.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<LogErrorAttribute>>();
                logger.LogError(logEx, "寫入 ErrorLogs 失敗");
            }

            // 回傳統一錯誤
            context.Result = new JsonResult(new { error = "系統發生錯誤，請稍後再試。" })
            { StatusCode = StatusCodes.Status500InternalServerError };

            context.ExceptionHandled = true;
        }
    }

}
