//using HNB.Models;
using HNB.Models;
using HNB.Filters;
using HNB.Utilities;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<RequestResponseLoggerFilter>(); // °O¿ý½Ð¨D
});

#if DEBUG
builder.Services.AddDbContext<RailwayContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TestConnection")));
#else
builder.Services.AddDbContext<RailwayContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProdConnection")));
#endif

builder.Services.AddHttpContextAccessor();

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
