using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// 作品主表
/// </summary>
[Table("projects", Schema = "dbo")]
[Index("slug", Name = "projects_slug_key", IsUnique = true)]
public partial class project
{
    /// <summary>
    /// 專案主鍵，自增流水號
    /// </summary>
    [Key]
    public long project_id { get; set; }

    /// <summary>
    /// 路由短碼（唯一），例如 warehouse
    /// </summary>
    [StringLength(100)]
    public string slug { get; set; } = null!;

    /// <summary>
    /// 專案標題
    /// </summary>
    [StringLength(200)]
    public string title { get; set; } = null!;

    /// <summary>
    /// 專案副標題
    /// </summary>
    [StringLength(300)]
    public string? subtitle { get; set; }

    /// <summary>
    /// 專案分類，用於分群與相關推薦
    /// </summary>
    [StringLength(100)]
    public string? category { get; set; }

    /// <summary>
    /// 專案簡介
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// 技術棧（JSON 陣列），例：[ASP.NET MVC, EF Core]
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? tech_stack_json { get; set; }

    /// <summary>
    /// 功能點（JSON 陣列），例：[條碼綁定啟動, 入出庫作業]
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? features_json { get; set; }

    /// <summary>
    /// 封面圖 URL
    /// </summary>
    [StringLength(500)]
    public string? cover_url { get; set; }

    /// <summary>
    /// SEO 關鍵字
    /// </summary>
    [StringLength(300)]
    public string? seo_keywords { get; set; }

    /// <summary>
    /// SEO 描述
    /// </summary>
    [StringLength(500)]
    public string? seo_description { get; set; }

    /// <summary>
    /// 顯示排序，數字越小越前面
    /// </summary>
    public int? sort_order { get; set; }

    /// <summary>
    /// 狀態：draft/published/archived
    /// </summary>
    [StringLength(20)]
    public string? status { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? is_active { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? updated_at { get; set; }

    /// <summary>
    /// 卡片 icon（lucide 名稱），例如 camera / server / database
    /// </summary>
    [StringLength(50)]
    public string? icon_key { get; set; }

    /// <summary>
    /// icon 容器背景用的 Tailwind 類別，例如 bg-blue-50
    /// </summary>
    [StringLength(50)]
    public string? icon_bg_class { get; set; }

    /// <summary>
    /// icon 顏色用的 Tailwind 類別，例如 text-blue-600
    /// </summary>
    [StringLength(50)]
    public string? icon_color_class { get; set; }

    /// <summary>
    /// 類別顯示名稱（中文顯示，例如：手機APP、軟體系統、Web系統）
    /// </summary>
    [StringLength(50)]
    public string? category_label { get; set; }

    public string? introduction { get; set; }

    [Column(TypeName = "jsonb")]
    public string? challenges_json { get; set; }

    [Column(TypeName = "jsonb")]
    public string? solutions_json { get; set; }

    [StringLength(50)]
    public string? duration { get; set; }

    public int? team_size { get; set; }

    [StringLength(200)]
    public string? client_name { get; set; }

    [StringLength(50)]
    public string? project_status { get; set; }

    [InverseProperty("project")]
    public virtual ICollection<project_asset> project_assets { get; set; } = new List<project_asset>();

    [InverseProperty("project")]
    public virtual ICollection<project_link> project_links { get; set; } = new List<project_link>();

    [InverseProperty("project")]
    public virtual ICollection<project_section> project_sections { get; set; } = new List<project_section>();

    [InverseProperty("project")]
    public virtual ICollection<project_tag> project_tags { get; set; } = new List<project_tag>();
}
