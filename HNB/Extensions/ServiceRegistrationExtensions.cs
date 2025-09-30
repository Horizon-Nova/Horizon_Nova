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
        services.AddScoped<FileManagerService>();
        services.AddScoped<PermissionManagementService>();
        return services;
    }
    /// <summary> DI注入管理 Repositories 功能 </summary>
    public static IServiceCollection RepositoriesModule(this IServiceCollection services)
    {
        services.AddScoped<TeamZoneRepositories>();
        services.AddScoped<PermissionManagementRepository>();
        services.AddScoped<SettingsRepositories>();
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
