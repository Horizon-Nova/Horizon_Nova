using HNB.Extensions;
using HNB.Filters;
using HNB.Middleware;
using HNB.Utilities;
using HNB.Areas.Backoffice.BackgroundServices.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Models.Hnbdata;
using Models.HnbHnbBackoffice;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LogErrorAttribute>();
    options.Filters.Add<RequestResponseLoggerFilter>();
});

builder.Services.AddDbContext<HnbdataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Hnbdata")));
builder.Services.AddDbContext<HnbHnbBackofficeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HnbHnbBackoffice")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services
    .ServicesModule()
    .RepositoriesModule()
    .UtilitiesModule();

builder.Services.Configure<ForwardedHeadersOptions>(opt =>
{
    opt.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    opt.KnownNetworks.Clear();
    opt.KnownProxies.Clear();
});

builder.Services.AddSession();

builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Backoffice/Authorize/Login";
        options.LogoutPath = "/Backoffice/Authorize/Logout";
        options.AccessDeniedPath = "/Backoffice/Authorize/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 4294967296;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 4294967296;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

var app = builder.Build();

app.UseExceptionHandler("/Error/NotFound");
app.UseStatusCodePagesWithReExecute("/Error/NotFound");
app.UseHsts();

app.UseMiddleware<ExceptionLoggingMiddleware>();
app.UseMiddleware<IpSecurityMiddleware>();
//app.UseMiddleware<HardwareMonitoringMiddleware>();

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller= }/{action= }/{id?}");

//using (var scope = app.Services.CreateScope())
//{
//    var dirUtil = scope.ServiceProvider.GetRequiredService<HNB.Areas.Backoffice.Utilities.DirectoryManagerUtilities>();
//    dirUtil.SyncAllFilesToDatabase();
//}

// ⚠️ 防止意外部署到正式環境 - 開發中請勿移除此錯誤 ⚠️
//#error "開發中：重構尚未完成測試，禁止部署到正式環境！"

app.Run();
