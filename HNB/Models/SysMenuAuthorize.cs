using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 選單權限表
/// </summary>
public partial class SysMenuAuthorize
{
    public long Id { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public long BaseCreatorId { get; set; }

    /// <summary>
    /// 選單 ID
    /// </summary>
    public long MenuId { get; set; }

    /// <summary>
    /// 授權對象 ID
    /// </summary>
    public long AuthorizeId { get; set; }

    /// <summary>
    /// 授權類型 (1 角色 2 使用者)
    /// </summary>
    public int AuthorizeType { get; set; }
}
