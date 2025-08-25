using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// 作品的章節/段落內容
/// </summary>
[Table("project_sections", Schema = "dbo")]
public partial class project_section
{
    /// <summary>
    /// 段落主鍵，自增流水號
    /// </summary>
    [Key]
    public long section_id { get; set; }

    /// <summary>
    /// 所屬專案 ID，對應 projects.project_id
    /// </summary>
    public long project_id { get; set; }

    /// <summary>
    /// 段落標題
    /// </summary>
    [StringLength(200)]
    public string? title { get; set; }

    /// <summary>
    /// 段落副標題
    /// </summary>
    [StringLength(300)]
    public string? subtitle { get; set; }

    /// <summary>
    /// HTML 內容，可直接渲染
    /// </summary>
    public string? html_content { get; set; }

    /// <summary>
    /// Markdown 內容，可轉換成 HTML
    /// </summary>
    public string? markdown_content { get; set; }

    /// <summary>
    /// 章節排序，數字越小越前面
    /// </summary>
    public int? sort_order { get; set; }

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

    [StringLength(50)]
    public string? section_type { get; set; }

    [ForeignKey("project_id")]
    [InverseProperty("project_sections")]
    public virtual project project { get; set; } = null!;
}
