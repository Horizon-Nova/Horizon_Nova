using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// 作品集「類型」主表；用來區分不同內容型別（如 portfolio）。
/// </summary>
[Table("portfolio_types", Schema = "dbo")]
[Index("key", Name = "portfolio_types_key_key", IsUnique = true)]
public partial class portfolio_type
{
    /// <summary>
    /// 主鍵流水號。
    /// </summary>
    [Key]
    public int id { get; set; }

    /// <summary>
    /// 類型唯一識別鍵（例如：portfolio）。
    /// </summary>
    [StringLength(50)]
    public string key { get; set; } = null!;

    /// <summary>
    /// 類型顯示名稱。
    /// </summary>
    [StringLength(100)]
    public string name { get; set; } = null!;

    /// <summary>
    /// 建立時間（預設 CURRENT_TIMESTAMP）。
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? created_at { get; set; }

    [InverseProperty("type")]
    public virtual ICollection<portfolio_field> portfolio_fields { get; set; } = new List<portfolio_field>();

    [InverseProperty("type")]
    public virtual ICollection<portfolio_item> portfolio_items { get; set; } = new List<portfolio_item>();
}
