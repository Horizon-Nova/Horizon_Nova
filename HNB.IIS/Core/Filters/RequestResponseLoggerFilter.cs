using HNB.IIS.Core.Helpers;
using HNB.IIS.Core.Models.Hnbdata;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HNB.IIS.Core.Filters;

public class RequestResponseLoggerFilter : IAsyncResourceFilter
{
    
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var http = context.HttpContext;
        
        var isMultipartFormData = http.Request.ContentType?.StartsWith("multipart/form-data") ?? false;
        
        if (isMultipartFormData)
        {
            await next();
            return;
        }
        
        http.Request.EnableBuffering();
        string reqBody = await ReadRequestBodyAsync(http.Request);
        
        var originalBody = http.Response.Body;
        await using var memStream = new MemoryStream();
        http.Response.Body = memStream;
        
        var start = DateTime.UtcNow;
        Exception? error = null;
        string respText = string.Empty;
        
        try
        {
            var executed = await next();
            error = executed.Exception;
            
            memStream.Seek(0, SeekOrigin.Begin);
            respText = await ReadResponseBodyAsync(http.Response);
        }
        catch (Exception ex)
        {
            error = ex;
            throw;
        }
        finally
        {
            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBody);
            http.Response.Body = originalBody;
            
            var duration = (DateTime.UtcNow - start).TotalMilliseconds;
            await LogToDbAsync(http, reqBody, respText, duration, error);
        }
    }
    
    private static async Task<string> ReadRequestBodyAsync(HttpRequest req)
    {
        if (req.Method is not ("POST" or "PUT")) return "無 Request Body";
        
        using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        req.Body.Position = 0;
        
        return body.Length > 1000 ? body[..1000] + "..." : body;
    }
    
    private static async Task<string> ReadResponseBodyAsync(HttpResponse resp)
    {
        if (resp.ContentType?.StartsWith("application/json") == false &&
            resp.ContentType?.StartsWith("text/") == false)
            return $"Binary/Stream Response ({resp.ContentType ?? "unknown"})";
        
        using var reader = new StreamReader(resp.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        
        return body.Length > 1000 ? body[..1000] + "..." : body;
    }
    
    private static string GetClientIp(HttpContext http)
    {
        var forwarded = http.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var remoteIp = http.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }
        
        return remoteIp ?? "Unknown IP";
    }
    
    private static async Task LogToDbAsync(HttpContext http, string reqBody, string respBody, double duration, Exception? ex)
    {
        const int MaxBodyLen = 4000;
        
        string CleanBody(string body) =>
            LogSanitizer.Clean(body.Length > MaxBodyLen ? body[..MaxBodyLen] + "…(truncated)" : body) ?? "";
            
        var record = new access_record
        {
            id = Guid.NewGuid(),
            log_type = "action",
            user_name = LogSanitizer.Clean(http.User.Identity?.Name ?? "Anonymous") ?? "Anonymous",
            roles = LogSanitizer.Clean(http.User.Identity?.IsAuthenticated == true
                ? string.Join(',', http.User.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                    .Select(c => c.Value))
                : "Anonymous") ?? "Anonymous",
            request_path = LogSanitizer.Clean(http.Request.Path + http.Request.QueryString) ?? "",
            ip = LogSanitizer.Clean(GetClientIp(http)) ?? "",
            result = LogSanitizer.Clean(ex == null ? "執行成功" : $"執行失敗：{ex.Message}") ?? "",
            user_agent = LogSanitizer.Clean(http.Request.Headers["User-Agent"].ToString()),
            http_method = LogSanitizer.Clean(http.Request.Method) ?? "",
            request_body = CleanBody(reqBody),
            response_body = CleanBody(respBody),
            status_code = http.Response.StatusCode,
            duration_ms = duration,
        };
        
        var db = http.RequestServices.GetRequiredService<HnbdataDbContext>();
        db.access_records.Add(record);
        await db.SaveChangesAsync();
    }
}

