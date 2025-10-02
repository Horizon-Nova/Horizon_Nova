using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

/// <summary>
/// 側欄導航管理表
/// </summary>
[Table("sidebar_navigation", Schema = "dbo")]
[Index("code", Name = "idx_sidebar_navigation_code")]
[Index("parent_code", Name = "idx_sidebar_navigation_parent_code")]
[Index("code", Name = "sidebar_navigation_code_key", IsUnique = true)]
public partial class sidebar_navigation
{
    /// <summary>
    /// 主鍵，自動遞增
    /// </summary>
    [Key]
    public int id { get; set; }

    /// <summary>
    /// 導航項目編號，唯一識別碼
    /// </summary>
    [StringLength(50)]
    public string code { get; set; } = null!;

    /// <summary>
    /// 導航項目顯示標題
    /// </summary>
    [StringLength(100)]
    public string title { get; set; } = null!;

    /// <summary>
    /// 導航項目連結網址
    /// </summary>
    [StringLength(500)]
    public string url { get; set; } = null!;

    /// <summary>
    /// 導航項目圖示名稱
    /// </summary>
    [StringLength(50)]
    public string? icon { get; set; }

    /// <summary>
    /// 排序順序，數字越小越前面
    /// </summary>
    public int? sort_order { get; set; }

    /// <summary>
    /// 父級導航項目編號，用於建立階層結構
    /// </summary>
    [StringLength(50)]
    public string? parent_code { get; set; }

    /// <summary>
    /// 是否啟用此導航項目
    /// </summary>
    public bool? is_active { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime? created_at { get; set; }

    /// <summary>
    /// 最後更新時間
    /// </summary>
    public DateTime? updated_at { get; set; }

    [InverseProperty("parent_codeNavigation")]
    public virtual ICollection<sidebar_navigation> Inverseparent_codeNavigation { get; set; } = new List<sidebar_navigation>();

    [ForeignKey("parent_code")]
    [InverseProperty("Inverseparent_codeNavigation")]
    public virtual sidebar_navigation? parent_codeNavigation { get; set; }
}
