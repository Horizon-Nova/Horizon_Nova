using HNB.Services;

namespace HNB.Filters;

public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ErrorLogService loggerService, ILogger<ExceptionLoggingMiddleware> logger)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            try
            {
                await loggerService.SaveAsync(context, ex, "Middleware", 0);
            }
            catch (Exception logEx)
            {
                logger.LogError(logEx, "Middleware logging failed");
            }

            throw;
        }
    }

}
