// HNB/Areas/HnbBackoffice/Filters/CookieProbeAttribute.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HNB.Areas.HnbBackoffice.Filters;

public sealed class CookieProbeAttribute : TypeFilterAttribute
{
    /// <param name="cookieName">預設 HNB_API_TOKEN</param>
    /// <param name="order">執行順序（越小越早）。預設 -500，通常會跑在你自己的授權 Filter 之前。</param>
    public CookieProbeAttribute(string cookieName = "HNB_API_TOKEN", int order = -500)
        : base(typeof(CookieProbeFilter))
    {
        Arguments = new object[] { cookieName, order };
        Order = order;
    }
}

public sealed class CookieProbeFilter : IAuthorizationFilter, IOrderedFilter
{
    private readonly string _cookieName;
    private readonly ILogger<CookieProbeFilter> _log;

    public int Order { get; }

    public CookieProbeFilter(string cookieName, int order, ILogger<CookieProbeFilter> log)
    {
        _cookieName = cookieName;
        _log = log;
        Order = order;
    }

    #region CookieProbeFilter：列出所有 Cookie
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;

        // 放過 AllowAnonymous
        var ep = http.GetEndpoint();
        if (ep?.Metadata.GetMetadata<IAllowAnonymous>() is not null) return;

        var traceId = http.TraceIdentifier ?? Guid.NewGuid().ToString("N");
        var path = http.Request.Path + http.Request.QueryString;
        var scheme = http.Request.Scheme;
        var host = http.Request.Host.ToString();

        // 列出所有 Cookie
        if (http.Request.Cookies.Count > 0)
        {
            foreach (var kv in http.Request.Cookies)
            {
                var len = kv.Value?.Length ?? 0;
                var head = len >= 12 ? kv.Value.Substring(0, 12) : kv.Value;

                _log.LogInformation("[CookieProbe] {Scheme}://{Host}{Path} cookie={Name} len={Len} head={Head} trace={TraceId}",
                    scheme, host, path, kv.Key, len, head, traceId);

                Console.WriteLine($"[CookieProbe] {scheme}://{host}{path} cookie={kv.Key} len={len} head={head} trace={traceId}");
            }
        }
        else
        {
            _log.LogInformation("[CookieProbe] {Scheme}://{Host}{Path} no cookies trace={TraceId}",
                scheme, host, path, traceId);

            Console.WriteLine($"[CookieProbe] {scheme}://{host}{path} no cookies trace={traceId}");
        }

        // 可選：把所有 Cookie 名稱/數量加到 Response Header，方便 DevTools 看
        http.Response.Headers["X-Probe-CookieCount"] = http.Request.Cookies.Count.ToString();
        http.Response.Headers["X-Probe-CookieNames"] = string.Join(",", http.Request.Cookies.Keys);

        // 不做攔截
    }
    #endregion

}
