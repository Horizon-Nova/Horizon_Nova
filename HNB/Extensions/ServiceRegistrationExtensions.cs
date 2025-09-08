using HNB.Areas.HnbBackoffice.BackgroundServices;
using HNB.Areas.HnbBackoffice.Repositories;
using HNB.Areas.HnbBackoffice.Services;
using HNB.Repositories;
//using HNB.Repositories;
using HNB.Services;
using Microsoft.Extensions.DependencyInjection;
using Models.HnbHnbBackoffice;

//using HNB.BackSystem;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
//using HNB.Repositories;
//using HNB.Services;

namespace HNB.Extensions;

public static class ServiceRegistrationExtensions
{
    /// <summary> DI注入管理 GitHubAccess 功能 </summary>
    public static IServiceCollection AddGitHubAccessModule(this IServiceCollection services)
    {
        services.AddScoped<GitHubAccessServices>();
        return services;
    }
    /// <summary> DI注入管理 IpMiddlewareServices 功能 </summary>
    public static IServiceCollection AddIpMiddlewareServicesModule(this IServiceCollection services)
    {
        services.AddScoped<IpMiddlewareServices>();
        return services;
    }
    /// <summary> DI注入管理 ErrorLogService 功能 </summary>
    public static IServiceCollection AddErrorLogServiceModule(this IServiceCollection services)
    {
        services.AddScoped<ErrorLogService>();
        services.AddScoped<ErrorLogRepository>();
        return services;
    }



}
