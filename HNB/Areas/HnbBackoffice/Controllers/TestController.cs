using System.Text;
using HNB.Areas.HnbBackoffice.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HnbBackoffice.Controllers
{
    [Area("HnbBackoffice")]
    [Route("HnbBackoffice/[controller]/[action]")]
    public class TestController : Controller
    {
        private const string TestCookie = "HNB_TEST";

        #region 設定 Cookie（支援跨站模式）
        /// <summary>設定測試 Cookie。?cross=1 啟用 SameSite=None; Partitioned。</summary>
        [HttpGet]
        public IActionResult SetCookie(int cross = 0, int seconds = 300)
        {
            var val = $"v{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var exp = DateTimeOffset.UtcNow.AddSeconds(seconds);

            var opts = new CookieOptions
            {
                Expires = exp,
                HttpOnly = true,
                Secure = true,
                Path = "/",
                SameSite = cross == 1 ? SameSiteMode.None : SameSiteMode.Lax
            };
            if (cross == 1) opts.Extensions.Add("Partitioned");

            Response.Cookies.Append(TestCookie, val, opts);

            // 觀察實際 Set-Cookie
            Response.OnStarting(() =>
            {
                var sc = string.Join(" | ", Response.Headers["Set-Cookie"].ToArray());
                Console.WriteLine($"[TestCookie] Set-Cookie => {sc}");
                return Task.CompletedTask;
            });

            return Json(new
            {
                ok = true,
                cookie = TestCookie,
                value = val,
                crossSite = cross == 1,
                expiresAt = exp
            });
        }
        #endregion

        #region 讀取 Cookie（列出全部與指定）
        /// <summary>列出所有 Cookie 與目標 Cookie 值。</summary>
        [HttpGet]
        public IActionResult GetCookie()
        {
            var all = HttpContext.Request.Cookies
                .Select(kv => new { name = kv.Key, len = kv.Value?.Length ?? 0, head = (kv.Value ?? string.Empty).PadRight(12).Substring(0, 12) })
                .ToList();

            var has = HttpContext.Request.Cookies.TryGetValue(TestCookie, out var v);

            return Json(new
            {
                ok = true,
                count = all.Count,
                all,
                target = new { name = TestCookie, has, value = has ? v : null }
            });
        }
        #endregion

        #region 清除 Cookie（多路徑嘗試）
        /// <summary>刪除測試 Cookie（嘗試多個常見 Path；跨站時同樣用 None）。</summary>
        [HttpGet]
        public IActionResult ClearCookie(int cross = 0)
        {
            var paths = new[] { "/", "/HnbBackoffice", "/HnbBackoffice/Test" };
            foreach (var p in paths)
            {
                Response.Cookies.Delete(TestCookie, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Path = p,
                    SameSite = cross == 1 ? SameSiteMode.None : SameSiteMode.Lax
                });
            }
            Response.Cookies.Delete(TestCookie);

            return Json(new { ok = true, cleared = true });
        }
        #endregion

        #region 健康檢查（確認下一跳是否帶回 Cookie）
        /// <summary>回應請求端送來的 Cookie 標頭（僅用於除錯）。</summary>
        [HttpGet]
        public IActionResult EchoHeader()
        {
            var raw = HttpContext.Request.Headers.Cookie.ToString();
            return Content($"Cookie: {raw ?? "(none)"}", "text/plain", Encoding.UTF8);
        }
        #endregion
    }
}
