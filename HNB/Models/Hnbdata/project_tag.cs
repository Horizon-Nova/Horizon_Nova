using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// ProjectTags（畫面 icon 專區）
/// </summary>
[Table("project_tags", Schema = "dbo")]
[Index("category", Name = "project_tags_category_key", IsUnique = true)]
public partial class project_tag
{
    [Key]
    public int id { get; set; }

    /// <summary>
    /// 專案大類（例如 手機APP、軟體系統、Web系統）
    /// </summary>
    [StringLength(100)]
    public string category { get; set; } = null!;

    /// <summary>
    /// 專案大類 icon 名稱
    /// </summary>
    [StringLength(255)]
    public string? icon { get; set; }

    /// <summary>
    /// 專案大類 icon 顏色 (#HEX 或文字)
    /// </summary>
    [StringLength(50)]
    public string? icon_color { get; set; }

    /// <summary>
    /// 創建時間
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime created_at { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime updated_at { get; set; }

    [StringLength(50)]
    public string code { get; set; } = null!;
}
