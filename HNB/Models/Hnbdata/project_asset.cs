using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// 作品相關的圖片/影片等資產
/// </summary>
[Table("project_assets", Schema = "dbo")]
public partial class project_asset
{
    /// <summary>
    /// 資產主鍵，自增流水號
    /// </summary>
    [Key]
    public long asset_id { get; set; }

    /// <summary>
    /// 所屬專案 ID
    /// </summary>
    public long project_id { get; set; }

    /// <summary>
    /// 資產檔案 URL
    /// </summary>
    [StringLength(500)]
    public string url { get; set; } = null!;

    /// <summary>
    /// 圖片或影片說明文字
    /// </summary>
    [StringLength(200)]
    public string? caption { get; set; }

    /// <summary>
    /// 媒體型態，如 image/png
    /// </summary>
    [StringLength(100)]
    public string? mime_type { get; set; }

    /// <summary>
    /// 寬度 (px)
    /// </summary>
    public int? width { get; set; }

    /// <summary>
    /// 高度 (px)
    /// </summary>
    public int? height { get; set; }

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
    [InverseProperty("project_assets")]
    public virtual project project { get; set; } = null!;
}
