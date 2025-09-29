using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_system_config_database
{
    public long? id { get; set; }

    /// <summary>
    /// 主機位址
    /// </summary>
    [StringLength(255)]
    public string? db_host { get; set; }

    /// <summary>
    /// 連接埠
    /// </summary>
    [StringLength(10)]
    public string? db_port { get; set; }

    /// <summary>
    /// 資料庫名稱
    /// </summary>
    [StringLength(128)]
    public string? db_name { get; set; }

    /// <summary>
    /// 備份頻率
    /// </summary>
    [StringLength(50)]
    public string? backup_frequency { get; set; }

    /// <summary>
    /// 啟用自動備份
    /// </summary>
    public bool? auto_backup_enabled { get; set; }
}
