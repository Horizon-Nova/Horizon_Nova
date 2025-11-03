using HNB.IIS.Core.Helpers;
using HNB.IIS.Core.Models.Hnbdata;
using HNB.IIS.Core.Repositories;
using System.Text.Json;

namespace HNB.IIS.Core.Services;

public class ErrorLogService(ErrorLogRepository repository, ILogger<ErrorLogService> logger)
{
    /// <summary>
    /// 記錄異常到資料庫
    /// </summary>
    public async Task LogExceptionAsync(
        Exception exception,
        HttpContext? httpContext = null,
        short stage = 0,
        string? functionName = null)
    {
        try
        {
            var errorLog = CreateErrorLog(exception, httpContext, stage, functionName);
            await Task.Run(() => repository.InsertErrorLog(errorLog));
        }
        catch (Exception ex)
        {
            // 如果記錄錯誤本身失敗，至少要記錄到日誌系統
            logger.LogError(ex, "Failed to log exception to database. Original exception: {OriginalException}", 
                exception.Message);
        }
    }

    /// <summary>
    /// 建立錯誤日誌物件
    /// </summary>
    private error_log CreateErrorLog(
        Exception exception,
        HttpContext? httpContext,
        short stage,
        string? functionName)
    {
        var errorLog = new error_log
        {
            stage = stage,
            layer = GetLayerName(stage),
            function = functionName ?? GetFunctionName(httpContext, exception),
            function_full = GetFunctionFullName(exception),
            message = LogSanitizer.Clean(exception.ToString()) ?? string.Empty,
            stack_trace = LogSanitizer.Clean(exception.StackTrace),
            created_at = DateTime.UtcNow
        };

        // 如果有 HttpContext，填充相關資訊
        if (httpContext != null)
        {
            errorLog.path = LogSanitizer.Clean(httpContext.Request.Path.Value);
            errorLog.http_method = httpContext.Request.Method;
            errorLog.status_code = httpContext.Response.StatusCode;
            errorLog.user_id = LogSanitizer.Clean(httpContext.User?.Identity?.Name);
            errorLog.trace_id = LogSanitizer.Clean(httpContext.TraceIdentifier);
            errorLog.extra = CreateExtraData(httpContext, exception);
        }

        return errorLog;
    }

    /// <summary>
    /// 取得層級名稱
    /// </summary>
    private string GetLayerName(short stage)
    {
        return stage switch
        {
            0 => "Middleware",
            1 => "Filter",
            2 => "ExceptionHandler",
            3 => "Background",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// 取得函數名稱
    /// </summary>
    private string GetFunctionName(HttpContext? httpContext, Exception exception)
    {
        if (httpContext != null)
        {
            var endpoint = httpContext.GetEndpoint();
            var routeValues = httpContext.Request.RouteValues;
            
            var controller = routeValues.ContainsKey("controller") 
                ? routeValues["controller"]?.ToString() 
                : "Unknown";
            var action = routeValues.ContainsKey("action") 
                ? routeValues["action"]?.ToString() 
                : "Unknown";
            
            return $"{controller}/{action}";
        }

        return exception.TargetSite?.Name ?? "Unknown";
    }

    /// <summary>
    /// 取得完整函數名稱
    /// </summary>
    private string? GetFunctionFullName(Exception exception)
    {
        if (exception.TargetSite == null)
            return null;

        var method = exception.TargetSite;
        var className = method.DeclaringType?.FullName ?? "Unknown";
        var methodName = method.Name;
        
        return LogSanitizer.Clean($"{className}.{methodName}()");
    }

    /// <summary>
    /// 建立額外資料 JSON
    /// </summary>
    private string CreateExtraData(HttpContext httpContext, Exception exception)
    {
        try
        {
            var extra = new Dictionary<string, object?>
            {
                ["ip"] = GetClientIp(httpContext),
                ["user_agent"] = LogSanitizer.Clean(httpContext.Request.Headers["User-Agent"].ToString()),
                ["referer"] = LogSanitizer.Clean(httpContext.Request.Headers["Referer"].ToString()),
                ["query_string"] = LogSanitizer.Clean(httpContext.Request.QueryString.ToString()),
                ["exception_type"] = exception.GetType().FullName,
                ["inner_exception"] = exception.InnerException?.Message
            };

            // 添加請求標頭（過濾敏感資訊）
            var headers = httpContext.Request.Headers
                .Where(h => !IsSensitiveHeader(h.Key))
                .ToDictionary(h => h.Key, h => h.Value.ToString());
            
            extra["headers"] = headers;

            return JsonSerializer.Serialize(extra, new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
        catch
        {
            return "{}";
        }
    }

    /// <summary>
    /// 取得客戶端 IP
    /// </summary>
    private string? GetClientIp(HttpContext httpContext)
    {
        var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? httpContext.Connection.RemoteIpAddress?.ToString();
        
        return LogSanitizer.Clean(ip);
    }

    /// <summary>
    /// 判斷是否為敏感標頭
    /// </summary>
    private bool IsSensitiveHeader(string headerName)
    {
        var sensitive = new[] { "Authorization", "Cookie", "X-API-Key", "X-Auth-Token" };
        return sensitive.Any(s => s.Equals(headerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 查詢錯誤日誌列表
    /// </summary>
    public List<error_log> GetErrorLogs(int count = 100)
    {
        return repository.QueryErrorLogList(count);
    }

    /// <summary>
    /// 查詢錯誤日誌總數
    /// </summary>
    public int GetErrorLogCount()
    {
        return repository.QueryErrorLogCount();
    }
}

