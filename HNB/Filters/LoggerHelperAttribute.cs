using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HNB.Models;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace HNB.Filters;

public class RequestResponseLoggerFilter : IAsyncResourceFilter
{
    private readonly ILogger<RequestResponseLoggerFilter> _logger;
    public RequestResponseLoggerFilter(ILogger<RequestResponseLoggerFilter> logger) => _logger = logger;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var http = context.HttpContext;

        /* ---------- Request ---------- */
        http.Request.EnableBuffering();
        string reqBody = await ReadRequestBodyAsync(http.Request);

        /* ---------- 攔截 Response ---------- */
        var originalBody = http.Response.Body;
        await using var memStream = new MemoryStream();
        http.Response.Body = memStream;

        var start = DateTime.Now;
        var executed = await next();
        var duration = (DateTime.Now - start).TotalMilliseconds;

        /* ---------- 讀 Response ---------- */
        memStream.Seek(0, SeekOrigin.Begin);
        string respText = await ReadResponseBodyAsync(http.Response);
        memStream.Seek(0, SeekOrigin.Begin);
        await memStream.CopyToAsync(originalBody);

        /* ---------- 寫入 DB ---------- */
        await LogToDbAsync(http, reqBody, respText, duration, executed.Exception);
    }

    /* ----------------- Helper ----------------- */

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

        var trustedProxies = new[] { "127.0.0.1", "::1" };
        bool isTrustedProxy = trustedProxies.Contains(remoteIp);

        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }

        return http.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown IP";
    }

    private static async Task LogToDbAsync(HttpContext http, string reqBody, string respBody,
                                           double duration, Exception? ex)
    {
        string ip = GetClientIp(http);

        var record = new AccessRecord
        {
            Id = Guid.NewGuid(),
            LogType = "action",
            UserName = http.User.Identity?.Name ?? "Anonymous",
            Roles = http.User.Identity?.IsAuthenticated == true
                ? string.Join(',', http.User.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                    .Select(c => c.Value))
                : "Anonymous",
            RequestPath = http.Request.Path + http.Request.QueryString,
            Ip = ip,
            Result = ex == null ? "執行成功" : $"執行失敗：{ex.Message}",
            UserAgent = http.Request.Headers["User-Agent"].ToString(),
            HttpMethod = http.Request.Method,
            RequestBody = reqBody,
            ResponseBody = respBody,
            StatusCode = http.Response.StatusCode,
            DurationMs = duration,
        };

        var db = http.RequestServices.GetRequiredService<RailwayContext>();
        db.AccessRecords.Add(record);
        await db.SaveChangesAsync();
    }
}
