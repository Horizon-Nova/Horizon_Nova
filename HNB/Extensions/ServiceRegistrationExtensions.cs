using HNB.Areas.Backoffice.BackgroundServices;
using HNB.Areas.Backoffice.BackgroundServices.Middleware;
using HNB.Areas.Backoffice.BackgroundServices.Repositories;
using HNB.Areas.Backoffice.Repositories;
using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Utilities;
using HNB.IntelligentSystems.ObjectDetection.Api;
using HNB.IntelligentSystems.ObjectDetection.Configuration;
using HNB.IntelligentSystems.ObjectDetection.Module;
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
        services.AddScoped<FileManagerServices>();
        services.AddScoped<PermissionManagementService>();
        services.AddScoped<AuthService>();
        services.AddScoped<SidebarNavigationService>();
        services.AddScoped<DatabaseService>();
        
        // 物件辨識服務 - 使用 Singleton 避免重複載入模型
        services.AddSingleton<ObjectDetectionModule>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var environment = sp.GetRequiredService<IWebHostEnvironment>();
            var storageRoot = configuration["Storage:Root"] ?? "Areas/Backoffice/storage";
            var config = new ObjectDetectionConfig
            {
                ModelPath = Path.Combine(environment.ContentRootPath, storageRoot, "AI", "groundingdino.onnx"),
                VocabPath = Path.Combine(environment.ContentRootPath, storageRoot, "AI", "vocab.txt"),
                TextPrompt = configuration["ObjectDetection:TextPrompt"] ?? "jacket . clothes . pants . shoes .",
                BoxThreshold = float.TryParse(configuration["ObjectDetection:BoxThreshold"], out var boxThreshold) ? boxThreshold : 0.25f,
                TextThreshold = float.TryParse(configuration["ObjectDetection:TextThreshold"], out var textThreshold) ? textThreshold : 0.2f,
                IncludeLogits = bool.TryParse(configuration["ObjectDetection:IncludeLogits"], out var includeLogits) ? includeLogits : true
            };
            return new ObjectDetectionModule(config);
        });
        
        services.AddScoped<DallE3Service>();
        services.AddScoped<ClothingAIService>();
        return services;
    }
    /// <summary> DI注入管理 Repositories 功能 </summary>
    public static IServiceCollection RepositoriesModule(this IServiceCollection services)
    {
        services.AddScoped<PermissionManagementRepository>();
        services.AddScoped<SettingsRepositories>();
        services.AddScoped<AuthRepository>();
        services.AddScoped<SidebarNavigationRepository>();
        services.AddScoped<HardwareMonitoringRepository>();
        services.AddScoped<BlockedIpRepository>();
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
