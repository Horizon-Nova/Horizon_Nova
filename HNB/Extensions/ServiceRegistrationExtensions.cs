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
    public static IServiceCollection HNBServiceModule(this IServiceCollection services)
    {
        services.AddScoped<ErrorLogService>();
        services.AddScoped<IpMiddlewareServices>();
        services.AddScoped<GitHubAccessServices>();

        return services;
    }

    /// <summary> DI注入管理 倉儲層 功能 </summary>
    public static IServiceCollection HNBRepositoriesModule(this IServiceCollection services)
    {
        services.AddScoped<ErrorLogRepository>();

        return services;
    }
    /// <summary> DI注入管理 Utilities 功能 </summary>
    public static IServiceCollection HNBUtilitiesModule(this IServiceCollection services)
    {
        services.AddScoped<GitHubAccessUtilities>();

        return services;
    }


}
