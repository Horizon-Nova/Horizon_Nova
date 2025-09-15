using HNB.Areas.HnbBackoffice.BackgroundServices;
using HNB.Areas.HnbBackoffice.Repositories;
using HNB.Areas.HnbBackoffice.Services;
using HNB.Areas.HnbBackoffice.Utilities;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace HNB.Areas.HnbBackoffice.Extensions;

public static class HnbBackofficeExtensions
{
    /// <summary> DI注入管理 業務層/服務層 功能 </summary>
    public static IServiceCollection HnbBackofficeServiceModule(this IServiceCollection services)
    {
        services.AddScoped<UserManagementService>();
        services.AddScoped<AuthorizeService>();
        services.AddScoped<DbKeyJwtService>();
        services.AddHostedService<SystemMonitorHostedService>();
        services.AddScoped<BackofficeService>();
        services.AddScoped<SettingsServices>();
        return services;
    }

    /// <summary> DI注入管理 倉儲層 功能 </summary>
    public static IServiceCollection HnbBackofficeRepositoriesModule(this IServiceCollection services)
    {
        services.AddScoped<UserManagementRepositories>();
        services.AddScoped<DbKeyJwtRepositories>();
        services.AddScoped<SystemMonitorHostedRepositories>();
        services.AddScoped<SettingsRepositories>();
        return services;
    }
    /// <summary> DI注入管理 Utilities 功能 </summary>
    public static IServiceCollection HnbBackofficeUtilitiesModule(this IServiceCollection services)
    {
        services.AddSingleton<DirectoryManagerUtilities>();
        services.AddSingleton<DbKeyJwtUtilities>();
        return services;
    }
}
