using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// 作品外部連結
/// </summary>
[Table("project_links", Schema = "dbo")]
public partial class project_link
{
    /// <summary>
    /// 連結主鍵，自增流水號
    /// </summary>
    [Key]
    public long link_id { get; set; }

    /// <summary>
    /// 所屬專案 ID
    /// </summary>
    public long project_id { get; set; }

    /// <summary>
    /// 顯示文字，例如 GitHub
    /// </summary>
    [StringLength(100)]
    public string label { get; set; } = null!;

    /// <summary>
    /// 連結 URL
    /// </summary>
    [StringLength(500)]
    public string url { get; set; } = null!;

    /// <summary>
    /// 連結類型，如 repo/demo/doc/faq
    /// </summary>
    [StringLength(50)]
    public string? link_type { get; set; }

    /// <summary>
    /// 顯示排序
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

    [ForeignKey("project_id")]
    [InverseProperty("project_links")]
    public virtual project project { get; set; } = null!;
}
