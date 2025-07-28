using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 字典資料表
/// </summary>
public partial class SysDataDictDetail
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
    /// 資料版本
    /// </summary>
    public int BaseVersion { get; set; }

    /// <summary>
    /// 字典類型(外鍵)
    /// </summary>
    public string DictType { get; set; } = null!;

    /// <summary>
    /// 字典排序
    /// </summary>
    public int DictSort { get; set; }

    /// <summary>
    /// 字典鍵(一般從1開始)
    /// </summary>
    public int DictKey { get; set; }

    /// <summary>
    /// 字典值
    /// </summary>
    public string DictValue { get; set; } = null!;

    /// <summary>
    /// 顯示樣式(default primary success info warning danger)
    /// </summary>
    public string ListClass { get; set; } = null!;

    /// <summary>
    /// 字典狀態(0禁用 1啟用)
    /// </summary>
    public int DictStatus { get; set; }

    /// <summary>
    /// 預設選中(0否 1是)
    /// </summary>
    public int IsDefault { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;
}
