using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 操作日誌表
/// </summary>
public partial class SysLogOperate
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
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 日誌類型
    /// </summary>
    public string LogType { get; set; } = null!;

    /// <summary>
    /// 業務類型
    /// </summary>
    public string BusinessType { get; set; } = null!;

    /// <summary>
    /// 執行 URL
    /// </summary>
    public string ExecuteUrl { get; set; } = null!;

    /// <summary>
    /// 執行參數
    /// </summary>
    public string ExecuteParam { get; set; } = null!;

    /// <summary>
    /// 執行結果
    /// </summary>
    public string ExecuteResult { get; set; } = null!;

    /// <summary>
    /// 執行時間(ms)
    /// </summary>
    public int ExecuteTime { get; set; }
}
