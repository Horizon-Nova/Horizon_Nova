using HNB.IIS.Core.Helpers;
using System.Text;

namespace HNB.IIS.Core.Utilities;

/// <summary>
/// 錯誤處理工具類
/// </summary>
public static class ErrorUtility
{
    /// <summary>
    /// 取得完整的異常訊息（包含內部異常）
    /// </summary>
    public static string GetFullExceptionMessage(Exception exception)
    {
        var sb = new StringBuilder();
        var current = exception;
        var level = 0;

        while (current != null)
        {
            if (level > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"--- Inner Exception (Level {level}) ---");
            }

            sb.AppendLine($"Type: {current.GetType().FullName}");
            sb.AppendLine($"Message: {current.Message}");

            if (!string.IsNullOrEmpty(current.StackTrace))
            {
                sb.AppendLine("Stack Trace:");
                sb.AppendLine(current.StackTrace);
            }

            current = current.InnerException;
            level++;
        }

        return LogSanitizer.Clean(sb.ToString()) ?? string.Empty;
    }

    /// <summary>
    /// 取得簡化的錯誤訊息（只取最內層的異常訊息）
    /// </summary>
    public static string GetSimplifiedErrorMessage(Exception exception)
    {
        var innermost = GetInnermostException(exception);
        return LogSanitizer.Clean(innermost.Message) ?? "Unknown error";
    }

    /// <summary>
    /// 取得最內層的異常
    /// </summary>
    public static Exception GetInnermostException(Exception exception)
    {
        var current = exception;
        while (current.InnerException != null)
        {
            current = current.InnerException;
        }
        return current;
    }

    /// <summary>
    /// 取得異常的來源資訊
    /// </summary>
    public static string GetExceptionSource(Exception exception)
    {
        if (exception.TargetSite != null)
        {
            var method = exception.TargetSite;
            var className = method.DeclaringType?.Name ?? "Unknown";
            var methodName = method.Name;
            return $"{className}.{methodName}";
        }

        return exception.Source ?? "Unknown";
    }

    /// <summary>
    /// 判斷異常是否為致命錯誤
    /// </summary>
    public static bool IsFatalException(Exception exception)
    {
        return exception is OutOfMemoryException
            || exception is StackOverflowException
            || exception is AccessViolationException
            || exception is AppDomainUnloadedException
            || exception is BadImageFormatException
            || exception is InvalidProgramException;
    }

    /// <summary>
    /// 判斷異常是否為可預期的業務異常
    /// </summary>
    public static bool IsBusinessException(Exception exception)
    {
        return exception is ArgumentException
            || exception is ArgumentNullException
            || exception is InvalidOperationException
            || exception is KeyNotFoundException
            || exception is UnauthorizedAccessException;
    }

    /// <summary>
    /// 取得異常的嚴重程度
    /// </summary>
    public static string GetExceptionSeverity(Exception exception)
    {
        if (IsFatalException(exception))
            return "Critical";

        if (exception is UnauthorizedAccessException)
            return "Warning";

        if (IsBusinessException(exception))
            return "Information";

        return "Error";
    }

    /// <summary>
    /// 格式化堆疊追蹤（只保留專案相關的部分）
    /// </summary>
    public static string? FormatStackTrace(string? stackTrace, string projectNamespace = "HNB.IIS")
    {
        if (string.IsNullOrEmpty(stackTrace))
            return null;

        var lines = stackTrace.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var relevantLines = lines
            .Where(line => line.Contains(projectNamespace))
            .ToList();

        if (relevantLines.Any())
        {
            return LogSanitizer.Clean(string.Join(Environment.NewLine, relevantLines));
        }

        // 如果沒有專案相關的堆疊，返回前5行
        return LogSanitizer.Clean(string.Join(Environment.NewLine, lines.Take(5)));
    }

    /// <summary>
    /// 從 HttpContext 取得使用者資訊摘要
    /// </summary>
    public static string GetUserSummary(HttpContext? httpContext)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.Identity.Name ?? "Unknown";
            var claims = string.Join(", ", httpContext.User.Claims
                .Where(c => c.Type != "sub" && c.Type != "sid")
                .Select(c => $"{c.Type}={c.Value}")
                .Take(3));
            
            return $"User: {userId} ({claims})";
        }

        return "Anonymous";
    }

    /// <summary>
    /// 從 HttpContext 取得請求摘要
    /// </summary>
    public static string GetRequestSummary(HttpContext? httpContext)
    {
        if (httpContext == null)
            return "No HTTP context";

        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path;
        var query = httpContext.Request.QueryString.Value;
        var ip = GetClientIp(httpContext);

        return $"{method} {path}{query} from {ip}";
    }

    /// <summary>
    /// 取得客戶端 IP
    /// </summary>
    private static string GetClientIp(HttpContext httpContext)
    {
        return httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "Unknown";
    }

    /// <summary>
    /// 建立錯誤摘要（用於快速通知）
    /// </summary>
    public static string CreateErrorSummary(Exception exception, HttpContext? httpContext = null)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Exception: {exception.GetType().Name}");
        sb.AppendLine($"Message: {GetSimplifiedErrorMessage(exception)}");
        sb.AppendLine($"Source: {GetExceptionSource(exception)}");
        sb.AppendLine($"Severity: {GetExceptionSeverity(exception)}");
        
        if (httpContext != null)
        {
            sb.AppendLine($"Request: {GetRequestSummary(httpContext)}");
            sb.AppendLine($"User: {GetUserSummary(httpContext)}");
            sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        return sb.ToString();
    }
}

