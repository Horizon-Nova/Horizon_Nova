using HNB.Areas.Backoffice.BackgroundServices;
using HNB.Areas.Backoffice.BackgroundServices.Middleware;
using HNB.Areas.Backoffice.BackgroundServices.Repositories;
using HNB.Areas.Backoffice.Repositories;
using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.HNB_WEB.Repositories;
using HNB.Areas.HNB_WEB.Services;
using HNB.Repositories;
using HNB.Services;

namespace HNB.Extensions;

public static class ServiceRegistrationExtensions
{
    /// <summary> DI注入管理 Services 功能 </summary>
    public static IServiceCollection ServicesModule(this IServiceCollection services)
    {
        services.AddScoped<ErrorLogService>();
        services.AddScoped<ErrorLogRepository>();
        services.AddScoped<SettingsServices>();
        services.AddScoped<IpMiddlewareServices>();
        services.AddScoped<TeamZoneServices>();
        services.AddScoped<FileManagerServices>();
        services.AddScoped<PermissionManagementService>();
        services.AddScoped<AuthService>();
        services.AddScoped<SidebarNavigationService>();
        services.AddScoped<DatabaseService>();
        services.AddScoped<AIModelServices>();
        return services;
    }
    /// <summary> DI注入管理 Repositories 功能 </summary>
    public static IServiceCollection RepositoriesModule(this IServiceCollection services)
    {
        services.AddScoped<TeamZoneRepositories>();
        services.AddScoped<PermissionManagementRepository>();
        services.AddScoped<SettingsRepositories>();
        services.AddScoped<AuthRepository>();
        services.AddScoped<SidebarNavigationRepository>();
        services.AddScoped<HardwareMonitoringRepository>();
        services.AddScoped<BlockedIpRepository>();
        services.AddScoped<AIModelRepositories>();
        services.AddScoped<FileManagerRepository>();
        return services;
    }
    /// <summary> DI注入管理 Utilities 功能 </summary>
    public static IServiceCollection UtilitiesModule(this IServiceCollection services)
    {
        services.AddScoped<DirectoryManagerUtilities>();
        services.AddScoped<CacheManagementUtilities>();
        return services;
    }

}
