using HNB.Extensions;
using HNB.Filters;
using HNB.Middleware;
using HNB.Utilities;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Models.Hnb;
using Models.HnbBackoffice;
using Models.HnbWeb;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LogErrorAttribute>();
    options.Filters.Add<RequestResponseLoggerFilter>();
});

builder.Services.AddDbContext<HnbDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Hnb")));
builder.Services.AddDbContext<HnbBackofficeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HnbBackoffice")));
builder.Services.AddDbContext<HnbWebDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HnbWeb")));

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

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();

var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".onnx"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Areas", "Backoffice", "storage")),
    RequestPath = "/storage",
    ContentTypeProvider = contentTypeProvider,
    ServeUnknownFileTypes = true
});

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

app.MapControllers();

app.Run();
