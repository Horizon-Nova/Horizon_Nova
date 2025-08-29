using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

public partial class HnbdataDbContext : DbContext
{
    public HnbdataDbContext(DbContextOptions<HnbdataDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<access_record> access_records { get; set; }

    public virtual DbSet<blocked_ip> blocked_ips { get; set; }

    public virtual DbSet<error_log> error_logs { get; set; }

    public virtual DbSet<project> projects { get; set; }

    public virtual DbSet<project_tag> project_tags { get; set; }

    public virtual DbSet<team_member> team_members { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<access_record>(entity =>
        {
            entity.HasKey(e => e.id).HasName("access_records_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.log_type).HasDefaultValueSql("'authorize'::text");
        });

        modelBuilder.Entity<blocked_ip>(entity =>
        {
            entity.HasKey(e => e.id).HasName("blocked_ips_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<error_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("error_logs_pkey");

            entity.Property(e => e.id).HasComment("主鍵");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("紀錄建立時間");
            entity.Property(e => e.extra).HasComment("附加資料（如 headers, query, ip, user-agent）");
            entity.Property(e => e.function).HasComment("簡易函數描述（例如 Controller/Action）");
            entity.Property(e => e.function_full).HasComment("方法全名，例如 Namespace.Controller.Action()");
            entity.Property(e => e.http_method).HasComment("HTTP 方法（GET/POST/PUT/...）");
            entity.Property(e => e.layer).HasComment("層級名稱：Middleware / Filter / ExceptionHandler / Background");
            entity.Property(e => e.message).HasComment("錯誤訊息（完整 ToString）");
            entity.Property(e => e.path).HasComment("請求路徑（Request.Path）");
            entity.Property(e => e.stack_trace).HasComment("堆疊追蹤");
            entity.Property(e => e.stage).HasComment("層級代碼：0=Middleware, 1=Filter, 2=ExceptionHandler, 3=Background");
            entity.Property(e => e.status_code).HasComment("回應狀態碼（500/404/...）");
            entity.Property(e => e.trace_id).HasComment("請求唯一識別碼（HttpContext.TraceIdentifier）");
            entity.Property(e => e.user_id).HasComment("使用者 ID（HttpContext.User.Identity.Name）");
        });

        modelBuilder.Entity<project>(entity =>
        {
            entity.HasKey(e => e.id).HasName("project_pkey");

            entity.ToTable("project", "dbo", tb => tb.HasComment("Project（專案區）"));

            entity.Property(e => e.arch_img).HasComment("技術架構圖檔名");
            entity.Property(e => e.category).HasComment("專案大類");
            entity.Property(e => e.challenges).HasComment("主要挑戰");
            entity.Property(e => e.client).HasComment("客戶名稱（如需 FK 可改接 Client 表）");
            entity.Property(e => e.dev_tools).HasComment("開發工具");
            entity.Property(e => e.duration).HasComment("開發時程（例如：6個月）");
            entity.Property(e => e.feature_intro).HasComment("功能特色簡介");
            entity.Property(e => e.features).HasComment("主要功能（可逗號分隔）");
            entity.Property(e => e.feedback).HasComment("客戶回饋（JSON）");
            entity.Property(e => e.highlight).HasComment("專案亮點（可逗號分隔）");
            entity.Property(e => e.icon).HasComment("專案 icon 名稱或檔名");
            entity.Property(e => e.icon_color).HasComment("專案 icon 顏色（例如 #FF5733 或 blue）");
            entity.Property(e => e.intro).HasComment("專案介紹（詳細）");
            entity.Property(e => e.main_features).HasComment("功能主特色");
            entity.Property(e => e.name).HasComment("專案名稱（主鍵）");
            entity.Property(e => e.outcome_performance).HasComment("專案成果績效（量化）");
            entity.Property(e => e.outcome_summary).HasComment("專案成果簡介");
            entity.Property(e => e.screenshots).HasComment("專案截圖檔名（多筆以逗號分隔）");
            entity.Property(e => e.solution).HasComment("解決方案");
            entity.Property(e => e.status).HasComment("專案狀態（例如：已完成並上線）");
            entity.Property(e => e.summary).HasComment("專案簡介");
            entity.Property(e => e.team_size).HasComment("團隊規模（人數）");
            entity.Property(e => e.tech_stack).HasComment("技術棧");
        });

        modelBuilder.Entity<project_tag>(entity =>
        {
            entity.HasKey(e => e.id).HasName("project_tags_pkey");

            entity.ToTable("project_tags", "dbo", tb => tb.HasComment("ProjectTags（畫面 icon 專區）"));

            entity.Property(e => e.category).HasComment("專案大類（例如 手機APP、軟體系統、Web系統）");
            entity.Property(e => e.code).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasComment("創建時間");
            entity.Property(e => e.icon).HasComment("專案大類 icon 名稱");
            entity.Property(e => e.icon_color).HasComment("專案大類 icon 顏色 (#HEX 或文字)");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("now()")
                .HasComment("更新時間");
        });

        modelBuilder.Entity<team_member>(entity =>
        {
            entity.HasKey(e => e.id).HasName("team_member_pkey");

            entity.Property(e => e.is_active).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
