using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 資料字典類型表
/// </summary>
public partial class SysDataDict
{
    public long Id { get; set; }

    public int BaseIsDelete { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public DateTime BaseModifyTime { get; set; }

    public long BaseCreatorId { get; set; }

    public long BaseModifierId { get; set; }

    public int BaseVersion { get; set; }

    /// <summary>
    /// 字典類型
    /// </summary>
    public string DictType { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int DictSort { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;
}
