using HNB.IIS.Core.Extensions;
using HNB.IIS.Core.Filters;
using HNB.IIS.Core.Middleware;
using HNB.IIS.Core.Models;
using Microsoft.EntityFrameworkCore;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<RequestResponseLoggerFilter>();
}).AddHnbIisCoreControllers(); // 自動掛載 Core 的 Controllers 和 Views

builder.Services.AddScoped<SiteManagementService>();

builder.Services.AddDbContext<HnbdataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HnbDb")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSession();

builder.Services.AddHnbIisCore();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/NotFound");
    app.UseStatusCodePagesWithReExecute("/Error/NotFound");
    app.UseHsts();
}

app.UseMiddleware<ExceptionLoggingMiddleware>();
app.UseMiddleware<IpSecurityMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SiteManagement}/{action=Index}/{id?}");

app.Run();
