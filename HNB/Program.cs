//using HNB.Models;
using HNB.Models;
using HNB.Filters;
using HNB.Utilities;
using HNB.Extensions;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<RequestResponseLoggerFilter>(); // 記錄請求
});

#if DEBUG
builder.Services.AddDbContext<RailwayContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TestConnection")));
#else
builder.Services.AddDbContext<RailwayContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProdConnection")));
#endif

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// 依賴注入集中管理
builder.Services.AddGitHubAccessModule();

// Data-Protection 金鑰
var keyPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "DataProtectionKeys");
builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyPath));

// Cookie 驗證
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Auth/Login";                        // 未登入導向頁
        opt.AccessDeniedPath = "/Auth/AccessDenied";          // 沒權限導向頁
        opt.Cookie.HttpOnly = true;                           // JS 拿不到，防 XSS
        opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // 只允許 HTTPS
        opt.Cookie.SameSite = SameSiteMode.Strict;            // 嚴格不帶跨域，防 CSRF
        opt.ExpireTimeSpan = TimeSpan.FromMinutes(45);        // 45 分鐘自動失效
    });

// 反向 Proxy 標頭
builder.Services.Configure<ForwardedHeadersOptions>(opt =>
{
    opt.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    opt.KnownNetworks.Clear();
    opt.KnownProxies.Clear();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<ExceptionLoggingMiddleware>();
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Overview}/{action=Team_introduction}/{id?}");

app.Run();
