using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 使用者歸屬表
/// </summary>
public partial class SysUserBelong
{
    public long Id { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public long BaseCreatorId { get; set; }

    /// <summary>
    /// 使用者 ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 歸屬 ID
    /// </summary>
    public long BelongId { get; set; }

    /// <summary>
    /// 歸屬類型 (1 職位 2 角色)
    /// </summary>
    public int BelongType { get; set; }
}
