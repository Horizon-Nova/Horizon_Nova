using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// 欄位值表（EAV）；儲存每個項目在各欄位的實際值。
/// </summary>
[PrimaryKey("item_id", "field_id")]
[Table("portfolio_values", Schema = "dbo")]
[Index("field_id", Name = "idx_portfolio_values_field")]
public partial class portfolio_value
{
    /// <summary>
    /// 項目 ID，對應 dbo.portfolio_items.id（ON DELETE CASCADE）。
    /// </summary>
    [Key]
    public int item_id { get; set; }

    /// <summary>
    /// 欄位 ID，對應 dbo.portfolio_fields.id（ON DELETE CASCADE）。
    /// </summary>
    [Key]
    public int field_id { get; set; }

    /// <summary>
    /// 實際值（JSONB）；依 data_type 分別可為字串、數字、布林、日期、陣列或物件。
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string value_json { get; set; } = null!;

    [ForeignKey("field_id")]
    [InverseProperty("portfolio_values")]
    public virtual portfolio_field field { get; set; } = null!;

    [ForeignKey("item_id")]
    [InverseProperty("portfolio_values")]
    public virtual portfolio_item item { get; set; } = null!;
}
