using HNB.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace HNB.Filters
{
    public class ExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionLoggingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context, RailwayContext db, ILogger<ExceptionLoggingMiddleware> logger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var endpoint = context.GetEndpoint();
                string function = context.Request.Path;

                if (endpoint != null)
                {
                    var descriptor = endpoint.Metadata
                        .OfType<ControllerActionDescriptor>()
                        .FirstOrDefault();

                    if (descriptor != null)
                    {
                        function = $"{descriptor.ControllerTypeInfo.FullName}.{descriptor.ActionName}";
                    }
                }

                /* -------- 嘗試寫入 ErrorLogs -------- */
                try
                {
                    string message = (ex.Message?.Contains("timeout", StringComparison.OrdinalIgnoreCase) == true ||
                                      ex.Message?.Contains("error", StringComparison.OrdinalIgnoreCase) == true)
                                     ? null : ex.Message;

                    string stackTrace = (message == null) ? null : ex.StackTrace;

                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Function = function,
                        Message = message,
                        StackTrace = stackTrace,
                        CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                    });

                    await db.SaveChangesAsync();
                }
                catch (Exception logEx)
                {
                    logger.LogError(logEx, "寫入 ErrorLogs 失敗");
                }

                /* -------- 輸出統一的錯誤回應 -------- */
                logger.LogError(ex, "[GLOBAL] 未處理例外");
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    var taiwanTz = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
                    var taiwanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, taiwanTz);

                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "系統發生錯誤",
                        timestamp = taiwanTime.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
                else
                {
                    logger.LogWarning("Response 已開始輸出，無法再寫入錯誤訊息至 HTTP Response。");
                }
            }
        }
    }
}
