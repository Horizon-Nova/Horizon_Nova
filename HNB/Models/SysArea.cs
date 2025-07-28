using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 台灣行政區表
/// </summary>
public partial class SysArea
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 刪除標記(0正常 1刪除)
    /// </summary>
    public int BaseIsDelete { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime BaseCreateTime { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    public DateTime BaseModifyTime { get; set; }

    /// <summary>
    /// 建立人
    /// </summary>
    public long BaseCreatorId { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    public long BaseModifierId { get; set; }

    /// <summary>
    /// 資料版本(每次更新+1)
    /// </summary>
    public int BaseVersion { get; set; }

    /// <summary>
    /// 地區代碼
    /// </summary>
    public string AreaCode { get; set; } = null!;

    /// <summary>
    /// 父地區代碼
    /// </summary>
    public string ParentAreaCode { get; set; } = null!;

    /// <summary>
    /// 地區名稱
    /// </summary>
    public string AreaName { get; set; } = null!;

    /// <summary>
    /// 郵遞區號
    /// </summary>
    public string ZipCode { get; set; } = null!;

    /// <summary>
    /// 地區層級 (1 省 2 市 3 區)
    /// </summary>
    public int AreaLevel { get; set; }
}
