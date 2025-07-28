using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 角色表
/// </summary>
public partial class SysRole
{
    public long Id { get; set; }

    public int BaseIsDelete { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public DateTime BaseModifyTime { get; set; }

    public long BaseCreatorId { get; set; }

    public long BaseModifierId { get; set; }

    public int BaseVersion { get; set; }

    /// <summary>
    /// 角色名稱
    /// </summary>
    public string RoleName { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int RoleSort { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public int RoleStatus { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;
}
