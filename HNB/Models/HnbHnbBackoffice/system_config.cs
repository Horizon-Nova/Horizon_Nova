using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Table("system_config", Schema = "dbo")]
public partial class system_config
{
    [Key]
    public long id { get; set; }

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
    /// 最近活動類型（如：系統啟動、快取更新、維護等）
    /// </summary>
    [StringLength(50)]
    public string? last_activity_type { get; set; }

    /// <summary>
    /// 最近活動的詳細描述
    /// </summary>
    public string? last_activity_description { get; set; }

    /// <summary>
    /// 最近活動發生的時間戳記
    /// </summary>
    public DateTime? last_activity_timestamp { get; set; }

    /// <summary>
    /// 最近活動列表（JSON格式，包含多個活動記錄）
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? recent_activities { get; set; }

    [InverseProperty("system_config")]
    public virtual ICollection<cpu_info> cpu_infos { get; set; } = new List<cpu_info>();

    [InverseProperty("system_config")]
    public virtual ICollection<gpu_info> gpu_infos { get; set; } = new List<gpu_info>();

    [InverseProperty("system_config")]
    public virtual ICollection<memory_info> memory_infos { get; set; } = new List<memory_info>();
}
