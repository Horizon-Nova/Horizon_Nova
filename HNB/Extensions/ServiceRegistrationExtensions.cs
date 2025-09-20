using HNB.Areas.HNB_WEB.Repositories;
using HNB.Areas.HNB_WEB.Services;
using HNB.Areas.HnbBackoffice.BackgroundServices;
using HNB.Areas.HnbBackoffice.Repositories;
using HNB.Areas.HnbBackoffice.Services;
using HNB.Areas.HnbBackoffice.Utilities;
using HNB.Repositories;
using HNB.Services;
using Microsoft.Extensions.DependencyInjection;
using Models.HnbHnbBackoffice;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;


namespace HNB.Extensions;

public static class ServiceRegistrationExtensions
{
    /// <summary> DI注入管理 業務層/服務層 功能 </summary>
    public static IServiceCollection ServiceModule(this IServiceCollection services)
    {
        services.AddScoped<ErrorLogService>();
        services.AddScoped<IpMiddlewareServices>();
        services.AddScoped<GitHubAccessServices>();
        services.AddScoped<UserManagementService>();
        services.AddScoped<AuthorizeService>();
        services.AddScoped<DbKeyJwtService>();
        services.AddHostedService<SystemMonitorHostedService>();
        services.AddScoped<BackofficeService>();
        services.AddScoped<SettingsServices>();
        services.AddScoped<TeamZoneServices>();

        return services;
    }

    /// <summary> DI注入管理 倉儲層 功能 </summary>
    public static IServiceCollection RepositoriesModule(this IServiceCollection services)
    {
        services.AddScoped<ErrorLogRepository>();
        services.AddScoped<UserManagementRepositories>();
        services.AddScoped<DbKeyJwtRepositories>();
        services.AddScoped<SystemMonitorHostedRepositories>();
        services.AddScoped<SettingsRepositories>();
        services.AddScoped<TeamZoneRepositories>();

        return services;
    }
    /// <summary> DI注入管理 Utilities 功能 </summary>
    public static IServiceCollection UtilitiesModule(this IServiceCollection services)
    {
        services.AddScoped<GitHubAccessUtilities>();
        services.AddSingleton<DirectoryManagerUtilities>();
        services.AddSingleton<DbKeyJwtUtilities>();
        return services;
    }


}
