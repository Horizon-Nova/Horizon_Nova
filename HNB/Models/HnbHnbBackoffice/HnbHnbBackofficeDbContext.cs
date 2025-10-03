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

    public virtual DbSet<cpu_info> cpu_infos { get; set; }

    public virtual DbSet<gpu_info> gpu_infos { get; set; }

    public virtual DbSet<memory_info> memory_infos { get; set; }

    public virtual DbSet<permission_management> permission_managements { get; set; }

    public virtual DbSet<security_ip_key> security_ip_keys { get; set; }

    public virtual DbSet<sidebar_navigation> sidebar_navigations { get; set; }

    public virtual DbSet<system_config> system_configs { get; set; }

    public virtual DbSet<vw_hardware_monitoring> vw_hardware_monitorings { get; set; }

    public virtual DbSet<vw_permission_organization> vw_permission_organizations { get; set; }

    public virtual DbSet<vw_permission_role> vw_permission_roles { get; set; }

    public virtual DbSet<vw_permission_user> vw_permission_users { get; set; }

    public virtual DbSet<vw_sidebar_navigation> vw_sidebar_navigations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("dbo", "pgcrypto");

        modelBuilder.Entity<cpu_info>(entity =>
        {
            entity.HasKey(e => e.id).HasName("cpu_info_pkey");

            entity.ToTable("cpu_info", "dbo", tb => tb.HasComment("CPU硬體資訊表 - 儲存CPU詳細規格、溫度、使用率、健康度等資訊"));

            entity.Property(e => e.architecture).HasComment("CPU架構");
            entity.Property(e => e.base_clock).HasComment("CPU基礎時脈(GHz)");
            entity.Property(e => e.boost_clock).HasComment("CPU加速時脈(GHz)");
            entity.Property(e => e.cache).HasComment("CPU快取大小");
            entity.Property(e => e.cores).HasComment("CPU核心數");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.current_power_consumption).HasComment("CPU目前功耗(W)");
            entity.Property(e => e.current_temperature).HasComment("CPU目前溫度(°C)");
            entity.Property(e => e.current_usage).HasComment("CPU目前使用率(%)");
            entity.Property(e => e.health_percentage).HasComment("CPU健康度百分比");
            entity.Property(e => e.health_status).HasComment("CPU健康狀態");
            entity.Property(e => e.manufacturer).HasComment("CPU製造商");
            entity.Property(e => e.max_temperature).HasComment("CPU最高溫度(°C)");
            entity.Property(e => e.model).HasComment("CPU型號");
            entity.Property(e => e.socket).HasComment("CPU插槽類型");
            entity.Property(e => e.tdp).HasComment("CPU熱設計功耗(W)");
            entity.Property(e => e.threads).HasComment("CPU執行緒數");
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.system_config).WithMany(p => p.cpu_infos).HasConstraintName("cpu_info_system_config_id_fkey");
        });

        modelBuilder.Entity<gpu_info>(entity =>
        {
            entity.HasKey(e => e.id).HasName("gpu_info_pkey");

            entity.ToTable("gpu_info", "dbo", tb => tb.HasComment("GPU硬體資訊表 - 儲存GPU詳細規格、溫度、使用率、健康度等資訊"));

            entity.Property(e => e.base_clock).HasComment("GPU基礎時脈(MHz)");
            entity.Property(e => e.boost_clock).HasComment("GPU加速時脈(MHz)");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.cuda_cores).HasComment("GPU CUDA核心數");
            entity.Property(e => e.current_power_consumption).HasComment("GPU目前功耗(W)");
            entity.Property(e => e.current_temperature).HasComment("GPU目前溫度(°C)");
            entity.Property(e => e.current_usage).HasComment("GPU目前使用率(%)");
            entity.Property(e => e.driver_version).HasComment("GPU驅動程式版本");
            entity.Property(e => e.health_percentage).HasComment("GPU健康度百分比");
            entity.Property(e => e.health_status).HasComment("GPU健康狀態");
            entity.Property(e => e.manufacturer).HasComment("GPU製造商");
            entity.Property(e => e.max_temperature).HasComment("GPU最高溫度(°C)");
            entity.Property(e => e.memory_clock).HasComment("GPU記憶體時脈(MHz)");
            entity.Property(e => e.memory_size).HasComment("GPU記憶體大小");
            entity.Property(e => e.memory_type).HasComment("GPU記憶體類型");
            entity.Property(e => e.model).HasComment("GPU型號");
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.system_config).WithMany(p => p.gpu_infos).HasConstraintName("gpu_info_system_config_id_fkey");
        });

        modelBuilder.Entity<memory_info>(entity =>
        {
            entity.HasKey(e => e.id).HasName("memory_info_pkey");

            entity.ToTable("memory_info", "dbo", tb => tb.HasComment("記憶體硬體資訊表 - 儲存記憶體容量、速度、使用率、健康度等資訊"));

            entity.Property(e => e.available_memory_gb).HasComment("可用記憶體(GB)");
            entity.Property(e => e.channels).HasComment("記憶體通道數");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.current_usage).HasComment("記憶體目前使用率(%)");
            entity.Property(e => e.health_percentage).HasComment("記憶體健康度百分比");
            entity.Property(e => e.health_status).HasComment("記憶體健康狀態");
            entity.Property(e => e.memory_type).HasComment("記憶體類型");
            entity.Property(e => e.speed).HasComment("記憶體速度");
            entity.Property(e => e.total_capacity).HasComment("記憶體總容量");
            entity.Property(e => e.total_capacity_gb).HasComment("記憶體總容量(GB)");
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.used_memory_gb).HasComment("已使用記憶體(GB)");

            entity.HasOne(d => d.system_config).WithMany(p => p.memory_infos).HasConstraintName("memory_info_system_config_id_fkey");
        });

        modelBuilder.Entity<permission_management>(entity =>
        {
            entity.HasKey(e => e.id).HasName("permission_management_pkey");
            
            entity.ToTable("permission_management", "dbo");

            entity.HasIndex(e => e.navigation_permissions, "idx_permission_management_navigation_permissions").HasMethod("gin");

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
            entity.Property(e => e.navigation_permissions).HasComment("用戶可訪問的導航頁面權限陣列，儲存 sidebar_navigation 表的 code 編號");
            entity.Property(e => e.organization_names).HasComment("用戶所屬的組織名稱陣列，關聯到 permission_management 表中 type=organization 的記錄");
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

        modelBuilder.Entity<sidebar_navigation>(entity =>
        {
            entity.HasKey(e => e.id).HasName("sidebar_navigation_pkey");

            entity.ToTable("sidebar_navigation", "dbo", tb => tb.HasComment("側欄導航管理表"));

            entity.Property(e => e.id).HasComment("主鍵，自動遞增");
            entity.Property(e => e.code).HasComment("導航項目編號，唯一識別碼");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("建立時間");
            entity.Property(e => e.icon)
                .HasDefaultValueSql("'circle'::character varying")
                .HasComment("導航項目圖示名稱");
            entity.Property(e => e.is_active)
                .HasDefaultValue(true)
                .HasComment("是否啟用此導航項目");
            entity.Property(e => e.parent_code).HasComment("父級導航項目編號，用於建立階層結構");
            entity.Property(e => e.sort_order)
                .HasDefaultValue(0)
                .HasComment("排序順序，數字越小越前面");
            entity.Property(e => e.title).HasComment("導航項目顯示標題");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("最後更新時間");
            entity.Property(e => e.url).HasComment("導航項目連結網址");

            entity.HasOne(d => d.parent_codeNavigation).WithMany(p => p.Inverseparent_codeNavigation)
                .HasPrincipalKey(p => p.code)
                .HasForeignKey(d => d.parent_code)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_sidebar_navigation_parent");
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

        modelBuilder.Entity<vw_sidebar_navigation>(entity =>
        {
            entity.ToView("vw_sidebar_navigation", "dbo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
