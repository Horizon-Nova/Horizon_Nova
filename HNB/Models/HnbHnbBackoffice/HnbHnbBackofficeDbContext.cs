using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

public partial class HnbHnbBackofficeDbContext : DbContext
{
    public HnbHnbBackofficeDbContext(DbContextOptions<HnbHnbBackofficeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<security_ip_key> security_ip_keys { get; set; }

    public virtual DbSet<system_config> system_configs { get; set; }

    public virtual DbSet<vw_system_config_database> vw_system_config_databases { get; set; }

    public virtual DbSet<vw_system_config_notification> vw_system_config_notifications { get; set; }

    public virtual DbSet<vw_system_config_security> vw_system_config_securities { get; set; }

    public virtual DbSet<vw_system_config_server> vw_system_config_servers { get; set; }

    public virtual DbSet<vw_system_config_system> vw_system_config_systems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("dbo", "pgcrypto");

        modelBuilder.Entity<security_ip_key>(entity =>
        {
            entity.HasKey(e => e.id).HasName("security_ip_keys_pkey");

            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.disabled).HasDefaultValue(false);
            entity.Property(e => e.expires_at)
                .HasDefaultValueSql("(now() + '01:00:00'::interval)")
                .HasComment("失效時間");
            entity.Property(e => e.ip_addr).HasComment("IPv4/IPv6");
            entity.Property(e => e.key).HasComment("DB 自動產生");
            entity.Property(e => e.key_components).HasComment("單一字串");
            entity.Property(e => e.time_param).HasComment("基準時間");
        });

        modelBuilder.Entity<system_config>(entity =>
        {
            entity.HasKey(e => e.id).HasName("system_config_pkey");

            entity.ToTable("system_config", "dbo", tb => tb.HasComment("系統設定資料表"));

            entity.Property(e => e.id).HasComment("主鍵編號");
            entity.Property(e => e.admin_email).HasComment("管理員信箱");
            entity.Property(e => e.auto_backup_enabled)
                .HasDefaultValue(false)
                .HasComment("啟用自動備份");
            entity.Property(e => e.backup_frequency).HasComment("備份頻率 (text[]: 每小時、每日、每周)");
            entity.Property(e => e.cpu_usage).HasComment("CPU 使用率 (陣列格式)");
            entity.Property(e => e.db_host).HasComment("資料庫主機位址");
            entity.Property(e => e.db_name).HasComment("資料庫名稱");
            entity.Property(e => e.db_port).HasComment("資料庫連接埠");
            entity.Property(e => e.debug_mode)
                .HasDefaultValue(false)
                .HasComment("偵錯模式");
            entity.Property(e => e.disk_usage).HasComment("磁碟使用率 (陣列格式)");
            entity.Property(e => e.email_notification)
                .HasDefaultValue(false)
                .HasComment("電子郵件通知");
            entity.Property(e => e.host_name).HasComment("伺服器主機名稱");
            entity.Property(e => e.inbound_traffic).HasComment("入站流量");
            entity.Property(e => e.ip_whitelist).HasComment("IP 白名單（JSON 陣列格式）");
            entity.Property(e => e.kernel_version).HasComment("伺服器核心版本");
            entity.Property(e => e.mail_template).HasComment("郵件模板");
            entity.Property(e => e.mail_template_id).HasComment("郵件模板ID");
            entity.Property(e => e.maintenance_mode)
                .HasDefaultValue(false)
                .HasComment("維護模式");
            entity.Property(e => e.max_login_attempts).HasComment("最大登入嘗試次數");
            entity.Property(e => e.memory_usage).HasComment("記憶體使用率 (陣列格式)");
            entity.Property(e => e.notify_status)
                .HasDefaultValueSql("'{}'::text[]")
                .HasComment("通知類型狀態 (text[])");
            entity.Property(e => e.notify_types)
                .HasDefaultValueSql("'{}'::text[]")
                .HasComment("通知類型 (text[])");
            entity.Property(e => e.operating_system).HasComment("伺服器作業系統");
            entity.Property(e => e.outbound_traffic).HasComment("出站流量");
            entity.Property(e => e.push_notification)
                .HasDefaultValue(false)
                .HasComment("推播通知");
            entity.Property(e => e.session_timeout).HasComment("會話逾時 (秒)");
            entity.Property(e => e.sms_notification)
                .HasDefaultValue(false)
                .HasComment("簡訊通知");
            entity.Property(e => e.timezone).HasComment("時區設定");
            entity.Property(e => e.two_factor_auth)
                .HasDefaultValue(false)
                .HasComment("雙因子驗證");
            entity.Property(e => e.uptime).HasComment("系統運行時間");
            entity.Property(e => e.website_name).HasComment("網站名稱");
            entity.Property(e => e.website_url).HasComment("網站網址");
        });

