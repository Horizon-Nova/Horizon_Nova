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

    public virtual DbSet<permission_management> permission_managements { get; set; }

    public virtual DbSet<security_ip_key> security_ip_keys { get; set; }

    public virtual DbSet<system_config> system_configs { get; set; }

    public virtual DbSet<vw_hardware_monitoring> vw_hardware_monitorings { get; set; }

    public virtual DbSet<vw_permission_organization> vw_permission_organizations { get; set; }

    public virtual DbSet<vw_permission_role> vw_permission_roles { get; set; }

    public virtual DbSet<vw_permission_user> vw_permission_users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("dbo", "pgcrypto");

        modelBuilder.Entity<permission_management>(entity =>
        {
            entity.HasKey(e => e.id).HasName("permission_management_pkey");

            entity.HasIndex(e => e.payment_methods, "idx_permission_management_payment_methods").HasMethod("gin");

            entity.HasIndex(e => e.preferences, "idx_permission_management_preferences").HasMethod("gin");

            entity.HasIndex(e => e.subscription_products, "idx_permission_management_subscription_products").HasMethod("gin");

            entity.Property(e => e.auto_renew).HasDefaultValue(true);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.is_email_verified).HasDefaultValue(false);
            entity.Property(e => e.is_online).HasDefaultValue(false);
            entity.Property(e => e.is_phone_verified).HasDefaultValue(false);
            entity.Property(e => e.language).HasDefaultValueSql("'zh-TW'::character varying");
            entity.Property(e => e.level).HasDefaultValue(1);
            entity.Property(e => e.login_count).HasDefaultValue(0);
            entity.Property(e => e.profile_completion_percentage).HasDefaultValue(0);
            entity.Property(e => e.sort_order).HasDefaultValue(0);
            entity.Property(e => e.status).HasDefaultValueSql("'active'::character varying");
            entity.Property(e => e.theme).HasDefaultValueSql("'auto'::character varying");
            entity.Property(e => e.timezone).HasDefaultValueSql("'Asia/Taipei'::character varying");
            entity.Property(e => e.two_factor_enabled).HasDefaultValue(false);

            entity.HasOne(d => d.parent).WithMany(p => p.Inverseparent).HasConstraintName("permission_management_parent_id_fkey");
        });

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

            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.debug_mode).HasDefaultValue(false);
            entity.Property(e => e.last_activity_description).HasComment("最近活動的詳細描述");
            entity.Property(e => e.last_activity_timestamp).HasComment("最近活動發生的時間戳記");
            entity.Property(e => e.last_activity_type).HasComment("最近活動類型（如：系統啟動、快取更新、維護等）");
            entity.Property(e => e.maintenance_mode).HasDefaultValue(false);
            entity.Property(e => e.recent_activities).HasComment("最近活動列表（JSON格式，包含多個活動記錄）");
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<vw_hardware_monitoring>(entity =>
        {
            entity.ToView("vw_hardware_monitoring", "dbo");

            entity.Property(e => e.last_activity_description).HasComment("最近活動描述");
            entity.Property(e => e.last_activity_timestamp).HasComment("最近活動時間");
            entity.Property(e => e.last_activity_type).HasComment("最近活動類型");
            entity.Property(e => e.recent_activities).HasComment("最近活動列表（JSON格式）");
        });

        modelBuilder.Entity<vw_permission_organization>(entity =>
        {
            entity.ToView("vw_permission_organization", "dbo");
        });

        modelBuilder.Entity<vw_permission_role>(entity =>
        {
            entity.ToView("vw_permission_role", "dbo");
        });

        modelBuilder.Entity<vw_permission_user>(entity =>
        {
            entity.ToView("vw_permission_user", "dbo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
