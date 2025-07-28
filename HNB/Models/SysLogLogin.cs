using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 登入日誌表
/// </summary>
public partial class SysLogLogin
{
    public long Id { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public long BaseCreatorId { get; set; }

    /// <summary>
    /// 執行狀態 (0 失敗 1 成功)
    /// </summary>
    public int LogStatus { get; set; }

    /// <summary>
    /// IP 位址
    /// </summary>
    public string IpAddress { get; set; } = null!;

    /// <summary>
    /// IP 位置
    /// </summary>
    public string IpLocation { get; set; } = null!;

    /// <summary>
    /// 瀏覽器
    /// </summary>
    public string Browser { get; set; } = null!;

    /// <summary>
    /// 作業系統
    /// </summary>
    public string Os { get; set; } = null!;

    /// <summary>
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 額外備註
    /// </summary>
    public string ExtraRemark { get; set; } = null!;
}
