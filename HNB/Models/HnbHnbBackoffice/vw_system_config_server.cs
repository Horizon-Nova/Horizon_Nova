using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_system_config_server
{
    public long? id { get; set; }

    /// <summary>
    /// 主機名稱
    /// </summary>
    [StringLength(128)]
    public string? host_name { get; set; }

    /// <summary>
    /// 作業系統
    /// </summary>
    [StringLength(128)]
    public string? operating_system { get; set; }

    /// <summary>
    /// 核心版本
    /// </summary>
    [StringLength(128)]
    public string? kernel_version { get; set; }

    /// <summary>
    /// 系統運行時間
    /// </summary>
    [StringLength(128)]
    public string? uptime { get; set; }

    /// <summary>
    /// 入站流量
    /// </summary>
    [StringLength(64)]
    public string? inbound_traffic { get; set; }

    /// <summary>
    /// 出站流量
    /// </summary>
    [StringLength(64)]
    public string? outbound_traffic { get; set; }

    /// <summary>
    /// CPU 使用率
    /// </summary>
    public List<string>? cpu_usage { get; set; }

    /// <summary>
    /// 記憶體使用率
    /// </summary>
    public List<string>? memory_usage { get; set; }

    /// <summary>
    /// 磁碟使用率
    /// </summary>
    public List<string>? disk_usage { get; set; }
}
