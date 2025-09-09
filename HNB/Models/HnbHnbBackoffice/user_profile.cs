using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

/// <summary>
/// 使用者個人資料表
/// </summary>
[Table("user_profiles", Schema = "dbo")]
[Index("account", Name = "user_profiles_account_key", IsUnique = true)]
[Index("email", Name = "user_profiles_email_key", IsUnique = true)]
[Index("employee_id", Name = "user_profiles_employee_id_key", IsUnique = true)]
[Index("id_card_number", Name = "user_profiles_id_card_number_key", IsUnique = true)]
public partial class user_profile
{
    /// <summary>
    /// 主鍵編號
    /// </summary>
    [Key]
    public int id { get; set; }

    /// <summary>
    /// 組織ID
    /// </summary>
    public int organization_id { get; set; }

    /// <summary>
    /// 員工ID
    /// </summary>
    [StringLength(100)]
    public string employee_id { get; set; } = null!;

    /// <summary>
    /// 組織名稱
    /// </summary>
    [StringLength(255)]
    public string? organization_name { get; set; }

    /// <summary>
    /// 員工名稱
    /// </summary>
    [StringLength(255)]
    public string employee_name { get; set; } = null!;

    /// <summary>
    /// 權限
    /// </summary>
    public List<string>? permissions { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    [StringLength(255)]
    public string email { get; set; } = null!;

    /// <summary>
    /// 分機號
    /// </summary>
    [StringLength(50)]
    public string? extension { get; set; }

    /// <summary>
    /// 停用
    /// </summary>
    public bool disabled { get; set; }

    /// <summary>
    /// 帳號
    /// </summary>
    [StringLength(255)]
    public string account { get; set; } = null!;

    /// <summary>
    /// 密碼
    /// </summary>
    [StringLength(255)]
    public string password { get; set; } = null!;

    /// <summary>
    /// 身分證
    /// </summary>
    [StringLength(50)]
    public string? id_card_number { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly? birthday { get; set; }

    /// <summary>
    /// 血型
    /// </summary>
    [StringLength(10)]
    public string? blood_type { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    [StringLength(50)]
    public string? phone { get; set; }

    /// <summary>
    /// 緊急聯絡人
    /// </summary>
    [StringLength(255)]
    public string? emergency_contact_name { get; set; }

    /// <summary>
    /// 緊急連絡人電話
    /// </summary>
    [StringLength(50)]
    public string? emergency_contact_phone { get; set; }

    /// <summary>
    /// 最後登入
    /// </summary>
    public DateTime? last_login_at { get; set; }

    /// <summary>
    /// 位置
    /// </summary>
    [StringLength(255)]
    public string? location { get; set; }

    /// <summary>
    /// 員工名稱(英文)
    /// </summary>
    [StringLength(255)]
    public string? employee_english_name { get; set; }

    /// <summary>
    /// 裝置設備
    /// </summary>
    public List<string>? device_equipment { get; set; }

    /// <summary>
    /// 常用設備
    /// </summary>
    public List<string>? common_equipment { get; set; }

    /// <summary>
    /// IP
    /// </summary>
    public IPAddress? last_login_ip { get; set; }

    /// <summary>
    /// 畫面白名單
    /// </summary>
    public List<string>? screen_whitelist { get; set; }

    /// <summary>
    /// 畫面元件白名單
    /// </summary>
    public List<string>? screen_component_whitelist { get; set; }

    /// <summary>
    /// 資料建立時間
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// 資料最後更新時間
    /// </summary>
    public DateTime? updated_at { get; set; }

    [StringLength(512)]
    public string? avatar_path { get; set; }

    [StringLength(64)]
    public string? avatar_mime { get; set; }

    public DateTime? avatar_updated_at { get; set; }
}
