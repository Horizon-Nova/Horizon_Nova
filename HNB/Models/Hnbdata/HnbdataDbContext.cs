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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
