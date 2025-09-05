using HNB.Areas.HnbBackoffice.BackgroundServices;
using HNB.Areas.HnbBackoffice.Repositories;
using HNB.Areas.HnbBackoffice.Services;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace HNB.Areas.HnbBackoffice.Extensions;

public static class HnbBackofficeExtensions
{
    /// <summary> DI注入管理 Settings 功能 </summary>
    public static IServiceCollection AddSettingsModule(this IServiceCollection services)
    {
        services.AddScoped<SettingsServices>();
        services.AddScoped<SettingsRepositories>();
        return services;
    }
    /// <summary> DI注入管理 Backoffice 功能 </summary>
    public static IServiceCollection AddBackofficeModule(this IServiceCollection services)
    {
        services.AddScoped<BackofficeService>();
        return services;
    }    
    /// <summary> DI注入管理 SystemMonitorHosted 功能 </summary>
    public static IServiceCollection AddSystemMonitorHostedModule(this IServiceCollection services)
    {
        services.AddScoped<SystemMonitorHostedRepositories>();
        services.AddHostedService<SystemMonitorHostedService>();
        return services;
    }
    /// <summary> DI注入管理 DbKeyJwt 功能 </summary>
    public static IServiceCollection AddDbKeyJwtModule(this IServiceCollection services)
    {
        services.AddScoped<DbKeyJwtRepositories>();
        services.AddScoped<DbKeyJwtService>();
        return services;
    }
}
