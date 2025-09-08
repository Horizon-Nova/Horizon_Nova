using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Table("security_ip_keys", Schema = "dbo")]
public partial class security_ip_key
{
    [Key]
    public long id { get; set; }

    /// <summary>
    /// IPv4/IPv6
    /// </summary>
    public IPAddress ip_addr { get; set; } = null!;

    /// <summary>
    /// 單一字串
    /// </summary>
    [Column(TypeName = "character varying")]
    public string key_components { get; set; } = null!;

    /// <summary>
    /// 基準時間
    /// </summary>
    public long time_param { get; set; }

    public DateTime created_at { get; set; }

    /// <summary>
    /// 失效時間
    /// </summary>
    public DateTime expires_at { get; set; }

    public string? note { get; set; }

    /// <summary>
    /// DB 自動產生
    /// </summary>
    public string? key { get; set; }

    public bool disabled { get; set; }
}
