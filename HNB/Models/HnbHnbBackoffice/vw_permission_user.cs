using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_permission_user
{
    public int? id { get; set; }

    [StringLength(100)]
    public string? username { get; set; }

    [StringLength(100)]
    public string? nickname { get; set; }

    [StringLength(200)]
    public string? full_name { get; set; }

    [StringLength(255)]
    public string? email { get; set; }

    [StringLength(20)]
    public string? phone { get; set; }

    public string? bio { get; set; }

    [StringLength(500)]
    public string? avatar_url { get; set; }

    [StringLength(500)]
    public string? third_party_avatar { get; set; }

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

    [StringLength(50)]
    public string? login_method { get; set; }

    [StringLength(255)]
    public string? third_party_id { get; set; }

    public bool? is_email_verified { get; set; }

    public bool? is_phone_verified { get; set; }

    public bool? two_factor_enabled { get; set; }

    public List<string>? trusted_devices { get; set; }

    public List<string>? trusted_ips { get; set; }

    [Column(TypeName = "jsonb")]
    public string? last_device_info { get; set; }

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

    public IPAddress? last_login_ip { get; set; }

    public string? last_login_user_agent { get; set; }

    public List<string>? tags { get; set; }

    public string? notes { get; set; }

    [StringLength(100)]
    public string? organization_name { get; set; }

    [StringLength(100)]
    public string? role_name { get; set; }

    public long? managed_users_count { get; set; }

    public long? managed_roles_count { get; set; }
}
