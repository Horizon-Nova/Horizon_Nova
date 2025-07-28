using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 排程任務日誌表
/// </summary>
public partial class SysAutoJobLog
{
    public long Id { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public long BaseCreatorId { get; set; }

    /// <summary>
    /// 任務組名稱
    /// </summary>
    public string JobGroupName { get; set; } = null!;

    /// <summary>
    /// 任務名稱
    /// </summary>
    public string JobName { get; set; } = null!;

    /// <summary>
    /// 執行狀態 (0 失敗 1 成功)
    /// </summary>
    public int LogStatus { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;
}
