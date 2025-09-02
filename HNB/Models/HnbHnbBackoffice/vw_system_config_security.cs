using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_system_config_security
{
    public long? id { get; set; }

    /// <summary>
    /// 會話逾時 (秒)
    /// </summary>
    [StringLength(10)]
    public string? session_timeout { get; set; }

    /// <summary>
    /// 最大登入嘗試次數
    /// </summary>
    [StringLength(10)]
    public string? max_login_attempts { get; set; }

    /// <summary>
    /// 雙因子驗證
    /// </summary>
    public bool? two_factor_auth { get; set; }

    /// <summary>
    /// IP 白名單
    /// </summary>
    [Column(TypeName = "json")]
    public string? ip_whitelist { get; set; }
}
