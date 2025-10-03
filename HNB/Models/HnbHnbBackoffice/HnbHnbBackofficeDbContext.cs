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

            entity.ToTable("permission_management", "dbo", tb => tb.HasComment("權限管理統一資料表 - 用於統一管理用戶、角色、組織三種類型的資料，透過 type 欄位區分資料類型"));

            entity.HasIndex(e => e.navigation_permissions, "idx_permission_management_navigation_permissions").HasMethod("gin");

            entity.HasIndex(e => e.payment_methods, "idx_permission_management_payment_methods").HasMethod("gin");

            entity.HasIndex(e => e.preferences, "idx_permission_management_preferences").HasMethod("gin");

            entity.HasIndex(e => e.subscription_products, "idx_permission_management_subscription_products").HasMethod("gin");

            entity.Property(e => e.id).HasComment("主鍵ID");
            entity.Property(e => e.auto_renew)
                .HasDefaultValue(true)
                .HasComment("自動續費：true=自動續費, false=手動續費");
            entity.Property(e => e.avatar_url).HasComment("頭像網址：用戶頭像圖片連結");
            entity.Property(e => e.billing_cycle).HasComment("計費週期：monthly/yearly等");
            entity.Property(e => e.bio).HasComment("個人簡介：用戶的自我介紹");
            entity.Property(e => e.birthday).HasComment("生日：用戶出生日期");
            entity.Property(e => e.color_scheme).HasComment("色彩主題：用戶介面色彩配置");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasComment("建立時間：記錄建立時間");
            entity.Property(e => e.created_by).HasComment("建立者ID：建立此記錄的用戶ID");
            entity.Property(e => e.description).HasComment("描述：角色或組織的詳細說明");
            entity.Property(e => e.device_fingerprints).HasComment("設備指紋陣列：設備唯一識別碼列表");
            entity.Property(e => e.email).HasComment("電子郵件：主要用於用戶登入和聯絡");
            entity.Property(e => e.favorite_color).HasComment("喜愛的顏色：用戶偏好顏色");
            entity.Property(e => e.full_name).HasComment("完整名稱：用戶=真實姓名, 角色=角色完整名稱, 組織=組織完整名稱");
            entity.Property(e => e.gender).HasComment("性別：男/女/其他");
            entity.Property(e => e.internal_notes).HasComment("內部備註：僅管理員可見的內部備註");
            entity.Property(e => e.is_active)
                .HasDefaultValue(true)
                .HasComment("是否啟用：true=啟用, false=停用");
            entity.Property(e => e.is_email_verified)
                .HasDefaultValue(false)
                .HasComment("郵箱是否驗證：true=已驗證, false=未驗證");
            entity.Property(e => e.is_online)
                .HasDefaultValue(false)
                .HasComment("是否在線：true=在線, false=離線");
            entity.Property(e => e.is_phone_verified)
                .HasDefaultValue(false)
                .HasComment("電話是否驗證：true=已驗證, false=未驗證");
            entity.Property(e => e.language)
                .HasDefaultValueSql("'zh-TW'::character varying")
                .HasComment("語言設定：用戶介面語言");
            entity.Property(e => e.last_activity_at).HasComment("最後活動時間：最近一次活動時間");
            entity.Property(e => e.last_device_info).HasComment("最後設備資訊：JSON格式的設備詳細資訊");
            entity.Property(e => e.last_login_at).HasComment("最後登入時間：最近一次登入時間");
            entity.Property(e => e.last_login_ip).HasComment("最後登入IP：最近一次登入的IP地址");
            entity.Property(e => e.last_login_user_agent).HasComment("最後登入用戶代理：最近一次登入的瀏覽器資訊");
            entity.Property(e => e.last_password_change_at).HasComment("最後密碼變更時間：密碼最後修改時間");
            entity.Property(e => e.level)
                .HasDefaultValue(1)
                .HasComment("層級：組織=組織層級(1-10), 角色=角色層級, 用戶=用戶層級");
            entity.Property(e => e.location).HasComment("所在地：用戶居住或工作地點");
            entity.Property(e => e.login_count)
                .HasDefaultValue(0)
                .HasComment("登入次數：用戶總登入次數");
            entity.Property(e => e.login_method).HasComment("登入方式：local/oauth/google/facebook等");
            entity.Property(e => e.name).HasComment("名稱：用戶=username, 角色=角色名稱, 組織=組織名稱");
            entity.Property(e => e.navigation_permissions).HasComment("導航權限陣列：儲存sidebar_navigation表的code編號，用於控制用戶可訪問的頁面");
            entity.Property(e => e.nickname).HasComment("暱稱：用戶的顯示暱稱");
            entity.Property(e => e.notes).HasComment("備註：公開備註資訊");
            entity.Property(e => e.notification_settings).HasComment("通知設定：JSON格式的通知偏好設定");
            entity.Property(e => e.organization_names).HasComment("組織名稱陣列：組織名稱列表（用於顯示）");
            entity.Property(e => e.parent_id).HasComment("上級ID：用戶=所屬組織ID, 角色=所屬組織ID, 組織=上級組織ID");
            entity.Property(e => e.password_expires_at).HasComment("密碼到期時間：密碼過期日期");
            entity.Property(e => e.password_hash).HasComment("密碼雜湊值：用於用戶密碼驗證");
            entity.Property(e => e.payment_methods).HasComment("付款方式：JSON格式的付款方式資訊");
            entity.Property(e => e.permissions).HasComment("權限陣列：具體功能權限列表");
            entity.Property(e => e.phone).HasComment("電話號碼：用戶聯絡電話");
            entity.Property(e => e.preferences).HasComment("用戶偏好：JSON格式的個人偏好設定");
            entity.Property(e => e.privacy_settings).HasComment("隱私設定：JSON格式的隱私控制設定");
            entity.Property(e => e.profile_completion_percentage)
                .HasDefaultValue(0)
                .HasComment("資料完成度：個人資料完成百分比");
            entity.Property(e => e.role_names).HasComment("角色名稱陣列：角色名稱列表（用於顯示）");
            entity.Property(e => e.roles).HasComment("角色ID陣列：用戶=擁有的角色ID列表, 組織=分配的角色ID列表");
            entity.Property(e => e.salt).HasComment("密碼鹽值：用於密碼雜湊的隨機字串");
            entity.Property(e => e.security_questions).HasComment("安全問題：JSON格式的安全問題和答案");
            entity.Property(e => e.sort_order)
                .HasDefaultValue(0)
                .HasComment("排序順序：顯示順序編號");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'active'::character varying")
                .HasComment("狀態：active/inactive/suspended等");
            entity.Property(e => e.status_reason).HasComment("狀態原因：狀態變更的原因說明");
            entity.Property(e => e.subscription_expires_at).HasComment("訂閱到期時間：訂閱服務到期日期");
            entity.Property(e => e.subscription_products).HasComment("訂閱產品：JSON格式的訂閱產品資訊");
            entity.Property(e => e.subscription_status).HasComment("訂閱狀態：active/inactive/cancelled等");
            entity.Property(e => e.tags).HasComment("標籤陣列：用於分類和搜尋的標籤列表");
            entity.Property(e => e.theme)
                .HasDefaultValueSql("'auto'::character varying")
                .HasComment("主題設定：用戶介面主題");
            entity.Property(e => e.third_party_avatar).HasComment("第三方頭像：第三方登入的頭像網址");
            entity.Property(e => e.third_party_email).HasComment("第三方郵箱：第三方登入的郵箱地址");
            entity.Property(e => e.third_party_id).HasComment("第三方ID：OAuth等第三方登入的用戶ID");
            entity.Property(e => e.timezone)
                .HasDefaultValueSql("'Asia/Taipei'::character varying")
                .HasComment("時區：用戶所在時區");
            entity.Property(e => e.total_session_time).HasComment("總會話時間：用戶累計使用時間");
            entity.Property(e => e.trial_ends_at).HasComment("試用期結束時間：免費試用結束日期");
            entity.Property(e => e.trusted_devices).HasComment("信任設備陣列：已信任的設備ID列表");
            entity.Property(e => e.trusted_ips).HasComment("信任IP陣列：已信任的IP地址列表");
            entity.Property(e => e.two_factor_enabled)
                .HasDefaultValue(false)
                .HasComment("是否啟用雙因子認證：true=啟用, false=未啟用");
            entity.Property(e => e.two_factor_secret).HasComment("雙因子認證密鑰：TOTP密鑰");
            entity.Property(e => e.type).HasComment("資料類型：user=用戶, role=角色, organization=組織");
            entity.Property(e => e.updated_at).HasComment("更新時間：記錄最後更新時間");
            entity.Property(e => e.updated_by).HasComment("更新者ID：最後更新此記錄的用戶ID");
            entity.Property(e => e.user_names).HasComment("用戶名稱陣列：組織=所屬用戶名稱列表, 角色=擁有此角色的用戶名稱列表");
            entity.Property(e => e.zodiac_sign).HasComment("星座：用戶星座");

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

            entity.Property(e => e.assigned_roles).HasComment("分配的角色ID陣列：該組織下的所有角色ID");
            entity.Property(e => e.assigned_users).HasComment("所屬用戶ID陣列：該組織下的所有用戶ID");
            entity.Property(e => e.child_count).HasComment("子組織數量：自動計算該組織下有多少個子組織");
            entity.Property(e => e.child_organization_ids).HasComment("子組織ID陣列：該組織下的所有子組織ID");
            entity.Property(e => e.child_organizations).HasComment("子組織名稱陣列：該組織下的所有子組織名稱");
            entity.Property(e => e.created_at).HasComment("建立時間");
            entity.Property(e => e.created_by).HasComment("建立者ID");
            entity.Property(e => e.description).HasComment("組織描述");
            entity.Property(e => e.hierarchy_level).HasComment("階層等級：從level欄位取得");
            entity.Property(e => e.id).HasComment("主鍵ID");
            entity.Property(e => e.internal_notes).HasComment("內部備註");
            entity.Property(e => e.is_active).HasComment("是否啟用");
            entity.Property(e => e.is_root_organization).HasComment("是否為根組織：parent_id為NULL時為true");
            entity.Property(e => e.level).HasComment("組織層級");
            entity.Property(e => e.name).HasComment("組織名稱");
            entity.Property(e => e.notes).HasComment("備註");
            entity.Property(e => e.parent_id).HasComment("上級組織ID");
            entity.Property(e => e.parent_name).HasComment("上級組織名稱：從parent_id關聯取得");
            entity.Property(e => e.parent_organization_id).HasComment("上級組織ID：從parent_id取得");
            entity.Property(e => e.role_count).HasComment("角色數量：自動計算該組織下有多少個角色");
            entity.Property(e => e.role_names).HasComment("角色名稱陣列");
            entity.Property(e => e.sort_order).HasComment("排序順序");
            entity.Property(e => e.status).HasComment("狀態");
            entity.Property(e => e.tags).HasComment("標籤陣列");
            entity.Property(e => e.total_members_count).HasComment("總成員數量：角色數量+用戶數量");
            entity.Property(e => e.type).HasComment("資料類型：organization");
            entity.Property(e => e.updated_at).HasComment("更新時間");
            entity.Property(e => e.updated_by).HasComment("更新者ID");
            entity.Property(e => e.user_count).HasComment("用戶數量：自動計算該組織下有多少個用戶");
            entity.Property(e => e.user_names).HasComment("用戶名稱陣列");
        });

        modelBuilder.Entity<vw_permission_role>(entity =>
        {
            entity.ToView("vw_permission_role", "dbo");

            entity.Property(e => e.created_at).HasComment("建立時間");
            entity.Property(e => e.created_by).HasComment("建立者ID");
            entity.Property(e => e.description).HasComment("角色描述");
            entity.Property(e => e.id).HasComment("主鍵ID");
            entity.Property(e => e.internal_notes).HasComment("內部備註");
            entity.Property(e => e.is_active).HasComment("是否啟用");
            entity.Property(e => e.is_system_role).HasComment("是否為系統角色：type為system時為true");
            entity.Property(e => e.level).HasComment("角色層級");
            entity.Property(e => e.name).HasComment("角色名稱");
            entity.Property(e => e.navigation_permissions).HasComment("導航權限陣列：可訪問的導航項目");
            entity.Property(e => e.notes).HasComment("備註");
            entity.Property(e => e.organization_id).HasComment("所屬組織ID：從parent_id取得");
            entity.Property(e => e.organization_name).HasComment("所屬組織名稱：從parent_id關聯取得");
            entity.Property(e => e.parent_id).HasComment("所屬組織ID");
            entity.Property(e => e.permission_count).HasComment("權限數量：自動計算權限陣列長度");
            entity.Property(e => e.permission_names).HasComment("權限名稱陣列：從permissions欄位解析");
            entity.Property(e => e.permissions).HasComment("權限陣列");
            entity.Property(e => e.role_type).HasComment("角色類型：從type欄位取得");
            entity.Property(e => e.sort_order).HasComment("排序順序");
            entity.Property(e => e.status).HasComment("狀態");
            entity.Property(e => e.tags).HasComment("標籤陣列");
            entity.Property(e => e.type).HasComment("資料類型：role");
            entity.Property(e => e.updated_at).HasComment("更新時間");
            entity.Property(e => e.updated_by).HasComment("更新者ID");
            entity.Property(e => e.user_count).HasComment("用戶數量：自動計算擁有此角色的用戶數量");
            entity.Property(e => e.user_names).HasComment("用戶名稱陣列：擁有此角色的用戶名稱列表");
        });

        modelBuilder.Entity<vw_permission_user>(entity =>
        {
            entity.ToView("vw_permission_user", "dbo");

            entity.Property(e => e.assigned_roles_count).HasComment("分配的角色數量：從roles陣列長度計算");
            entity.Property(e => e.auto_renew).HasComment("自動續費");
            entity.Property(e => e.avatar_url).HasComment("頭像網址");
            entity.Property(e => e.billing_cycle).HasComment("計費週期");
            entity.Property(e => e.bio).HasComment("個人簡介");
            entity.Property(e => e.birthday).HasComment("生日");
            entity.Property(e => e.child_organizations_count).HasComment("管理的子組織數量：該用戶管理的子組織數量");
            entity.Property(e => e.color_scheme).HasComment("色彩主題");
            entity.Property(e => e.created_at).HasComment("建立時間");
            entity.Property(e => e.created_by).HasComment("建立者ID");
            entity.Property(e => e.email).HasComment("電子郵件");
            entity.Property(e => e.favorite_color).HasComment("喜愛的顏色");
            entity.Property(e => e.full_name).HasComment("完整名稱");
            entity.Property(e => e.gender).HasComment("性別");
            entity.Property(e => e.id).HasComment("主鍵ID");
            entity.Property(e => e.internal_notes).HasComment("內部備註");
            entity.Property(e => e.is_active).HasComment("是否啟用");
            entity.Property(e => e.is_email_verified).HasComment("郵箱是否驗證");
            entity.Property(e => e.is_online).HasComment("是否在線");
            entity.Property(e => e.is_phone_verified).HasComment("電話是否驗證");
            entity.Property(e => e.language).HasComment("語言設定");
            entity.Property(e => e.last_activity_at).HasComment("最後活動時間");
            entity.Property(e => e.last_device_info).HasComment("最後設備資訊");
            entity.Property(e => e.last_login_at).HasComment("最後登入時間");
            entity.Property(e => e.last_login_days_ago).HasComment("距離上次登入天數：計算欄位");
            entity.Property(e => e.last_login_ip).HasComment("最後登入IP");
            entity.Property(e => e.last_login_user_agent).HasComment("最後登入用戶代理");
            entity.Property(e => e.last_password_change_at).HasComment("最後密碼變更時間");
            entity.Property(e => e.level).HasComment("用戶層級");
            entity.Property(e => e.location).HasComment("所在地");
            entity.Property(e => e.login_count).HasComment("登入次數");
            entity.Property(e => e.login_method).HasComment("登入方式");
            entity.Property(e => e.nickname).HasComment("暱稱");
            entity.Property(e => e.notes).HasComment("備註");
            entity.Property(e => e.notification_settings).HasComment("通知設定");
            entity.Property(e => e.organization_id).HasComment("所屬組織ID：從parent_id取得");
            entity.Property(e => e.organization_name).HasComment("所屬組織名稱：從parent_id關聯取得");
            entity.Property(e => e.parent_id).HasComment("所屬組織ID");
            entity.Property(e => e.password_expires_at).HasComment("密碼到期時間");
            entity.Property(e => e.password_hash).HasComment("密碼雜湊值");
            entity.Property(e => e.payment_methods).HasComment("付款方式");
            entity.Property(e => e.permissions).HasComment("權限陣列");
            entity.Property(e => e.phone).HasComment("電話號碼");
            entity.Property(e => e.preferences).HasComment("用戶偏好");
            entity.Property(e => e.privacy_settings).HasComment("隱私設定");
            entity.Property(e => e.profile_completion_percentage).HasComment("資料完成度");
            entity.Property(e => e.role_ids).HasComment("角色ID陣列：從roles欄位解析");
            entity.Property(e => e.role_name).HasComment("主要角色名稱：從roles陣列取得第一個角色名稱");
            entity.Property(e => e.roles).HasComment("角色ID陣列");
            entity.Property(e => e.salt).HasComment("密碼鹽值");
            entity.Property(e => e.sort_order).HasComment("排序順序");
            entity.Property(e => e.status).HasComment("狀態");
            entity.Property(e => e.status_reason).HasComment("狀態原因");
            entity.Property(e => e.subscription_expires_at).HasComment("訂閱到期時間");
            entity.Property(e => e.subscription_products).HasComment("訂閱產品");
            entity.Property(e => e.subscription_status).HasComment("訂閱狀態");
            entity.Property(e => e.tags).HasComment("標籤陣列");
            entity.Property(e => e.theme).HasComment("主題設定");
            entity.Property(e => e.third_party_avatar).HasComment("第三方頭像");
            entity.Property(e => e.third_party_id).HasComment("第三方ID");
            entity.Property(e => e.timezone).HasComment("時區");
            entity.Property(e => e.total_session_time).HasComment("總會話時間");
            entity.Property(e => e.trial_ends_at).HasComment("試用期結束時間");
            entity.Property(e => e.trusted_devices).HasComment("信任設備陣列");
            entity.Property(e => e.trusted_ips).HasComment("信任IP陣列");
            entity.Property(e => e.two_factor_enabled).HasComment("是否啟用雙因子認證");
            entity.Property(e => e.type).HasComment("資料類型：user");
            entity.Property(e => e.updated_at).HasComment("更新時間");
            entity.Property(e => e.updated_by).HasComment("更新者ID");
            entity.Property(e => e.username).HasComment("用戶名稱：從name欄位取得");
            entity.Property(e => e.zodiac_sign).HasComment("星座");
        });

        modelBuilder.Entity<vw_sidebar_navigation>(entity =>
        {
            entity.ToView("vw_sidebar_navigation", "dbo");

            entity.Property(e => e.children).HasComment("子項目陣列：子導航項目的code陣列");
            entity.Property(e => e.children_count).HasComment("子項目數量：自動計算子導航項目數量");
            entity.Property(e => e.code).HasComment("導航項目編號");
            entity.Property(e => e.created_at).HasComment("建立時間");
            entity.Property(e => e.full_path).HasComment("完整路徑：階層路徑字串");
            entity.Property(e => e.hierarchy_level).HasComment("階層等級：1為根項目，2為子項目");
            entity.Property(e => e.icon).HasComment("導航項目圖示");
            entity.Property(e => e.id).HasComment("主鍵ID");
            entity.Property(e => e.is_active).HasComment("是否啟用");
            entity.Property(e => e.is_leaf).HasComment("是否為葉子節點：children_count = 0時為true");
            entity.Property(e => e.is_parent).HasComment("是否有子項目：children_count > 0時為true");
            entity.Property(e => e.parent_code).HasComment("父級導航項目編號");
            entity.Property(e => e.parent_title).HasComment("父級導航標題：從parent_code關聯取得");
            entity.Property(e => e.sort_order).HasComment("排序順序");
            entity.Property(e => e.title).HasComment("導航項目標題");
            entity.Property(e => e.updated_at).HasComment("更新時間");
            entity.Property(e => e.url).HasComment("導航項目連結");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
