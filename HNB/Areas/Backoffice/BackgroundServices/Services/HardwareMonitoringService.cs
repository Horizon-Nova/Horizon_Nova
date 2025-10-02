using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models.HnbHnbBackoffice;
using HNB.Areas.Backoffice.BackgroundServices.Utilities;

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
                last_activity_type = "系統啟動",
                last_activity_description = "硬體監控服務已啟動"
            };

            dbContext.system_configs.Add(config);
            await dbContext.SaveChangesAsync();
        }

        return config;
    }

    private async Task UpdateSystemConfig(HnbHnbBackofficeDbContext dbContext, system_config config)
    {
        // 基本系統資訊（總是更新）
        config.system_status = "運行中";
        config.uptime = GetSystemUptime();
        config.last_activity_type = "硬體監控更新";
        config.last_activity_description = "系統硬體資訊已更新";
        config.last_updated = DateTime.UtcNow;

        // 收集並存入詳細硬體資訊到專門的資料表
        await CollectAndSaveHardwareInfo(dbContext, config);
    }

    /// <summary>
    /// 收集硬體資訊並存入專門的資料表
    /// </summary>
    private async Task CollectAndSaveHardwareInfo(HnbHnbBackofficeDbContext dbContext, system_config config)
    {
        // 收集並存入 CPU 資訊
        await CollectAndSaveCpuInfo(dbContext, config.id);
        
        // 收集並存入 GPU 資訊
        await CollectAndSaveGpuInfo(dbContext, config.id);
        
        // 收集並存入記憶體資訊
        await CollectAndSaveMemoryInfo(dbContext, config.id);
        
        // 更新基本系統溫度資訊到 system_config
        await UpdateSystemTemperatureInfo(config);
        
        // 更新基本電源資訊到 system_config
        await UpdateSystemPowerInfo(config);
    }

    /// <summary>
    /// 收集並存入 CPU 資訊
    /// </summary>
    private async Task CollectAndSaveCpuInfo(HnbHnbBackofficeDbContext dbContext, long systemConfigId)
    {
        try
        {
            // 取得或建立 CPU 資訊記錄
            var cpuInfo = dbContext.cpu_infos.FirstOrDefault(c => c.system_config_id == systemConfigId);
            if (cpuInfo == null)
            {
                cpuInfo = new cpu_info { system_config_id = systemConfigId };
                dbContext.cpu_infos.Add(cpuInfo);
            }

            // 收集 CPU 資訊
            var cpuUsage = await SystemInfoCollector.GetCpuUsageAsync();
            var tempInfo = await SystemInfoCollector.GetSystemTemperatureAsync();

            // 更新 CPU 資訊 - 確保所有欄位都有值
            cpuInfo.model = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "null";
            cpuInfo.manufacturer = GetCpuManufacturer() ?? "null";
            cpuInfo.cores = Environment.ProcessorCount;
            cpuInfo.threads = Environment.ProcessorCount; // 簡化處理
            cpuInfo.current_usage = cpuUsage > 0 ? (decimal)cpuUsage : null;
            cpuInfo.current_temperature = tempInfo.CpuTemperature > 0 ? tempInfo.CpuTemperature : null;
            cpuInfo.health_status = cpuInfo.current_temperature > 80 ? "警告" : "正常";
            cpuInfo.health_percentage = cpuInfo.current_temperature > 0 ? Math.Max(0, 100 - (int)(cpuInfo.current_temperature * 0.8m)) : null;
            cpuInfo.updated_at = DateTime.UtcNow;

            logger.LogDebug($"CPU 資訊已更新 - 使用率: {cpuUsage:F1}%, 溫度: {tempInfo.CpuTemperature}°C");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "更新 CPU 資訊時發生錯誤");
        }
    }

    /// <summary>
    /// 收集並存入 GPU 資訊
    /// </summary>
    private async Task CollectAndSaveGpuInfo(HnbHnbBackofficeDbContext dbContext, long systemConfigId)
    {
        try
        {
            // 取得或建立 GPU 資訊記錄
            var gpuInfo = dbContext.gpu_infos.FirstOrDefault(g => g.system_config_id == systemConfigId);
            if (gpuInfo == null)
            {
                gpuInfo = new gpu_info { system_config_id = systemConfigId };
                dbContext.gpu_infos.Add(gpuInfo);
            }

            // 收集 GPU 資訊
            var gpuData = await SystemInfoCollector.GetGpuInfoAsync();
            var tempInfo = await SystemInfoCollector.GetSystemTemperatureAsync();

            // 更新 GPU 資訊 - 確保所有欄位都有值
            gpuInfo.model = gpuData.Model != "null" ? gpuData.Model : "null";
            gpuInfo.manufacturer = gpuData.Manufacturer != "null" ? gpuData.Manufacturer : "null";
            gpuInfo.memory_size = gpuData.MemorySize != "null" ? gpuData.MemorySize : "null";
            gpuInfo.driver_version = gpuData.DriverVersion != "null" ? gpuData.DriverVersion : "null";
            gpuInfo.current_temperature = tempInfo.GpuTemperature > 0 ? tempInfo.GpuTemperature : null;
            gpuInfo.current_usage = gpuData.UsagePercent > 0 ? gpuData.UsagePercent : null;
            gpuInfo.health_status = gpuInfo.model == "null" ? "未檢測到" : "正常";
            gpuInfo.health_percentage = gpuInfo.model == "null" ? null : 100;
            gpuInfo.updated_at = DateTime.UtcNow;

            logger.LogDebug($"GPU 資訊已更新 - 型號: {gpuData.Model}, 記憶體: {gpuData.MemorySize}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "更新 GPU 資訊時發生錯誤");
        }
    }

    /// <summary>
    /// 收集並存入記憶體資訊
    /// </summary>
    private async Task CollectAndSaveMemoryInfo(HnbHnbBackofficeDbContext dbContext, long systemConfigId)
    {
        try
        {
            // 取得或建立記憶體資訊記錄
            var memoryInfo = dbContext.memory_infos.FirstOrDefault(m => m.system_config_id == systemConfigId);
            if (memoryInfo == null)
            {
                memoryInfo = new memory_info { system_config_id = systemConfigId };
                dbContext.memory_infos.Add(memoryInfo);
            }

            var memoryData = await SystemInfoCollector.GetMemoryUsageAsync();

            var totalMemoryGb = memoryData.UsedGb + memoryData.AvailableGb;
            memoryInfo.total_capacity_gb = (int)totalMemoryGb;
            memoryInfo.available_memory_gb = memoryData.AvailableGb;
            memoryInfo.used_memory_gb = memoryData.UsedGb;
            memoryInfo.current_usage = memoryData.UsagePercent;
            memoryInfo.memory_type = "DDR4";
            memoryInfo.health_status = memoryData.UsagePercent > 90 ? "警告" : "正常";
            memoryInfo.health_percentage = Math.Max(0, 100 - (int)(memoryData.UsagePercent * 0.5m));
            memoryInfo.updated_at = DateTime.UtcNow;

            logger.LogDebug($"記憶體資訊已更新 - 使用率: {memoryData.UsagePercent:F1}%, 總容量: {totalMemoryGb:F1}GB");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "更新記憶體資訊時發生錯誤");
        }
    }

    /// <summary>
    /// 更新系統溫度資訊到 system_config
    /// </summary>
    private async Task UpdateSystemTemperatureInfo(system_config config)
    {
        try
        {
            var tempInfo = await SystemInfoCollector.GetSystemTemperatureAsync();
            
            var cpuTemp = tempInfo.CpuTemperature > 0 ? $"{tempInfo.CpuTemperature}°C" : "null";
            var gpuTemp = tempInfo.GpuTemperature > 0 ? $"{tempInfo.GpuTemperature}°C" : "null";
            var mbTemp = tempInfo.MotherboardTemperature > 0 ? $"{tempInfo.MotherboardTemperature}°C" : "null";
            
            config.power_supply_info = $"CPU: {cpuTemp}, GPU: {gpuTemp}, 主機板: {mbTemp}";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "更新系統溫度資訊時發生錯誤");
            config.power_supply_info = "CPU: null, GPU: null, 主機板: null";
        }
    }

    /// <summary>
    /// 更新系統電源資訊到 system_config
    /// </summary>
    private async Task UpdateSystemPowerInfo(system_config config)
    {
        try
        {
            var powerInfo = await SystemInfoCollector.GetPowerInfoAsync();
            config.power_status = powerInfo.PowerSupplyStatus ?? "null";
            config.battery_level = powerInfo.BatteryLevel > 0 ? powerInfo.BatteryLevel : null;
            config.power_efficiency = powerInfo.BatteryStatus ?? "null";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "更新系統電源資訊時發生錯誤");
            config.power_status = "null";
            config.battery_level = null;
            config.power_efficiency = "null";
        }
    }

    /// <summary>
    /// 取得 CPU 製造商
    /// </summary>
    private string? GetCpuManufacturer()
    {
        var processorId = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
        if (string.IsNullOrEmpty(processorId)) return null;
        
        if (processorId.Contains("Intel", StringComparison.OrdinalIgnoreCase))
            return "Intel";
        if (processorId.Contains("AMD", StringComparison.OrdinalIgnoreCase))
            return "AMD";
        if (processorId.Contains("ARM", StringComparison.OrdinalIgnoreCase))
            return "ARM";
            
        return "未知";
    }

    /// <summary>
    /// 更新伺服器規格字串中的特定項目
    /// </summary>
    private string UpdateServerSpecs(string currentSpecs, string key, string value)
    {
        var specs = new Dictionary<string, string>();
        
        // 解析現有規格
        if (!string.IsNullOrEmpty(currentSpecs))
        {
            var parts = currentSpecs.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var keyValue = part.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length == 2)
                {
                    specs[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }
        }
        
        // 更新特定項目
        specs[key] = value;
        
        // 重新組合
        return string.Join(", ", specs.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
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
