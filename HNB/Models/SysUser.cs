using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 使用者表
/// </summary>
public partial class SysUser
{
    public long Id { get; set; }

    public int BaseIsDelete { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public DateTime BaseModifyTime { get; set; }

    public long BaseCreatorId { get; set; }

    public long BaseModifierId { get; set; }

    public int BaseVersion { get; set; }

    /// <summary>
    /// 帳號
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 密碼
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// 鹽值
    /// </summary>
    public string Salt { get; set; } = null!;

    /// <summary>
    /// 真實姓名
    /// </summary>
    public string RealName { get; set; } = null!;

    /// <summary>
    /// 部門 ID
    /// </summary>
    public long DepartmentId { get; set; }

    /// <summary>
    /// 性別
    /// </summary>
    public int Gender { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public string Birthday { get; set; } = null!;

    /// <summary>
    /// 頭像
    /// </summary>
    public string Portrait { get; set; } = null!;

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// 手機
    /// </summary>
    public string Mobile { get; set; } = null!;

    /// <summary>
    /// QQ
    /// </summary>
    public string Qq { get; set; } = null!;

    /// <summary>
    /// 微信
    /// </summary>
    public string WeChat { get; set; } = null!;

    /// <summary>
    /// 登入次數
    /// </summary>
    public int LoginCount { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    public int UserStatus { get; set; }

    /// <summary>
    /// 系統帳號
    /// </summary>
    public int IsSystem { get; set; }

    /// <summary>
    /// 是否在線
    /// </summary>
    public int IsOnline { get; set; }

    /// <summary>
    /// 首次登入
    /// </summary>
    public DateTime FirstVisit { get; set; }

    /// <summary>
    /// 上次登入
    /// </summary>
    public DateTime PreviousVisit { get; set; }

    /// <summary>
    /// 最後登入
    /// </summary>
    public DateTime LastVisit { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 後台 Token
    /// </summary>
    public string WebToken { get; set; } = null!;

    /// <summary>
    /// API Token
    /// </summary>
    public string ApiToken { get; set; } = null!;
}
