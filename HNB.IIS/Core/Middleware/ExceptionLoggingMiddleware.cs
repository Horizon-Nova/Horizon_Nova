using HNB.IIS.Core.Services;
using System.Net;

namespace HNB.IIS.Core.Middleware;

/// <summary>
/// 全局異常捕捉中間件 - 在最外層捕捉所有未處理的異常
/// </summary>
public class ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ErrorLogService errorLogService)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // 記錄異常到資料庫和日誌系統
            logger.LogError(ex, "Unhandled exception caught in middleware. Path: {Path}, Method: {Method}", 
                context.Request.Path, 
                context.Request.Method);

            // 記錄到資料庫（stage = 0 表示 Middleware 層級）
            await errorLogService.LogExceptionAsync(ex, context, stage: 0);

            // 處理回應
            await HandleExceptionResponseAsync(context, ex);
        }
    }

    /// <summary>
    /// 處理異常回應
    /// </summary>
    private async Task HandleExceptionResponseAsync(HttpContext context, Exception exception)
    {
        // 設定回應狀態碼
        context.Response.StatusCode = GetStatusCode(exception);
        context.Response.ContentType = "text/html; charset=utf-8";

        // 根據環境決定要顯示的錯誤資訊
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        if (isDevelopment)
        {
            // 開發環境：顯示詳細錯誤資訊
            await context.Response.WriteAsync(GenerateDevelopmentErrorPage(context, exception));
        }
        else
        {
            // 生產環境：重定向到錯誤頁面
            context.Response.Redirect("/Error/NotFound");
        }
    }

    /// <summary>
    /// 根據異常類型決定 HTTP 狀態碼
    /// </summary>
    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            InvalidOperationException => (int)HttpStatusCode.Conflict,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    /// <summary>
    /// 生成開發環境錯誤頁面
    /// </summary>
    private string GenerateDevelopmentErrorPage(HttpContext context, Exception exception)
    {
        var html = $@"
<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>系統錯誤 - 開發模式</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
            color: #333;
        }}
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
            padding: 30px;
            border-bottom: 4px solid #e03e4d;
        }}
        .header h1 {{
            font-size: 32px;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            gap: 15px;
        }}
        .header .icon {{
            font-size: 42px;
        }}
        .header .subtitle {{
            opacity: 0.9;
            font-size: 16px;
        }}
        .content {{
            padding: 30px;
        }}
        .section {{
            margin-bottom: 30px;
            border-left: 4px solid #667eea;
            padding-left: 20px;
        }}
        .section h2 {{
            color: #667eea;
            font-size: 20px;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 10px;
        }}
        .section .badge {{
            background: #667eea;
            color: white;
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: bold;
        }}
        .error-message {{
            background: #fff5f5;
            border: 2px solid #feb2b2;
            border-radius: 8px;
            padding: 20px;
            color: #c53030;
            font-size: 16px;
            line-height: 1.6;
            word-break: break-word;
        }}
        .stack-trace {{
            background: #1e1e1e;
            color: #d4d4d4;
            border-radius: 8px;
            padding: 20px;
            overflow-x: auto;
            font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
            font-size: 13px;
            line-height: 1.6;
            max-height: 500px;
            overflow-y: auto;
        }}
        .stack-trace::-webkit-scrollbar {{
            width: 10px;
            height: 10px;
        }}
        .stack-trace::-webkit-scrollbar-track {{
            background: #2d2d2d;
            border-radius: 4px;
        }}
        .stack-trace::-webkit-scrollbar-thumb {{
            background: #555;
            border-radius: 4px;
        }}
        .stack-trace::-webkit-scrollbar-thumb:hover {{
            background: #666;
        }}
        .info-grid {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 15px;
            margin-top: 15px;
        }}
        .info-card {{
            background: #f7fafc;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 15px;
        }}
        .info-card .label {{
            color: #718096;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
            margin-bottom: 5px;
        }}
        .info-card .value {{
            color: #2d3748;
            font-size: 16px;
            font-weight: 500;
            word-break: break-word;
        }}
        .footer {{
            background: #f7fafc;
            padding: 20px 30px;
            border-top: 1px solid #e2e8f0;
            text-align: center;
            color: #718096;
            font-size: 14px;
        }}
        .warning-banner {{
            background: #fef5e7;
            border: 2px solid #f39c12;
            border-radius: 8px;
            padding: 15px 20px;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 15px;
        }}
        .warning-banner .icon {{
            font-size: 24px;
            color: #f39c12;
        }}
        .warning-banner .text {{
            flex: 1;
            color: #d68910;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>
                <span class='icon'>⚠️</span>
                系統錯誤
            </h1>
            <div class='subtitle'>
                應用程式發生未處理的異常 - 開發模式詳細資訊
            </div>
        </div>
        
        <div class='content'>
            <div class='warning-banner'>
                <span class='icon'>🔧</span>
                <div class='text'>
                    <strong>開發模式警告：</strong>此頁面僅在開發環境中顯示。生產環境將重定向到標準錯誤頁面。
                </div>
            </div>

            <div class='section'>
                <h2>
                    <span class='badge'>錯誤訊息</span>
                </h2>
                <div class='error-message'>
                    {System.Web.HttpUtility.HtmlEncode(exception.Message)}
                </div>
            </div>

            <div class='section'>
                <h2>
                    <span class='badge'>請求資訊</span>
                </h2>
                <div class='info-grid'>
                    <div class='info-card'>
                        <div class='label'>請求路徑</div>
                        <div class='value'>{System.Web.HttpUtility.HtmlEncode(context.Request.Path)}</div>
                    </div>
                    <div class='info-card'>
                        <div class='label'>HTTP 方法</div>
                        <div class='value'>{context.Request.Method}</div>
                    </div>
                    <div class='info-card'>
                        <div class='label'>狀態碼</div>
                        <div class='value'>{context.Response.StatusCode}</div>
                    </div>
                    <div class='info-card'>
                        <div class='label'>異常類型</div>
                        <div class='value'>{System.Web.HttpUtility.HtmlEncode(exception.GetType().FullName ?? "Unknown")}</div>
                    </div>
                    <div class='info-card'>
                        <div class='label'>追蹤識別碼</div>
                        <div class='value'>{System.Web.HttpUtility.HtmlEncode(context.TraceIdentifier)}</div>
                    </div>
                    <div class='info-card'>
                        <div class='label'>時間</div>
                        <div class='value'>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</div>
                    </div>
                </div>
            </div>

            <div class='section'>
                <h2>
                    <span class='badge'>堆疊追蹤</span>
                </h2>
                <div class='stack-trace'>{System.Web.HttpUtility.HtmlEncode(exception.StackTrace ?? "無堆疊追蹤資訊")}</div>
            </div>

            {(exception.InnerException != null ? $@"
            <div class='section'>
                <h2>
                    <span class='badge'>內部異常</span>
                </h2>
                <div class='error-message'>
                    {System.Web.HttpUtility.HtmlEncode(exception.InnerException.Message)}
                </div>
                <div class='stack-trace' style='margin-top: 15px;'>
                    {System.Web.HttpUtility.HtmlEncode(exception.InnerException.StackTrace ?? "無堆疊追蹤資訊")}
                </div>
            </div>
            " : "")}
        </div>

        <div class='footer'>
            此錯誤已自動記錄到系統日誌中 | HNB.IIS Core System
        </div>
    </div>
</body>
</html>";

        return html;
    }
}

