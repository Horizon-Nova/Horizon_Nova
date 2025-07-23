using Microsoft.Extensions.DependencyInjection;
//using HNB.Repositories;
using HNB.Services;
using HNB.Utilities;
//using HNB.BackSystem;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using HNB.Areas.HNB_WEB.Repositories;
using HNB.Areas.HNB_WEB.Services;

namespace HNB.Areas.HNB_WEB.Extensions;

public static class HNBWebServiceCollectionExtensions
{
    /// <summary> DI注入管理 Home 功能 </summary>
    public static IServiceCollection AddHomeModule(this IServiceCollection services)
    {
        services.AddScoped<HomeServices>();
        services.AddScoped<HomeRepositories>();
        return services;
    }

}
