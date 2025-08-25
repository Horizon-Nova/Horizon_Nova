using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// 標籤主表
/// </summary>
[Table("tags", Schema = "dbo")]
[Index("tag_name", Name = "tags_tag_name_key", IsUnique = true)]
public partial class tag
{
    /// <summary>
    /// 標籤主鍵，自增流水號
    /// </summary>
    [Key]
    public int tag_id { get; set; }

    /// <summary>
    /// 標籤名稱，例如 WMS
    /// </summary>
    [StringLength(100)]
    public string tag_name { get; set; } = null!;

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? is_active { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime? created_at { get; set; }

    [InverseProperty("tag")]
    public virtual ICollection<project_tag> project_tags { get; set; } = new List<project_tag>();
}
