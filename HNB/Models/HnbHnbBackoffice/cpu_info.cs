using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

/// <summary>
/// CPU硬體資訊表 - 儲存CPU詳細規格、溫度、使用率、健康度等資訊
/// </summary>
[Table("cpu_info", Schema = "dbo")]
public partial class cpu_info
{
    [Key]
    public long id { get; set; }

    public long? system_config_id { get; set; }

    /// <summary>
    /// CPU型號
    /// </summary>
    [StringLength(200)]
    public string? model { get; set; }

    /// <summary>
    /// CPU製造商
    /// </summary>
    [StringLength(100)]
    public string? manufacturer { get; set; }

    /// <summary>
    /// CPU核心數
    /// </summary>
    public int? cores { get; set; }

    /// <summary>
    /// CPU執行緒數
    /// </summary>
    public int? threads { get; set; }

    /// <summary>
    /// CPU基礎時脈(GHz)
    /// </summary>
    [Precision(4, 2)]
    public decimal? base_clock { get; set; }

    /// <summary>
    /// CPU加速時脈(GHz)
    /// </summary>
    [Precision(4, 2)]
    public decimal? boost_clock { get; set; }

    /// <summary>
    /// CPU快取大小
    /// </summary>
    [StringLength(20)]
    public string? cache { get; set; }

    /// <summary>
    /// CPU架構
    /// </summary>
    [StringLength(50)]
    public string? architecture { get; set; }

    /// <summary>
    /// CPU插槽類型
    /// </summary>
    [StringLength(50)]
    public string? socket { get; set; }

    /// <summary>
    /// CPU熱設計功耗(W)
    /// </summary>
    public int? tdp { get; set; }

    /// <summary>
    /// CPU目前溫度(°C)
    /// </summary>
    public int? current_temperature { get; set; }

    /// <summary>
    /// CPU最高溫度(°C)
    /// </summary>
    public int? max_temperature { get; set; }

    /// <summary>
    /// CPU目前使用率(%)
    /// </summary>
    [Precision(5, 2)]
    public decimal? current_usage { get; set; }

    /// <summary>
    /// CPU目前功耗(W)
    /// </summary>
    public int? current_power_consumption { get; set; }

    /// <summary>
    /// CPU健康狀態
    /// </summary>
    [StringLength(20)]
    public string? health_status { get; set; }

    /// <summary>
    /// CPU健康度百分比
    /// </summary>
    public int? health_percentage { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    [ForeignKey("system_config_id")]
    [InverseProperty("cpu_infos")]
    public virtual system_config? system_config { get; set; }
}
