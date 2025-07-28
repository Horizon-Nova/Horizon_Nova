using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 排程任務表
/// </summary>
public partial class SysAutoJob
{
    public long Id { get; set; }

    public int BaseIsDelete { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public DateTime BaseModifyTime { get; set; }

    public long BaseCreatorId { get; set; }

    public long BaseModifierId { get; set; }

    public int BaseVersion { get; set; }

    /// <summary>
    /// 任務組名稱
    /// </summary>
    public string JobGroupName { get; set; } = null!;

    /// <summary>
    /// 任務名稱
    /// </summary>
    public string JobName { get; set; } = null!;

    /// <summary>
    /// 任務狀態 (0 停用 1 啟用)
    /// </summary>
    public int JobStatus { get; set; }

    /// <summary>
    /// 排程 Cron
    /// </summary>
    public string CronExpression { get; set; } = null!;

    /// <summary>
    /// 開始時間
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 結束時間
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 下次執行時間
    /// </summary>
    public DateTime NextStartTime { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;
}
