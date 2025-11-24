using HNB.Areas.Backoffice.BackgroundServices.Repositories;
using HNB.Areas.Backoffice.Core;
using HNB.Areas.Backoffice.Repositories;
using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.HNB_WEB.Repositories;
using HNB.Areas.HNB_WEB.Services;
using HNB.Areas.Weather.Repositories;
using HNB.Areas.Weather.Services;
using HNB.IntelligentSystems.ObjectDetection.Api;
using HNB.IntelligentSystems.ObjectDetection.Configuration;
using HNB.IntelligentSystems.ObjectDetection.Module;
using HNB.Repositories;
using HNB.Services;

namespace HNB.Extensions;

public static class ServiceExtensions
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
        services.AddScoped<OrganizationScope>();
        services.AddScoped<TeamZoneService>();
        
        // Weather 區域服務
        services.AddScoped<LoginService>();
        services.AddScoped<ProfileService>();
        
        // 物件辨識配置建立方法（共用）
        ObjectDetectionConfig CreateObjectDetectionConfig(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var storageRoot = configuration["Storage:Root"] ?? "Areas/Backoffice/storage";
            return new ObjectDetectionConfig
            {
                ModelPath = Path.Combine(environment.ContentRootPath, storageRoot, "AI", "groundingdino.onnx"),
                VocabPath = Path.Combine(environment.ContentRootPath, storageRoot, "AI", "vocab.txt"),
                TextPrompt = configuration["ObjectDetection:TextPrompt"] ?? "jacket . clothes . pants . shoes .",
                BoxThreshold = float.TryParse(configuration["ObjectDetection:BoxThreshold"], out var boxThreshold) ? boxThreshold : 0.25f,
                TextThreshold = float.TryParse(configuration["ObjectDetection:TextThreshold"], out var textThreshold) ? textThreshold : 0.2f,
                IncludeLogits = bool.TryParse(configuration["ObjectDetection:IncludeLogits"], out var includeLogits) ? includeLogits : true
            };
        }
        
        // 物件辨識配置與健康檢查服務 - 使用 Singleton 避免重複載入模型
        services.AddSingleton<HNB.IntelligentSystems.ObjectDetection.Core.ModelHealthChecker>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var environment = sp.GetRequiredService<IWebHostEnvironment>();
            var config = CreateObjectDetectionConfig(configuration, environment);
            return new HNB.IntelligentSystems.ObjectDetection.Core.ModelHealthChecker(config);
        });
        
        // 物件辨識模組 - 依賴 ModelHealthChecker（從 DI 容器注入）
        services.AddSingleton<ObjectDetectionModule>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var environment = sp.GetRequiredService<IWebHostEnvironment>();
            var config = CreateObjectDetectionConfig(configuration, environment);
            var healthChecker = sp.GetRequiredService<HNB.IntelligentSystems.ObjectDetection.Core.ModelHealthChecker>();
            return new ObjectDetectionModule(config, healthChecker);
        });
        
        // DallE3 配置建立方法
        HNB.IntelligentSystems.DallE3.Configuration.DallE3Config CreateDallE3Config(IConfiguration configuration)
        {
            return new HNB.IntelligentSystems.DallE3.Configuration.DallE3Config
            {
                ApiKey = configuration["DallE3:ApiKey"] ?? string.Empty,
                BaseUrl = configuration["DallE3:BaseUrl"] ?? "https://api.openai.com/v1",
                Organization = configuration["DallE3:Organization"] ?? string.Empty,
                Size = configuration["DallE3:Size"] ?? "1024x1024",
                Quality = configuration["DallE3:Quality"] ?? "standard",
                ImageModel = configuration["DallE3:ImageModel"] ?? "dall-e-3",
                Background = configuration["DallE3:Background"] ?? "transparent",
                Style = configuration["DallE3:Style"] ?? "natural",
                GridPolicy = configuration["DallE3:GridPolicy"] ?? "auto",
                MaxImagesPerBatch = int.TryParse(configuration["DallE3:MaxImagesPerBatch"], out var maxImages) ? maxImages : 8,
                PreferredImagesPerBatch = int.TryParse(configuration["DallE3:PreferredImagesPerBatch"], out var preferredImages) ? preferredImages : 6
            };
        }
        
        // DallE3 模組 - 使用 Scoped 因為需要使用 IHttpClientFactory
        services.AddScoped<HNB.IntelligentSystems.DallE3.Module.DallE3Module>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var config = CreateDallE3Config(configuration);
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new HNB.IntelligentSystems.DallE3.Module.DallE3Module(config, httpClientFactory);
        });
        
        // QdrantEmbedding 配置建立方法
        HNB.IntelligentSystems.QdrantEmbedding.Configuration.QdrantEmbeddingConfig CreateQdrantEmbeddingConfig(IConfiguration configuration)
        {
            return new HNB.IntelligentSystems.QdrantEmbedding.Configuration.QdrantEmbeddingConfig
            {
                Url = configuration["QdrantEmbedding:Url"] ?? "http://localhost",
                Port = int.TryParse(configuration["QdrantEmbedding:Port"], out var port) ? port : 6333,
                UseHttps = bool.TryParse(configuration["QdrantEmbedding:UseHttps"], out var useHttps) && useHttps,
                ApiKey = configuration["QdrantEmbedding:ApiKey"],
                DefaultCollectionName = configuration["QdrantEmbedding:DefaultCollectionName"] ?? "default",
                DefaultVectorSize = int.TryParse(configuration["QdrantEmbedding:DefaultVectorSize"], out var vectorSize) ? vectorSize : 512,
                DistanceMetric = configuration["QdrantEmbedding:DistanceMetric"] ?? "Cosine",
                DefaultLimit = int.TryParse(configuration["QdrantEmbedding:DefaultLimit"], out var limit) ? limit : 10,
                TimeoutSeconds = int.TryParse(configuration["QdrantEmbedding:TimeoutSeconds"], out var timeout) ? timeout : 30
            };
        }
        
        // QdrantEmbedding 模組 - 使用 Scoped 因為需要使用 IHttpClientFactory
        services.AddScoped<HNB.IntelligentSystems.QdrantEmbedding.Module.QdrantEmbeddingModule>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var config = CreateQdrantEmbeddingConfig(configuration);
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new HNB.IntelligentSystems.QdrantEmbedding.Module.QdrantEmbeddingModule(config, httpClientFactory);
        });
        
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
        services.AddScoped<TeamZoneRepository>();
        
        // Weather 區域倉儲
        services.AddScoped<LoginRepository>();
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
