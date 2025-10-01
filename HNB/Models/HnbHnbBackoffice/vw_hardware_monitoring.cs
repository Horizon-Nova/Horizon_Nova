using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_hardware_monitoring
{
    public long? system_id { get; set; }

    [StringLength(100)]
    public string? admin_email { get; set; }

    [StringLength(50)]
    public string? timezone { get; set; }

    public bool? maintenance_mode { get; set; }

    public bool? debug_mode { get; set; }

    [StringLength(50)]
    public string? system_status { get; set; }

    [StringLength(50)]
    public string? system_version { get; set; }

    [StringLength(50)]
    public string? app_version { get; set; }

    [StringLength(50)]
    public string? database_version { get; set; }

    [StringLength(50)]
    public string? server_ip { get; set; }

    [StringLength(100)]
    public string? server_location { get; set; }

    [StringLength(100)]
    public string? server_provider { get; set; }

    [StringLength(20)]
    public string? environment_type { get; set; }

    [StringLength(100)]
    public string? host_name { get; set; }

    [StringLength(200)]
    public string? operating_system { get; set; }

    [StringLength(100)]
    public string? kernel_version { get; set; }

    [StringLength(100)]
    public string? uptime { get; set; }

    public string? server_specs { get; set; }

    [StringLength(50)]
    public string? power_status { get; set; }

    public string? power_supply_info { get; set; }

    public int? battery_level { get; set; }

    [StringLength(20)]
    public string? power_efficiency { get; set; }

    public DateTime? last_updated { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    /// <summary>
    /// 最近活動類型
    /// </summary>
    [StringLength(50)]
    public string? last_activity_type { get; set; }

    /// <summary>
    /// 最近活動描述
    /// </summary>
    public string? last_activity_description { get; set; }

    /// <summary>
    /// 最近活動時間
    /// </summary>
    public DateTime? last_activity_timestamp { get; set; }

    /// <summary>
    /// 最近活動列表（JSON格式）
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? recent_activities { get; set; }

    public long? cpu_id { get; set; }

    [StringLength(200)]
    public string? cpu_model { get; set; }

    [StringLength(100)]
    public string? cpu_manufacturer { get; set; }

    public int? cpu_cores { get; set; }

    public int? cpu_threads { get; set; }

    [Precision(4, 2)]
    public decimal? cpu_base_clock { get; set; }

    [Precision(4, 2)]
    public decimal? cpu_boost_clock { get; set; }

    [StringLength(20)]
    public string? cpu_cache { get; set; }

    [StringLength(50)]
    public string? cpu_architecture { get; set; }

    [StringLength(50)]
    public string? cpu_socket { get; set; }

    public int? cpu_tdp { get; set; }

    public int? cpu_temperature { get; set; }

    public int? cpu_max_temperature { get; set; }

    [Precision(5, 2)]
    public decimal? cpu_usage_percent { get; set; }

    public int? cpu_power_consumption { get; set; }

    [StringLength(20)]
    public string? cpu_health_status { get; set; }

    public int? cpu_health_percentage { get; set; }

    public DateTime? cpu_created_at { get; set; }

    public DateTime? cpu_updated_at { get; set; }

    public long? gpu_id { get; set; }

    [StringLength(200)]
    public string? gpu_model { get; set; }

    [StringLength(100)]
    public string? gpu_manufacturer { get; set; }

    [StringLength(20)]
    public string? gpu_memory_size { get; set; }

    [StringLength(20)]
    public string? gpu_memory_type { get; set; }

    public int? gpu_base_clock { get; set; }

    public int? gpu_boost_clock { get; set; }

    public int? gpu_memory_clock { get; set; }

    [StringLength(50)]
    public string? gpu_driver_version { get; set; }

    public int? gpu_cuda_cores { get; set; }

    public int? gpu_temperature { get; set; }

    public int? gpu_max_temperature { get; set; }

    [Precision(5, 2)]
    public decimal? gpu_usage_percent { get; set; }

    public int? gpu_power_consumption { get; set; }

    [StringLength(20)]
    public string? gpu_health_status { get; set; }

    public int? gpu_health_percentage { get; set; }

    public DateTime? gpu_created_at { get; set; }

    public DateTime? gpu_updated_at { get; set; }

    public long? memory_id { get; set; }

    [StringLength(20)]
    public string? memory_total_capacity { get; set; }

    public int? memory_total_capacity_gb { get; set; }

    [StringLength(20)]
    public string? memory_speed { get; set; }

    public int? memory_channels { get; set; }

    [StringLength(20)]
    public string? memory_type { get; set; }

    [Precision(5, 2)]
    public decimal? memory_usage_percent { get; set; }

    [Precision(8, 2)]
    public decimal? memory_available_gb { get; set; }

    [Precision(8, 2)]
    public decimal? memory_used_gb { get; set; }

    [StringLength(20)]
    public string? memory_health_status { get; set; }

    public int? memory_health_percentage { get; set; }

    public DateTime? memory_created_at { get; set; }

    public DateTime? memory_updated_at { get; set; }

    [Column(TypeName = "json")]
    public string? storage_devices { get; set; }
}
