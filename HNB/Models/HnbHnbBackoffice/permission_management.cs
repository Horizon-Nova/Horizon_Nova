using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Table("permission_management", Schema = "dbo")]
[Index("created_at", Name = "idx_permission_management_created_at")]
[Index("email", Name = "idx_permission_management_email")]
[Index("is_active", Name = "idx_permission_management_is_active")]
[Index("last_login_at", Name = "idx_permission_management_last_login_at")]
[Index("login_method", Name = "idx_permission_management_login_method")]
[Index("parent_id", Name = "idx_permission_management_parent_id")]
[Index("status", Name = "idx_permission_management_status")]
[Index("subscription_status", Name = "idx_permission_management_subscription_status")]
[Index("third_party_id", Name = "idx_permission_management_third_party_id")]
[Index("type", Name = "idx_permission_management_type")]
public partial class permission_management
{
    [Key]
    public int id { get; set; }

    [StringLength(50)]
    public string type { get; set; } = null!;

    [StringLength(100)]
    public string name { get; set; } = null!;

    [StringLength(500)]
    public string? description { get; set; }

    [StringLength(255)]
    public string? email { get; set; }

    [StringLength(20)]
    public string? phone { get; set; }

    public string? bio { get; set; }

    [StringLength(500)]
    public string? avatar_url { get; set; }

    [StringLength(100)]
    public string? nickname { get; set; }

    [StringLength(200)]
    public string? full_name { get; set; }

    [StringLength(10)]
    public string? gender { get; set; }

    public DateOnly? birthday { get; set; }

    [StringLength(20)]
    public string? zodiac_sign { get; set; }

    [StringLength(50)]
    public string? favorite_color { get; set; }

    [StringLength(50)]
    public string? color_scheme { get; set; }

    [StringLength(200)]
    public string? location { get; set; }

    [StringLength(50)]
    public string? timezone { get; set; }

    [StringLength(255)]
    public string? password_hash { get; set; }

    [StringLength(255)]
    public string? salt { get; set; }

    [StringLength(50)]
    public string? login_method { get; set; }

    [StringLength(255)]
    public string? third_party_id { get; set; }

    [StringLength(255)]
    public string? third_party_email { get; set; }

    [StringLength(500)]
    public string? third_party_avatar { get; set; }

    public bool? is_email_verified { get; set; }

    public bool? is_phone_verified { get; set; }

    public bool? two_factor_enabled { get; set; }

    [StringLength(255)]
    public string? two_factor_secret { get; set; }

    public List<string>? trusted_devices { get; set; }

    public List<string>? trusted_ips { get; set; }

    public List<string>? device_fingerprints { get; set; }

    [Column(TypeName = "jsonb")]
    public string? last_device_info { get; set; }

    [Column(TypeName = "jsonb")]
    public string? security_questions { get; set; }

    [Column(TypeName = "jsonb")]
    public string? subscription_products { get; set; }

    [Column(TypeName = "jsonb")]
    public string? payment_methods { get; set; }

    [StringLength(50)]
    public string? subscription_status { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? subscription_expires_at { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? trial_ends_at { get; set; }

    [StringLength(20)]
    public string? billing_cycle { get; set; }

    public bool? auto_renew { get; set; }

    [Column(TypeName = "jsonb")]
    public string? preferences { get; set; }

    [Column(TypeName = "jsonb")]
    public string? notification_settings { get; set; }

    [Column(TypeName = "jsonb")]
    public string? privacy_settings { get; set; }

    [StringLength(10)]
    public string? language { get; set; }

    [StringLength(20)]
    public string? theme { get; set; }

    public int? login_count { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? last_login_at { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? last_activity_at { get; set; }

    public TimeSpan? total_session_time { get; set; }

    public int? profile_completion_percentage { get; set; }

    public int? parent_id { get; set; }

    public int? sort_order { get; set; }

    public int? level { get; set; }

    public List<string>? permissions { get; set; }

    public List<string>? roles { get; set; }

    public bool? is_active { get; set; }

    public bool? is_online { get; set; }

    [StringLength(50)]
    public string? status { get; set; }

    public string? status_reason { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? created_at { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? updated_at { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? last_password_change_at { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? password_expires_at { get; set; }

    public int? created_by { get; set; }

    public int? updated_by { get; set; }

    public IPAddress? last_login_ip { get; set; }

    public string? last_login_user_agent { get; set; }

    public List<string>? tags { get; set; }

    public string? notes { get; set; }

    public string? internal_notes { get; set; }

    [InverseProperty("parent")]
    public virtual ICollection<permission_management> Inverseparent { get; set; } = new List<permission_management>();

    [ForeignKey("parent_id")]
    [InverseProperty("Inverseparent")]
    public virtual permission_management? parent { get; set; }
}
