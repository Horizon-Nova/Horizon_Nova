using HNB.IIS.Core.Filters;
using HNB.IIS.Core.Middleware;
using HNB.IIS.Core.Models.Hnbdata;
using HNB.IIS.Core.Models.HnbHnbBackoffice;
using HNB.IIS.Core.Repositories;
using HNB.IIS.Core.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 認證
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Error/NotFound";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1073741824;
});

builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1073741824;
});

// MVC
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<RequestResponseLoggerFilter>();
});

// 資料庫
builder.Services.AddDbContext<HnbdataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Hnbdata")));

builder.Services.AddDbContext<HnbHnbBackofficeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HnbHnbBackoffice")));

// 倉儲
builder.Services.AddScoped<BlockedIpRepository>();
builder.Services.AddScoped<PermissionRepository>();

// 服務
builder.Services.AddScoped<IpMiddlewareServices>();
builder.Services.AddScoped<SiteManagementService>();

// 其他
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// 錯誤處理
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/NotFound");
    app.UseStatusCodePagesWithReExecute("/Error/NotFound");
    app.UseHsts();
}

app.UseMiddleware<IpSecurityMiddleware>();

// 標準中間件
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SiteManagement}/{action=Index}/{id?}");

app.Run();

