using HNB.Services;

namespace HNB.Middleware;

public class ExceptionLoggingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, ErrorLogService loggerService, ILogger<ExceptionLoggingMiddleware> logger)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            try
            {
                loggerService.Save(context, ex, "Middleware", 0);
            }
            catch (Exception logEx)
            {
                logger.LogError(logEx, "Middleware logging failed");
            }

            throw;
        }
    }
}
