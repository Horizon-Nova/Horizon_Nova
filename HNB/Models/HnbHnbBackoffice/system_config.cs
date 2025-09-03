using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

/// <summary>
/// 系統設定資料表
/// </summary>
[Table("system_config", Schema = "dbo")]
[Index("website_url", Name = "system_config_website_url_key", IsUnique = true)]
public partial class system_config
{
    /// <summary>
    /// 主鍵編號
    /// </summary>
    [Key]
    public long id { get; set; }

    /// <summary>
    /// 網站名稱
    /// </summary>
    [StringLength(255)]
    public string? website_name { get; set; }

    /// <summary>
    /// 網站網址
    /// </summary>
    [StringLength(255)]
    public string? website_url { get; set; }

    /// <summary>
    /// 管理員信箱
    /// </summary>
    [StringLength(255)]
    public string? admin_email { get; set; }

    /// <summary>
    /// 時區設定
    /// </summary>
    [StringLength(64)]
    public string? timezone { get; set; }

    /// <summary>
    /// 維護模式
    /// </summary>
    public bool? maintenance_mode { get; set; }

    /// <summary>
    /// 偵錯模式
    /// </summary>
    public bool? debug_mode { get; set; }

    /// <summary>
    /// 會話逾時 (秒)
    /// </summary>
    [StringLength(10)]
    public string? session_timeout { get; set; }

    /// <summary>
    /// 最大登入嘗試次數
    /// </summary>
    [StringLength(10)]
    public string? max_login_attempts { get; set; }

    /// <summary>
    /// 雙因子驗證
    /// </summary>
    public bool? two_factor_auth { get; set; }

    /// <summary>
    /// IP 白名單（JSON 陣列格式）
    /// </summary>
    [Column(TypeName = "json")]
    public string? ip_whitelist { get; set; }

    /// <summary>
    /// 電子郵件通知
    /// </summary>
    public bool? email_notification { get; set; }

    /// <summary>
    /// 簡訊通知
    /// </summary>
    public bool? sms_notification { get; set; }

    /// <summary>
    /// 推播通知
    /// </summary>
    public bool? push_notification { get; set; }

    /// <summary>
    /// 通知類型 (text[])
    /// </summary>
    public List<string>? notify_types { get; set; }

    /// <summary>
    /// 通知類型狀態 (text[])
    /// </summary>
    public List<string>? notify_status { get; set; }

    /// <summary>
    /// 資料庫主機位址
    /// </summary>
    [StringLength(255)]
    public string? db_host { get; set; }

    /// <summary>
    /// 資料庫連接埠
    /// </summary>
    [StringLength(10)]
    public string? db_port { get; set; }

    /// <summary>
    /// 資料庫名稱
    /// </summary>
    [StringLength(128)]
    public string? db_name { get; set; }

    /// <summary>
    /// 備份頻率 (text[]: 每小時、每日、每周)
    /// </summary>
    [StringLength(50)]
    public string? backup_frequency { get; set; }

    /// <summary>
    /// 啟用自動備份
    /// </summary>
    public bool? auto_backup_enabled { get; set; }

    /// <summary>
    /// 伺服器主機名稱
    /// </summary>
    [StringLength(128)]
    public string? host_name { get; set; }

    /// <summary>
    /// 伺服器作業系統
    /// </summary>
    [StringLength(128)]
    public string? operating_system { get; set; }

    /// <summary>
    /// 伺服器核心版本
    /// </summary>
    [StringLength(128)]
    public string? kernel_version { get; set; }

    /// <summary>
    /// 系統運行時間
    /// </summary>
    [StringLength(128)]
    public string? uptime { get; set; }

    /// <summary>
    /// 入站流量
    /// </summary>
    [StringLength(64)]
    public string? inbound_traffic { get; set; }

    /// <summary>
    /// 出站流量
    /// </summary>
    [StringLength(64)]
    public string? outbound_traffic { get; set; }

    /// <summary>
    /// CPU 使用率 (陣列格式)
    /// </summary>
    public List<string>? cpu_usage { get; set; }

    /// <summary>
    /// 記憶體使用率 (陣列格式)
    /// </summary>
    public List<string>? memory_usage { get; set; }

    /// <summary>
    /// 磁碟使用率 (陣列格式)
    /// </summary>
    public List<string>? disk_usage { get; set; }

    /// <summary>
    /// 郵件模板ID
    /// </summary>
    public List<long>? mail_template_id { get; set; }

    /// <summary>
    /// 郵件模板
    /// </summary>
    public List<string>? mail_template { get; set; }
}
