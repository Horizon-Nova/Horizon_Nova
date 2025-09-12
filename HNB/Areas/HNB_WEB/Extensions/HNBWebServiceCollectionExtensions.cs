using HNB.Areas.HNB_WEB.Repositories;
using HNB.Areas.HNB_WEB.Services;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace HNB.Areas.HNB_WEB.Extensions;

public static class HnbBackofficeExtensions
{

    /// <summary> DI注入管理 業務層/服務層 功能 </summary>
    public static IServiceCollection HNB_WEBServiceModule(this IServiceCollection services)
    {
        services.AddScoped<TeamZoneServices>();
        
        return services;
    }
    /// <summary> DI注入管理 倉儲層 功能 </summary>
    public static IServiceCollection HNB_WEBRepositoriesModule(this IServiceCollection services)
    {
        services.AddScoped<TeamZoneRepositories>();
        return services;
    }
}
