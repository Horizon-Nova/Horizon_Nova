using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HNB.Models;

public partial class RailwayContext : DbContext
{
    public RailwayContext(DbContextOptions<RailwayContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessRecord> AccessRecords { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("access_records_pkey");

            entity.ToTable("access_records", "hnb");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
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

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("error_logs_pkey");

            entity.ToTable("error_logs", "hnb");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Function)
                .HasMaxLength(255)
                .HasColumnName("function");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.StackTrace).HasColumnName("stack_trace");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
