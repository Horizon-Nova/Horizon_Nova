using Microsoft.Extensions.DependencyInjection;
//using HNB.Repositories;
using HNB.Services;
using HNB.Utilities;
//using HNB.BackSystem;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

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

}
