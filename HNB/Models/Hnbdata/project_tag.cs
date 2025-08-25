using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// 作品與標籤關聯（複合主鍵避免重複）
/// </summary>
[PrimaryKey("project_id", "tag_id")]
[Table("project_tags", Schema = "dbo")]
public partial class project_tag
{
    /// <summary>
    /// 所屬專案 ID
    /// </summary>
    [Key]
    public long project_id { get; set; }

    /// <summary>
    /// 所屬標籤 ID
    /// </summary>
    [Key]
    public int tag_id { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime? created_at { get; set; }

    [ForeignKey("project_id")]
    [InverseProperty("project_tags")]
    public virtual project project { get; set; } = null!;

    [ForeignKey("tag_id")]
    [InverseProperty("project_tags")]
    public virtual tag tag { get; set; } = null!;
}
