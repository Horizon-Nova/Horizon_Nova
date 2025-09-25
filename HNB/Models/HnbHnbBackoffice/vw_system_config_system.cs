using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_system_config_system
{
    public long? id { get; set; }

    /// <summary>
    /// 網站名稱
    /// </summary>
    [StringLength(255)]
    public string? website_name { get; set; }

    /// <summary>
    /// 網站網址
    /// </summary>
    [StringLength(255)]
    public string? website_url { get; set; }

    /// <summary>
    /// 管理員信箱
    /// </summary>
    [StringLength(255)]
    public string? admin_email { get; set; }

    /// <summary>
    /// 時區設定
    /// </summary>
    [StringLength(64)]
    public string? timezone { get; set; }

    /// <summary>
    /// 維護模式
    /// </summary>
    public bool? maintenance_mode { get; set; }

    /// <summary>
    /// 偵錯模式
    /// </summary>
    public bool? debug_mode { get; set; }
}