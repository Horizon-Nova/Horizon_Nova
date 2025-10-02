using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

/// <summary>
/// GPU硬體資訊表 - 儲存GPU詳細規格、溫度、使用率、健康度等資訊
/// </summary>
[Table("gpu_info", Schema = "dbo")]
public partial class gpu_info
{
    [Key]
    public long id { get; set; }

    public long? system_config_id { get; set; }

    /// <summary>
    /// GPU型號
    /// </summary>
    [StringLength(200)]
    public string? model { get; set; }

    /// <summary>
    /// GPU製造商
    /// </summary>
    [StringLength(100)]
    public string? manufacturer { get; set; }

    /// <summary>
    /// GPU記憶體大小
    /// </summary>
    [StringLength(20)]
    public string? memory_size { get; set; }

    /// <summary>
    /// GPU記憶體類型
    /// </summary>
    [StringLength(20)]
    public string? memory_type { get; set; }

    /// <summary>
    /// GPU基礎時脈(MHz)
    /// </summary>
    public int? base_clock { get; set; }

    /// <summary>
    /// GPU加速時脈(MHz)
    /// </summary>
    public int? boost_clock { get; set; }

    /// <summary>
    /// GPU記憶體時脈(MHz)
    /// </summary>
    public int? memory_clock { get; set; }

    /// <summary>
    /// GPU驅動程式版本
    /// </summary>
    [StringLength(50)]
    public string? driver_version { get; set; }

    /// <summary>
    /// GPU CUDA核心數
    /// </summary>
    public int? cuda_cores { get; set; }

    /// <summary>
    /// GPU目前溫度(°C)
    /// </summary>
    public int? current_temperature { get; set; }

    /// <summary>
    /// GPU最高溫度(°C)
    /// </summary>
    public int? max_temperature { get; set; }

    /// <summary>
    /// GPU目前使用率(%)
    /// </summary>
    [Precision(5, 2)]
    public decimal? current_usage { get; set; }

    /// <summary>
    /// GPU目前功耗(W)
    /// </summary>
    public int? current_power_consumption { get; set; }

    /// <summary>
    /// GPU健康狀態
    /// </summary>
    [StringLength(20)]
    public string? health_status { get; set; }

    /// <summary>
    /// GPU健康度百分比
    /// </summary>
    public int? health_percentage { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    [ForeignKey("system_config_id")]
    [InverseProperty("gpu_infos")]
    public virtual system_config? system_config { get; set; }
}
