using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// 作品集內容主表；每一筆代表一個作品（項目）。
/// </summary>
[Table("portfolio_items", Schema = "dbo")]
[Index("type_id", Name = "idx_portfolio_items_type")]
[Index("slug", Name = "portfolio_items_slug_key", IsUnique = true)]
public partial class portfolio_item
{
    /// <summary>
    /// 主鍵流水號。
    /// </summary>
    [Key]
    public int id { get; set; }

    /// <summary>
    /// 所屬類型 ID，對應 dbo.portfolio_types.id（刪除類型將連動刪除此類型的項目）。
    /// </summary>
    public int type_id { get; set; }

    /// <summary>
    /// 項目唯一代稱（適用於 SEO/URL）。
    /// </summary>
    [StringLength(120)]
    public string slug { get; set; } = null!;

    /// <summary>
    /// 主標題。
    /// </summary>
    [StringLength(200)]
    public string title { get; set; } = null!;

    /// <summary>
    /// 摘要（列表頁可顯示）。
    /// </summary>
    public string? summary { get; set; }

    /// <summary>
    /// 封面圖 URL。
    /// </summary>
    [StringLength(400)]
    public string? cover_image { get; set; }

    /// <summary>
    /// 分類鍵（例如：mobile / software / web）。
    /// </summary>
    [StringLength(50)]
    public string? category { get; set; }

    /// <summary>
    /// 建立時間（預設 CURRENT_TIMESTAMP）。
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? created_at { get; set; }

    /// <summary>
    /// 更新時間（預設 CURRENT_TIMESTAMP；請由程式或觸發器維護）。
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? updated_at { get; set; }

    [InverseProperty("item")]
    public virtual ICollection<portfolio_value> portfolio_values { get; set; } = new List<portfolio_value>();

    [ForeignKey("type_id")]
    [InverseProperty("portfolio_items")]
    public virtual portfolio_type type { get; set; } = null!;
}
