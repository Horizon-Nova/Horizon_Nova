using HNB.IIS.Core.Repositories;
using HNB.IIS.Core.Services;
using HNB.IIS.Core.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Razor;

namespace HNB.IIS.Core.Extensions;

public static class CoreExtensions
{
    public static IServiceCollection AddHnbIisCore(this IServiceCollection services)
    {
        // 註冊倉儲和服務
        services.AddScoped<ErrorLogRepository>();
        services.AddScoped<BlockedIpRepository>();
        services.AddScoped<PermissionRepository>();
        services.AddScoped<ErrorLogService>();
        services.AddScoped<IpMiddlewareServices>();
        
        return services;
    }

    public static IMvcBuilder AddHnbIisCoreControllers(this IMvcBuilder builder)
    {
        // 自動註冊 Core 中的 Controllers（如 ErrorController）
        builder.AddApplicationPart(typeof(CoreExtensions).Assembly);
        
        // 配置 Razor 視圖引擎，讓它能找到 Core 中的 Views
        builder.Services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationFormats.Add("/../Core/Views/{1}/{0}.cshtml");
            options.ViewLocationFormats.Add("/../Core/Views/Shared/{0}.cshtml");
        });
        
        return builder;
    }
}

