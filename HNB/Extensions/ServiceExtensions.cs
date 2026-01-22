using HNB.Areas.Backoffice.Core;
using HNB.Areas.Backoffice.Repositories;
using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.HNB_WEB.Repositories;
using HNB.Areas.HNB_WEB.Services;
using HNB.IntelligentSystems.Embedding.Module;
using HNB.IntelligentSystems.GroundingDINO.Module;
using HNB.Repositories;
using HNB.Services;

namespace HNB.Extensions;

public static class ServiceExtensions
{
    /// <summary> DI注入管理 Services 功能 </summary>
    public static IServiceCollection ServicesModule(this IServiceCollection services)
    {
        services.AddScoped<ErrorLogService>();
        services.AddScoped<SettingsServices>();
        services.AddScoped<IpMiddlewareServices>();
        services.AddScoped<FileManagerServices>();
        services.AddScoped<PermissionManagementService>();
        services.AddScoped<AuthService>();
        services.AddScoped<SidebarNavigationService>();
        services.AddScoped<DatabaseService>();
        services.AddScoped<OrganizationScope>();
        services.AddScoped<TeamZoneService>();
        
        // AI 模組已停用 - 為節省雲端記憶體成本
        // GroundingDINO 物件檢測模組 - 使用 Singleton 避免重複載入模型
        // services.AddSingleton<HNB.IntelligentSystems.GroundingDINO.Core.ModelHealthChecker>();
        // services.AddSingleton<GroundingDINOModule>();
        
        // DallE3 模組
        // services.AddScoped<HNB.IntelligentSystems.DallE3.Module.DallE3Module>();
        
        // Qdrant 模組
        // services.AddScoped<HNB.IntelligentSystems.Qdrant.Module.QdrantModule>();
        
        // Embedding 模組 - 支援切換向量模型
        // services.AddSingleton<EmbeddingModule>();
        
        return services;
    }
    /// <summary> DI注入管理 Repositories 功能 </summary>
    public static IServiceCollection RepositoriesModule(this IServiceCollection services)
    {
        services.AddScoped<PermissionManagementRepository>();
        services.AddScoped<SettingsRepositories>();
        services.AddScoped<AuthRepository>();
        services.AddScoped<SidebarNavigationRepository>();
        services.AddScoped<BlockedIpRepository>();
        services.AddScoped<TeamZoneRepository>();
        
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
