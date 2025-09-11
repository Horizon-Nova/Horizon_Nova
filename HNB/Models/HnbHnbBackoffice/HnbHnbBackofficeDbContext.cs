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

    public virtual DbSet<department_closure_v> department_closure_vs { get; set; }

    public virtual DbSet<org_tree_v> org_tree_vs { get; set; }

    public virtual DbSet<person_relation_v> person_relation_vs { get; set; }

    public virtual DbSet<security_ip_key> security_ip_keys { get; set; }

    public virtual DbSet<system_config> system_configs { get; set; }

    public virtual DbSet<user_profile> user_profiles { get; set; }

    public virtual DbSet<vw_system_config_database> vw_system_config_databases { get; set; }

    public virtual DbSet<vw_system_config_notification> vw_system_config_notifications { get; set; }

    public virtual DbSet<vw_system_config_security> vw_system_config_securities { get; set; }

    public virtual DbSet<vw_system_config_server> vw_system_config_servers { get; set; }

    public virtual DbSet<vw_system_config_system> vw_system_config_systems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("dbo", "pgcrypto");

        modelBuilder.Entity<department_closure_v>(entity =>
        {
            entity.ToView("department_closure_v", "dbo");
        });

        modelBuilder.Entity<org_tree_v>(entity =>
        {
            entity.ToView("org_tree_v", "dbo");
        });

        modelBuilder.Entity<person_relation_v>(entity =>
        {
            entity.ToView("person_relation_v", "dbo");
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

        modelBuilder.Entity<user_profile>(entity =>
        {
            entity.HasKey(e => e.id).HasName("pk_user_profiles_id");

            entity.ToTable("user_profiles", "dbo", tb => tb.HasComment("員工基本資料表"));

            entity.Property(e => e.account)
                .HasComputedColumnSql("\nCASE\n    WHEN (POSITION(('@'::text) IN (email)) = 0) THEN ''::text\n    ELSE ('etatung\\'::text || split_part((email)::text, '@'::text, 1))\nEND", true)
                .HasComment("計算欄位：由 Email 自動產生 etatung\\帳號");
            entity.Property(e => e.agentendtime).HasComment("代理結束時間");
            entity.Property(e => e.agentid).HasComment("代理人員工ID");
            entity.Property(e => e.agentstarttime).HasComment("代理開始時間");
            entity.Property(e => e.alias).HasComment("員工代號/別名");
            entity.Property(e => e.area_cde)
                .IsFixedLength()
                .HasComment("區域代號");
            entity.Property(e => e.away).HasComment("是否離開/暫離");
            entity.Property(e => e.birthday).HasComment("生日");
            entity.Property(e => e.blood_type)
                .IsFixedLength()
                .HasComment("血型");
            entity.Property(e => e.callin_date).HasComment("調入日期");
            entity.Property(e => e.callout_date).HasComment("調出日期");
            entity.Property(e => e.capital_position1)
                .IsFixedLength()
                .HasComment("資本職位1（補充欄位）");
            entity.Property(e => e.capital_position2)
                .IsFixedLength()
                .HasComment("資本職位2（補充欄位）");
            entity.Property(e => e.center)
                .IsFixedLength()
                .HasComment("中心");
            entity.Property(e => e.comp_cde)
                .IsFixedLength()
                .HasComment("公司代號");
            entity.Property(e => e.comp_phone).HasComment("公司電話");
            entity.Property(e => e.constellation).HasComment("星座（依生日推算）");
            entity.Property(e => e.costcenter).HasComment("成本中心（ERP）");
            entity.Property(e => e.cp_date).HasComment("資本職位生效日");
            entity.Property(e => e.cr_date).HasComment("建立日期時間");
            entity.Property(e => e.cr_user).HasComment("建立者");
            entity.Property(e => e.datestamp).HasComment("最後異動時間");
            entity.Property(e => e.deptid).HasComment("部門代號（對應 department.id）");
            entity.Property(e => e.email).HasComment("電子郵件（公司信箱）");
            entity.Property(e => e.erp_id).HasComment("ERP 員工代號");
            entity.Property(e => e.extension)
                .HasDefaultValueSql("''::character varying")
                .HasComment("分機號碼（預設空字串）");
            entity.Property(e => e.isdeptmanager).HasComment("是否部門主管");
            entity.Property(e => e.job_status).HasComment("工作狀態（現職/離職）");
            entity.Property(e => e.job_type)
                .IsFixedLength()
                .HasComment("職務類別（全職/兼職等）");
            entity.Property(e => e.leave_date).HasComment("離職日期");
            entity.Property(e => e.leave_reason).HasComment("離職原因");
            entity.Property(e => e.level).HasComment("職等");
            entity.Property(e => e.manager).HasComment("是否主管");
            entity.Property(e => e.mobile).HasComment("行動電話");
            entity.Property(e => e.name).HasComment("中文姓名");
            entity.Property(e => e.name2).HasComment("英文姓名");
            entity.Property(e => e.nationality)
                .IsFixedLength()
                .HasComment("國籍");
            entity.Property(e => e.notifytype).HasComment("通知方式（系統設定）");
            entity.Property(e => e.password).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.phone).HasComment("公司電話");
            entity.Property(e => e.pid).HasComment("身分證字號");
            entity.Property(e => e.pluralism).HasComment("是否兼任");
            entity.Property(e => e.position1)
                .IsFixedLength()
                .HasComment("職務代碼1");
            entity.Property(e => e.position2)
                .IsFixedLength()
                .HasComment("職務代碼2");
            entity.Property(e => e.position_date).HasComment("職位生效日期");
            entity.Property(e => e.positioncode).HasComment("職位代碼");
            entity.Property(e => e.positioncode2).HasComment("第二職位代碼");
            entity.Property(e => e.regest_date).HasComment("任職日期");
            entity.Property(e => e.replace_date).HasComment("代理生效日期");
            entity.Property(e => e.salts).HasDefaultValueSql("'{}'::text[]");
            entity.Property(e => e.sex).HasComment("性別（M/F）");
            entity.Property(e => e.status).HasComment("狀態碼（在職/停用等）");
            entity.Property(e => e.title1)
                .IsFixedLength()
                .HasComment("職銜1");
            entity.Property(e => e.title2)
                .IsFixedLength()
                .HasComment("職銜2");
            entity.Property(e => e.titleid).HasComment("職稱代號");
            entity.Property(e => e.titlename).HasComment("職稱名稱");
            entity.Property(e => e.userstamp).HasComment("最後異動人");
            entity.Property(e => e.work_place)
                .IsFixedLength()
                .HasComment("工作地點");
            entity.Property(e => e.workers).HasComment("工種 / 工作類別");
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
