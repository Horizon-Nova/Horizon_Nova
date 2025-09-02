using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_system_config_notification
{
    public long? id { get; set; }

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
    /// 郵件範本
    /// </summary>
    public List<string>? mail_template { get; set; }

    /// <summary>
    /// 郵件範本 ID
    /// </summary>
    public List<long>? mail_template_id { get; set; }

    /// <summary>
    /// 通知類型 (text[])
    /// </summary>
    public List<string>? notify_types { get; set; }

    /// <summary>
    /// 通知類型狀態 (text[])
    /// </summary>
    public List<string>? notify_status { get; set; }
}
