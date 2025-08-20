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

    public virtual DbSet<portfolio_field> portfolio_fields { get; set; }

    public virtual DbSet<portfolio_item> portfolio_items { get; set; }

    public virtual DbSet<portfolio_type> portfolio_types { get; set; }

    public virtual DbSet<portfolio_value> portfolio_values { get; set; }

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

        modelBuilder.Entity<portfolio_field>(entity =>
        {
            entity.HasKey(e => e.id).HasName("portfolio_fields_pkey");

            entity.ToTable("portfolio_fields", "dbo", tb => tb.HasComment("欄位定義（元資料）表；定義某類型有哪些欄位與顯示/排序規則。"));

            entity.Property(e => e.id).HasComment("主鍵流水號。");
            entity.Property(e => e.data_type).HasComment("資料型態：text | number | bool | date | tags | json | image | url。");
            entity.Property(e => e.detail_order)
                .HasDefaultValue(100)
                .HasComment("詳細頁顯示順序（數值越小越前面）。");
            entity.Property(e => e.field_key).HasComment("程式用欄位鍵（建議英數/底線命名）；同一類型內需唯一。");
            entity.Property(e => e.is_required)
                .HasDefaultValue(false)
                .HasComment("是否必填。");
            entity.Property(e => e.label).HasComment("欄位顯示名稱（給使用者看的標籤）。");
            entity.Property(e => e.list_order)
                .HasDefaultValue(100)
                .HasComment("列表頁顯示順序（數值越小越前面）。");
            entity.Property(e => e.list_visible)
                .HasDefaultValue(true)
                .HasComment("列表頁是否顯示此欄位。");
            entity.Property(e => e.options).HasComment("附加選項（JSONB），例如下拉選項、格式規則、單位等。");
            entity.Property(e => e.type_id).HasComment("所屬類型 ID，對應 dbo.portfolio_types.id（刪除類型將連動刪除此類型的欄位）。");

            entity.HasOne(d => d.type).WithMany(p => p.portfolio_fields).HasConstraintName("portfolio_fields_type_id_fkey");
        });

        modelBuilder.Entity<portfolio_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("portfolio_items_pkey");

            entity.ToTable("portfolio_items", "dbo", tb => tb.HasComment("作品集內容主表；每一筆代表一個作品（項目）。"));

            entity.Property(e => e.id).HasComment("主鍵流水號。");
            entity.Property(e => e.category).HasComment("分類鍵（例如：mobile / software / web）。");
            entity.Property(e => e.cover_image).HasComment("封面圖 URL。");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("建立時間（預設 CURRENT_TIMESTAMP）。");
            entity.Property(e => e.slug).HasComment("項目唯一代稱（適用於 SEO/URL）。");
            entity.Property(e => e.summary).HasComment("摘要（列表頁可顯示）。");
            entity.Property(e => e.title).HasComment("主標題。");
            entity.Property(e => e.type_id).HasComment("所屬類型 ID，對應 dbo.portfolio_types.id（刪除類型將連動刪除此類型的項目）。");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新時間（預設 CURRENT_TIMESTAMP；請由程式或觸發器維護）。");

            entity.HasOne(d => d.type).WithMany(p => p.portfolio_items).HasConstraintName("portfolio_items_type_id_fkey");
        });

        modelBuilder.Entity<portfolio_type>(entity =>
        {
            entity.HasKey(e => e.id).HasName("portfolio_types_pkey");

            entity.ToTable("portfolio_types", "dbo", tb => tb.HasComment("作品集「類型」主表；用來區分不同內容型別（如 portfolio）。"));

            entity.Property(e => e.id).HasComment("主鍵流水號。");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("建立時間（預設 CURRENT_TIMESTAMP）。");
            entity.Property(e => e.key).HasComment("類型唯一識別鍵（例如：portfolio）。");
            entity.Property(e => e.name).HasComment("類型顯示名稱。");
        });

        modelBuilder.Entity<portfolio_value>(entity =>
        {
            entity.HasKey(e => new { e.item_id, e.field_id }).HasName("portfolio_values_pkey");

            entity.ToTable("portfolio_values", "dbo", tb => tb.HasComment("欄位值表（EAV）；儲存每個項目在各欄位的實際值。"));

            entity.Property(e => e.item_id).HasComment("項目 ID，對應 dbo.portfolio_items.id（ON DELETE CASCADE）。");
            entity.Property(e => e.field_id).HasComment("欄位 ID，對應 dbo.portfolio_fields.id（ON DELETE CASCADE）。");
            entity.Property(e => e.value_json).HasComment("實際值（JSONB）；依 data_type 分別可為字串、數字、布林、日期、陣列或物件。");

            entity.HasOne(d => d.field).WithMany(p => p.portfolio_values).HasConstraintName("portfolio_values_field_id_fkey");

            entity.HasOne(d => d.item).WithMany(p => p.portfolio_values).HasConstraintName("portfolio_values_item_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
