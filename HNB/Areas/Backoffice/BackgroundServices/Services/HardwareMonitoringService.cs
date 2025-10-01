using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.BackgroundServices.Services;

/// <summary>
/// 硬體監控背景服務
/// 定期收集系統硬體資訊並更新到資料庫
/// </summary>
public class HardwareMonitoringService(IServiceProvider serviceProvider, ILogger<HardwareMonitoringService> logger) : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("硬體監控服務已啟動");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectAndUpdateHardwareInfo();
                logger.LogInformation("硬體資訊收集完成");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "硬體監控服務執行時發生錯誤");
            }

            await Task.Delay(_period, stoppingToken);
        }

        logger.LogInformation("硬體監控服務已停止");
    }

    private async Task CollectAndUpdateHardwareInfo()
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HnbHnbBackofficeDbContext>();

        try
        {
            var systemConfig = await GetOrCreateSystemConfig(dbContext);
            await UpdateSystemConfig(dbContext, systemConfig);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("硬體資訊已成功更新到資料庫");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新硬體資訊到資料庫時發生錯誤");
        }
    }

    private async Task<system_config> GetOrCreateSystemConfig(HnbHnbBackofficeDbContext dbContext)
    {
        var config = dbContext.system_configs.FirstOrDefault();
        
        if (config == null)
        {
            config = new system_config
            {
                system_status = "運行中",
                system_version = Environment.OSVersion.VersionString,
                app_version = "1.0.0",
                database_version = "PostgreSQL 18",
                server_ip = GetLocalIPAddress(),
                server_location = "台灣",
                server_provider = "自建",
                environment_type = "Production",
                host_name = Environment.MachineName,
                operating_system = Environment.OSVersion.VersionString,
                kernel_version = Environment.OSVersion.Version.ToString(),
                uptime = GetSystemUptime(),
                server_specs = GetServerSpecs(),
                power_status = "正常",
                power_supply_info = "AC 電源",
                battery_level = null,
                power_efficiency = "高效",
                last_updated = DateTime.UtcNow,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                last_activity_type = "系統啟動",
                last_activity_description = "硬體監控服務已啟動",
                last_activity_timestamp = DateTime.UtcNow
            };

            dbContext.system_configs.Add(config);
            await dbContext.SaveChangesAsync();
        }

        return config;
    }

    private async Task UpdateSystemConfig(HnbHnbBackofficeDbContext dbContext, system_config config)
    {
        config.system_status = "運行中";
        config.last_updated = DateTime.UtcNow;
        config.uptime = GetSystemUptime();
        config.last_activity_type = "硬體監控更新";
        config.last_activity_description = "系統硬體資訊已更新";
        config.last_activity_timestamp = DateTime.UtcNow;
        config.updated_at = DateTime.UtcNow;
    }

    private string GetLocalIPAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch { }
        return "未知";
    }

    private string GetSystemUptime()
    {
        try
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return $"{uptime.Days}天 {uptime.Hours}小時 {uptime.Minutes}分鐘";
        }
        catch
        {
            return "未知";
        }
    }

    private string GetServerSpecs()
    {
        try
        {
            return $"CPU: {Environment.ProcessorCount}核心, 記憶體: {GC.GetTotalMemory(false) / (1024 * 1024)}MB";
        }
        catch
        {
            return "未知";
        }
    }
}
