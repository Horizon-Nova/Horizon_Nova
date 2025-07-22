using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HNB.Models;

public partial class HnbdataContext : DbContext
{
    public HnbdataContext(DbContextOptions<HnbdataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessRecord> AccessRecords { get; set; }

    public virtual DbSet<BlockedIp> BlockedIps { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<SysMenu> SysMenus { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("access_records_pkey");

            entity.ToTable("access_records", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            entity.Property(e => e.HttpMethod).HasColumnName("http_method");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.LogType)
                .HasDefaultValueSql("'authorize'::text")
                .HasColumnName("log_type");
            entity.Property(e => e.RequestBody).HasColumnName("request_body");
            entity.Property(e => e.RequestPath).HasColumnName("request_path");
            entity.Property(e => e.ResponseBody).HasColumnName("response_body");
            entity.Property(e => e.Result).HasColumnName("result");
            entity.Property(e => e.Roles).HasColumnName("roles");
            entity.Property(e => e.StatusCode).HasColumnName("status_code");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserName).HasColumnName("user_name");
        });

        modelBuilder.Entity<BlockedIp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("blocked_ips_pkey");

            entity.ToTable("blocked_ips", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.Reason).HasColumnName("reason");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("error_logs_pkey");

            entity.ToTable("error_logs", "dbo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Function)
                .HasMaxLength(255)
                .HasColumnName("function");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.StackTrace).HasColumnName("stack_trace");
        });

        modelBuilder.Entity<SysMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysMenu_pkey");

            entity.ToTable("SysMenu", "dbo", tb => tb.HasComment("選單表"));

            entity.Property(e => e.Authorize)
                .HasMaxLength(50)
                .HasComment("權限標識");
            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseModifyTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.MenuIcon)
                .HasMaxLength(50)
                .HasComment("選單圖示");
            entity.Property(e => e.MenuName)
                .HasMaxLength(50)
                .HasComment("選單名稱");
            entity.Property(e => e.MenuSort).HasComment("排序");
            entity.Property(e => e.MenuStatus).HasComment("狀態 (0 停用 1 啟用)");
            entity.Property(e => e.MenuTarget)
                .HasMaxLength(50)
                .HasComment("開啟方式");
            entity.Property(e => e.MenuType).HasComment("類型 (1 目錄 2 頁面 3 按鈕)");
            entity.Property(e => e.MenuUrl)
                .HasMaxLength(100)
                .HasComment("選單 URL");
            entity.Property(e => e.ParentId).HasComment("父選單 ID (0 表示根選單)");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .HasComment("備註");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
