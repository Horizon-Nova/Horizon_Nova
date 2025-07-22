using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 選單表
/// </summary>
public partial class SysMenu
{
    public long Id { get; set; }

    public int BaseIsDelete { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public DateTime BaseModifyTime { get; set; }

    public long BaseCreatorId { get; set; }

    public long BaseModifierId { get; set; }

    public int BaseVersion { get; set; }

    /// <summary>
    /// 父選單 ID (0 表示根選單)
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 選單名稱
    /// </summary>
    public string MenuName { get; set; } = null!;

    /// <summary>
    /// 選單圖示
    /// </summary>
    public string MenuIcon { get; set; } = null!;

    /// <summary>
    /// 選單 URL
    /// </summary>
    public string MenuUrl { get; set; } = null!;

    /// <summary>
    /// 開啟方式
    /// </summary>
    public string MenuTarget { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int MenuSort { get; set; }

    /// <summary>
    /// 類型 (1 目錄 2 頁面 3 按鈕)
    /// </summary>
    public int MenuType { get; set; }

    /// <summary>
    /// 狀態 (0 停用 1 啟用)
    /// </summary>
    public int MenuStatus { get; set; }

    /// <summary>
    /// 權限標識
    /// </summary>
    public string Authorize { get; set; } = null!;

    /// <summary>
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;
}
