using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

/// <summary>
/// 記憶體硬體資訊表 - 儲存記憶體容量、速度、使用率、健康度等資訊
/// </summary>
[Table("memory_info", Schema = "dbo")]
public partial class memory_info
{
    [Key]
    public long id { get; set; }

    public long? system_config_id { get; set; }

    /// <summary>
    /// 記憶體總容量
    /// </summary>
    [StringLength(20)]
    public string? total_capacity { get; set; }

    /// <summary>
    /// 記憶體總容量(GB)
    /// </summary>
    public int? total_capacity_gb { get; set; }

    /// <summary>
    /// 記憶體速度
    /// </summary>
    [StringLength(20)]
    public string? speed { get; set; }

    /// <summary>
    /// 記憶體通道數
    /// </summary>
    public int? channels { get; set; }

    /// <summary>
    /// 記憶體類型
    /// </summary>
    [StringLength(20)]
    public string? memory_type { get; set; }

    /// <summary>
    /// 記憶體目前使用率(%)
    /// </summary>
    [Precision(5, 2)]
    public decimal? current_usage { get; set; }

    /// <summary>
    /// 可用記憶體(GB)
    /// </summary>
    [Precision(8, 2)]
    public decimal? available_memory_gb { get; set; }

    /// <summary>
    /// 已使用記憶體(GB)
    /// </summary>
    [Precision(8, 2)]
    public decimal? used_memory_gb { get; set; }

    /// <summary>
    /// 記憶體健康狀態
    /// </summary>
    [StringLength(20)]
    public string? health_status { get; set; }

    /// <summary>
    /// 記憶體健康度百分比
    /// </summary>
    public int? health_percentage { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    [ForeignKey("system_config_id")]
    [InverseProperty("memory_infos")]
    public virtual system_config? system_config { get; set; }
}