        modelBuilder.Entity<vw_system_config_database>(entity =>
        {
            entity.ToView("vw_system_config_database", "dbo");

            entity.Property(e => e.auto_backup_enabled).HasComment("啟用自動備份");
            entity.Property(e => e.backup_frequency).HasComment("備份頻率");
            entity.Property(e => e.db_host).HasComment("主機位址");
            entity.Property(e => e.db_name).HasComment("資料庫名稱");
            entity.Property(e => e.db_port).HasComment("連接埠");
        });

        modelBuilder.Entity<vw_system_config_notification>(entity =>
        {
            entity.ToView("vw_system_config_notification", "dbo");

            entity.Property(e => e.email_notification).HasComment("電子郵件通知");
            entity.Property(e => e.mail_template).HasComment("郵件範本");
            entity.Property(e => e.mail_template_id).HasComment("郵件範本 ID");
            entity.Property(e => e.notify_status).HasComment("通知類型狀態 (text[])");
            entity.Property(e => e.notify_types).HasComment("通知類型 (text[])");
            entity.Property(e => e.push_notification).HasComment("推播通知");
            entity.Property(e => e.sms_notification).HasComment("簡訊通知");
        });

        modelBuilder.Entity<vw_system_config_security>(entity =>
        {
            entity.ToView("vw_system_config_security", "dbo");

            entity.Property(e => e.ip_whitelist).HasComment("IP 白名單");
            entity.Property(e => e.max_login_attempts).HasComment("最大登入嘗試次數");
            entity.Property(e => e.session_timeout).HasComment("會話逾時 (秒)");
            entity.Property(e => e.two_factor_auth).HasComment("雙因子驗證");
        });

        modelBuilder.Entity<vw_system_config_server>(entity =>
        {
            entity.ToView("vw_system_config_server", "dbo");

            entity.Property(e => e.cpu_usage).HasComment("CPU 使用率");
            entity.Property(e => e.disk_usage).HasComment("磁碟使用率");
            entity.Property(e => e.host_name).HasComment("主機名稱");
            entity.Property(e => e.inbound_traffic).HasComment("入站流量");
            entity.Property(e => e.kernel_version).HasComment("核心版本");
            entity.Property(e => e.memory_usage).HasComment("記憶體使用率");
            entity.Property(e => e.operating_system).HasComment("作業系統");
            entity.Property(e => e.outbound_traffic).HasComment("出站流量");
            entity.Property(e => e.uptime).HasComment("系統運行時間");
        });

        modelBuilder.Entity<vw_system_config_system>(entity =>
        {
            entity.ToView("vw_system_config_system", "dbo");

            entity.Property(e => e.admin_email).HasComment("管理員信箱");
            entity.Property(e => e.debug_mode).HasComment("偵錯模式");
            entity.Property(e => e.maintenance_mode).HasComment("維護模式");
            entity.Property(e => e.timezone).HasComment("時區設定");
            entity.Property(e => e.website_name).HasComment("網站名稱");
            entity.Property(e => e.website_url).HasComment("網站網址");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
