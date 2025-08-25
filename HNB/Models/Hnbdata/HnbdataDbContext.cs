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

    public virtual DbSet<project_asset> project_assets { get; set; }

    public virtual DbSet<project_link> project_links { get; set; }

    public virtual DbSet<project_section> project_sections { get; set; }

    public virtual DbSet<project_tag> project_tags { get; set; }

    public virtual DbSet<tag> tags { get; set; }

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
            entity.HasKey(e => e.project_id).HasName("projects_pkey");

            entity.ToTable("projects", "dbo", tb => tb.HasComment("作品主表"));

            entity.Property(e => e.project_id).HasComment("專案主鍵，自增流水號");
            entity.Property(e => e.category).HasComment("專案分類，用於分群與相關推薦");
            entity.Property(e => e.category_label).HasComment("類別顯示名稱（中文顯示，例如：手機APP、軟體系統、Web系統）");
            entity.Property(e => e.cover_url).HasComment("封面圖 URL");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasComment("建立時間");
            entity.Property(e => e.description).HasComment("專案簡介");
            entity.Property(e => e.features_json).HasComment("功能點（JSON 陣列），例：[條碼綁定啟動, 入出庫作業]");
            entity.Property(e => e.icon_bg_class).HasComment("icon 容器背景用的 Tailwind 類別，例如 bg-blue-50");
            entity.Property(e => e.icon_color_class).HasComment("icon 顏色用的 Tailwind 類別，例如 text-blue-600");
            entity.Property(e => e.icon_key).HasComment("卡片 icon（lucide 名稱），例如 camera / server / database");
            entity.Property(e => e.is_active)
                .HasDefaultValue(true)
                .HasComment("是否啟用");
            entity.Property(e => e.seo_description).HasComment("SEO 描述");
            entity.Property(e => e.seo_keywords).HasComment("SEO 關鍵字");
            entity.Property(e => e.slug).HasComment("路由短碼（唯一），例如 warehouse");
            entity.Property(e => e.sort_order)
                .HasDefaultValue(0)
                .HasComment("顯示排序，數字越小越前面");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'draft'::character varying")
                .HasComment("狀態：draft/published/archived");
            entity.Property(e => e.subtitle).HasComment("專案副標題");
            entity.Property(e => e.tech_stack_json).HasComment("技術棧（JSON 陣列），例：[ASP.NET MVC, EF Core]");
            entity.Property(e => e.title).HasComment("專案標題");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("now()")
                .HasComment("更新時間");
        });

        modelBuilder.Entity<project_asset>(entity =>
        {
            entity.HasKey(e => e.asset_id).HasName("project_assets_pkey");

            entity.ToTable("project_assets", "dbo", tb => tb.HasComment("作品相關的圖片/影片等資產"));

            entity.Property(e => e.asset_id).HasComment("資產主鍵，自增流水號");
            entity.Property(e => e.caption).HasComment("圖片或影片說明文字");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasComment("建立時間");
            entity.Property(e => e.height).HasComment("高度 (px)");
            entity.Property(e => e.is_active)
                .HasDefaultValue(true)
                .HasComment("是否啟用");
            entity.Property(e => e.mime_type).HasComment("媒體型態，如 image/png");
            entity.Property(e => e.project_id).HasComment("所屬專案 ID");
            entity.Property(e => e.sort_order)
                .HasDefaultValue(0)
                .HasComment("顯示排序");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("now()")
                .HasComment("更新時間");
            entity.Property(e => e.url).HasComment("資產檔案 URL");
            entity.Property(e => e.width).HasComment("寬度 (px)");

            entity.HasOne(d => d.project).WithMany(p => p.project_assets).HasConstraintName("project_assets_project_id_fkey");
        });

        modelBuilder.Entity<project_link>(entity =>
        {
            entity.HasKey(e => e.link_id).HasName("project_links_pkey");

            entity.ToTable("project_links", "dbo", tb => tb.HasComment("作品外部連結"));

            entity.Property(e => e.link_id).HasComment("連結主鍵，自增流水號");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasComment("建立時間");
            entity.Property(e => e.is_active)
                .HasDefaultValue(true)
                .HasComment("是否啟用");
            entity.Property(e => e.label).HasComment("顯示文字，例如 GitHub");
            entity.Property(e => e.link_type).HasComment("連結類型，如 repo/demo/doc/faq");
            entity.Property(e => e.project_id).HasComment("所屬專案 ID");
            entity.Property(e => e.sort_order)
                .HasDefaultValue(0)
                .HasComment("顯示排序");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("now()")
                .HasComment("更新時間");
            entity.Property(e => e.url).HasComment("連結 URL");

            entity.HasOne(d => d.project).WithMany(p => p.project_links).HasConstraintName("project_links_project_id_fkey");
        });

        modelBuilder.Entity<project_section>(entity =>
        {
            entity.HasKey(e => e.section_id).HasName("project_sections_pkey");

            entity.ToTable("project_sections", "dbo", tb => tb.HasComment("作品的章節/段落內容"));

            entity.Property(e => e.section_id).HasComment("段落主鍵，自增流水號");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasComment("建立時間");
            entity.Property(e => e.html_content).HasComment("HTML 內容，可直接渲染");
            entity.Property(e => e.is_active)
                .HasDefaultValue(true)
                .HasComment("是否啟用");
            entity.Property(e => e.markdown_content).HasComment("Markdown 內容，可轉換成 HTML");
            entity.Property(e => e.project_id).HasComment("所屬專案 ID，對應 projects.project_id");
            entity.Property(e => e.sort_order)
                .HasDefaultValue(0)
                .HasComment("章節排序，數字越小越前面");
            entity.Property(e => e.subtitle).HasComment("段落副標題");
            entity.Property(e => e.title).HasComment("段落標題");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("now()")
                .HasComment("更新時間");

            entity.HasOne(d => d.project).WithMany(p => p.project_sections).HasConstraintName("project_sections_project_id_fkey");
        });

        modelBuilder.Entity<project_tag>(entity =>
        {
            entity.HasKey(e => new { e.project_id, e.tag_id }).HasName("project_tags_pkey");

            entity.ToTable("project_tags", "dbo", tb => tb.HasComment("作品與標籤關聯（複合主鍵避免重複）"));

            entity.Property(e => e.project_id).HasComment("所屬專案 ID");
            entity.Property(e => e.tag_id).HasComment("所屬標籤 ID");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasComment("建立時間");

            entity.HasOne(d => d.project).WithMany(p => p.project_tags).HasConstraintName("project_tags_project_id_fkey");

            entity.HasOne(d => d.tag).WithMany(p => p.project_tags)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("project_tags_tag_id_fkey");
        });

        modelBuilder.Entity<tag>(entity =>
        {
            entity.HasKey(e => e.tag_id).HasName("tags_pkey");

            entity.ToTable("tags", "dbo", tb => tb.HasComment("標籤主表"));

            entity.Property(e => e.tag_id).HasComment("標籤主鍵，自增流水號");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasComment("建立時間");
            entity.Property(e => e.is_active)
                .HasDefaultValue(true)
                .HasComment("是否啟用");
            entity.Property(e => e.tag_name).HasComment("標籤名稱，例如 WMS");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
