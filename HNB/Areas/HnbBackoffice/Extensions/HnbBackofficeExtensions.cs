using Microsoft.Extensions.DependencyInjection;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using HNB.Areas.HnbBackoffice.Repositories;
using HNB.Areas.HnbBackoffice.Services;

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

}
