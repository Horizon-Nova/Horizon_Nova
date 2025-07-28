using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 部門表
/// </summary>
public partial class SysDepartment
{
    public long Id { get; set; }

    public int BaseIsDelete { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public DateTime BaseModifyTime { get; set; }

    public long BaseCreatorId { get; set; }

    public long BaseModifierId { get; set; }

    public int BaseVersion { get; set; }

    /// <summary>
    /// 父部門 ID (0 表示根部門)
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 部門名稱
    /// </summary>
    public string DepartmentName { get; set; } = null!;

    /// <summary>
    /// 電話
    /// </summary>
    public string Telephone { get; set; } = null!;

    /// <summary>
    /// 傳真
    /// </summary>
    public string Fax { get; set; } = null!;

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// 負責人 ID
    /// </summary>
    public long PrincipalId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int DepartmentSort { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;
}
