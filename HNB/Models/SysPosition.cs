using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 職位表
/// </summary>
public partial class SysPosition
{
    public long Id { get; set; }

    public int BaseIsDelete { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public DateTime BaseModifyTime { get; set; }

    public long BaseCreatorId { get; set; }

    public long BaseModifierId { get; set; }

    public int BaseVersion { get; set; }

    /// <summary>
    /// 職稱名稱
    /// </summary>
    public string PositionName { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int PositionSort { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public int PositionStatus { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;
}
