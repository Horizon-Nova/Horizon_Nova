using HNB.Helpers;
using System.Text;
using System.Text.Json;
using HNB.Repositories;
using Models.Hnb;

namespace HNB.Services;

/// <summary>
/// 錯誤日誌服務層，負責處理錯誤日誌的業務邏輯
/// </summary>
public class ErrorLogService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ErrorLogRepository errorLogRepository)
{
    /// <summary>
    /// 發送錯誤日誌到 n8n webhook
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <param name="ex">例外物件</param>
    /// <param name="layer">發生錯誤的層級</param>
    /// <param name="stage">錯誤階段</param>
    /// <returns>成功返回 true，失敗返回 false</returns>
    public bool Save(HttpContext context, Exception ex, string layer, short stage)
    {
        var endpoint = context.GetEndpoint();
        string function = context.Request.Path;
        string? functionFull = null;

        if (endpoint != null)
        {
            var descriptor = endpoint.Metadata
                .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
                .FirstOrDefault();

            if (descriptor != null)
            {
                function = descriptor.ActionName;
                functionFull = $"{descriptor.ControllerTypeInfo.FullName}.{descriptor.ActionName}";
            }
        }

        var dbSaved = true;
        try
        {
            var log = new error_log
            {
                id = Guid.NewGuid(),
                created_at = DateTime.UtcNow,
                message = LogSanitizer.Clean(ex.Message) ?? "",
                layer = LogSanitizer.Clean(layer) ?? "",
                stage = stage,
                function = LogSanitizer.Clean(function) ?? "",
                function_full = LogSanitizer.Clean(functionFull) ?? "",
                stack_trace = LogSanitizer.Clean(ex.StackTrace) ?? "",
                path = LogSanitizer.Clean(context.Request.Path) ?? "",
                http_method = LogSanitizer.Clean(context.Request.Method) ?? "",
                status_code = context.Response?.StatusCode ?? 0,
                trace_id = LogSanitizer.Clean(context.TraceIdentifier) ?? "",
                user_id = LogSanitizer.Clean(context.User.Identity?.Name ?? "Anonymous") ?? ""
            };

            dbSaved = errorLogRepository.Insert(log);
        }
        catch
        {
            dbSaved = false;
        }

        // 非同步發送到 n8n webhook（不阻塞主流程）
        _ = Task.Run(async () =>
        {
            try
            {
                await SendToN8nWebhook(context, ex, layer, stage, function, functionFull);
            }
            catch
            {
                // 忽略 webhook 發送失敗，避免影響主流程
            }
        });

        return dbSaved;
    }

    /// <summary>
    /// 發送錯誤日誌到 n8n webhook
    /// </summary>
    private async Task SendToN8nWebhook(HttpContext context, Exception ex, string layer, short stage, string function, string? functionFull)
    {
        var webhookUrl = configuration["N8n:ErrorLogWebhookUrl"];
        var webhookAuth = configuration["N8n:ErrorLogWebhookAuth"];
        if (string.IsNullOrEmpty(webhookUrl))
            return;

        var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        if (!string.IsNullOrEmpty(webhookAuth))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", webhookAuth);
        }

        var payload = new
        {
            id = Guid.NewGuid().ToString(),
            message = LogSanitizer.Clean(ex.Message) ?? "",
            layer = LogSanitizer.Clean(layer) ?? "",
            stage = stage,
            function = LogSanitizer.Clean(function) ?? "",
            function_full = LogSanitizer.Clean(functionFull) ?? "",
            stack_trace = LogSanitizer.Clean(ex.StackTrace) ?? "",
            path = LogSanitizer.Clean(context.Request.Path) ?? "",
            http_method = LogSanitizer.Clean(context.Request.Method) ?? "",
            status_code = context.Response?.StatusCode ?? 0,
            trace_id = LogSanitizer.Clean(context.TraceIdentifier) ?? "",
            user_id = LogSanitizer.Clean(context.User.Identity?.Name ?? "Anonymous") ?? ""
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await httpClient.PostAsync(webhookUrl, content);
    }
}

