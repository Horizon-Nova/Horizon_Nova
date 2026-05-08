namespace HNB.Areas.Backoffice.Dtos;

/// <summary>
/// 硬體資訊資料傳輸物件
/// </summary>
public class HardwareInfoDto
{
    public string? server_ip { get; set; }
    public string? server_location { get; set; }
    public string? server_provider { get; set; }
    public string? environment_type { get; set; }
    
    public string? host_name { get; set; }
    public string? operating_system { get; set; }
    public string? kernel_version { get; set; }
    public string? uptime { get; set; }
    
    public string? cpu_name { get; set; }
    public string? cpu_manufacturer { get; set; }
    public string? cpu_model { get; set; }
    public int? cpu_cores { get; set; }
    public int? cpu_threads { get; set; }
    public decimal? cpu_base_clock { get; set; }
    public decimal? cpu_boost_clock { get; set; }
    public int? cpu_temperature { get; set; }
    public decimal? cpu_usage_percent { get; set; }
    public string? cpu_health_status { get; set; }
    public int? cpu_health_percentage { get; set; }
    
    public string? gpu_name { get; set; }
    public string? gpu_manufacturer { get; set; }
    public string? gpu_model { get; set; }
    public string? gpu_memory_size { get; set; }
    public int? gpu_temperature { get; set; }
    public decimal? gpu_usage_percent { get; set; }
    public string? gpu_health_status { get; set; }
    public int? gpu_health_percentage { get; set; }
    
    public string? memory_name { get; set; }
    public string? memory_type { get; set; }
    public string? memory_total_capacity { get; set; }
    public string? memory_speed { get; set; }
    public decimal? memory_usage_percent { get; set; }
    public string? memory_health_status { get; set; }
    public int? memory_health_percentage { get; set; }
    
    public long? system_memory_total { get; set; }
    public long? system_memory_used { get; set; }
    public long? system_memory_free { get; set; }
    public long? memory_total_capacity_gb { get; set; }
    public long? memory_used_gb { get; set; }
    public long? memory_available_gb { get; set; }
    
    public decimal? system_load_avg { get; set; }
    public int? system_processes { get; set; }
    public int? system_users { get; set; }
    
    public int? battery_level { get; set; }
    public string? power_efficiency { get; set; }
    public string? power_supply_info { get; set; }
    
    public DateTime? last_check_time { get; set; }
    public string? check_method { get; set; }
    public int? check_interval { get; set; }
    public bool? is_active { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
}

/// <summary>
/// 硬體收集輔助類別（內部使用，用於收集過程）
/// </summary>
public class HardwareCollector
{
    public string? server_ip { get; set; }
    public string? server_location { get; set; }
    public string? server_provider { get; set; }
    public string? environment_type { get; set; }
    
    public string? host_name { get; set; }
    public string? operating_system { get; set; }
    public string? kernel_version { get; set; }
    public string? uptime { get; set; }
    
    public List<string>? cpu_names { get; set; }
    public List<string>? cpu_manufacturers { get; set; }
    public List<string>? cpu_models { get; set; }
    public List<int>? cpu_cores { get; set; }
    public List<int>? cpu_threads { get; set; }
    public List<decimal>? cpu_base_clocks { get; set; }
    public List<decimal>? cpu_boost_clocks { get; set; }
    public List<decimal>? cpu_usages { get; set; }
    public List<int>? cpu_temperatures { get; set; }
    public List<string>? cpu_health_statuses { get; set; }
    public List<int>? cpu_health_percentages { get; set; }
    
    public List<string>? gpu_names { get; set; }
    public List<string>? gpu_manufacturers { get; set; }
    public List<string>? gpu_models { get; set; }
    public List<string>? gpu_memory_sizes { get; set; }
    public List<int>? gpu_temperatures { get; set; }
    public List<decimal>? gpu_usages { get; set; }
    public List<string>? gpu_health_statuses { get; set; }
    public List<int>? gpu_health_percentages { get; set; }
    
    public List<string>? memory_names { get; set; }
    public List<string>? memory_types { get; set; }
    public List<string>? memory_capacities { get; set; }
    public List<string>? memory_speeds { get; set; }
    public List<decimal>? memory_usages { get; set; }
    public List<string>? memory_health_statuses { get; set; }
    public List<int>? memory_health_percentages { get; set; }
    
    public long? system_memory_total { get; set; }
    public long? system_memory_used { get; set; }
    public long? system_memory_free { get; set; }
    
    public List<string>? storage_names { get; set; }
    public List<string>? storage_types { get; set; }
    public List<string>? storage_capacities { get; set; }
    public List<string>? storage_interfaces { get; set; }
    public List<decimal>? storage_read_speeds { get; set; }
    public List<decimal>? storage_write_speeds { get; set; }
    
    public long? system_disk_total { get; set; }
    public long? system_disk_used { get; set; }
    public long? system_disk_free { get; set; }
    public long? system_swap_total { get; set; }
    public long? system_swap_used { get; set; }
    
    public List<string>? network_interfaces { get; set; }
    public List<string>? network_types { get; set; }
    public List<string>? network_speeds { get; set; }
    public List<string>? network_statuses { get; set; }
    public List<long>? network_rx_bytes { get; set; }
    public List<long>? network_tx_bytes { get; set; }
    
    public List<decimal>? system_load_avg { get; set; }
    public int? system_processes { get; set; }
    public int? system_users { get; set; }
    
    public int? battery_level { get; set; }
    public string? power_efficiency { get; set; }
    public string? power_supply_info { get; set; }
    
    public DateTime? last_check_time { get; set; }
    public string? check_method { get; set; }
    public int? check_interval { get; set; }
    public bool? is_active { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
}
