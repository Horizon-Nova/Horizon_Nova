using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

/// <summary>
/// 欄位定義（元資料）表；定義某類型有哪些欄位與顯示/排序規則。
/// </summary>
[Table("portfolio_fields", Schema = "dbo")]
[Index("type_id", "field_key", Name = "portfolio_fields_type_id_field_key_key", IsUnique = true)]
public partial class portfolio_field
{
    /// <summary>
    /// 主鍵流水號。
    /// </summary>
    [Key]
    public int id { get; set; }

    /// <summary>
    /// 所屬類型 ID，對應 dbo.portfolio_types.id（刪除類型將連動刪除此類型的欄位）。
    /// </summary>
    public int type_id { get; set; }

    /// <summary>
    /// 程式用欄位鍵（建議英數/底線命名）；同一類型內需唯一。
    /// </summary>
    [StringLength(100)]
    public string field_key { get; set; } = null!;

    /// <summary>
    /// 欄位顯示名稱（給使用者看的標籤）。
    /// </summary>
    [StringLength(100)]
    public string label { get; set; } = null!;

    /// <summary>
    /// 資料型態：text | number | bool | date | tags | json | image | url。
    /// </summary>
    [StringLength(20)]
    public string data_type { get; set; } = null!;

    /// <summary>
    /// 是否必填。
    /// </summary>
    public bool is_required { get; set; }

    /// <summary>
    /// 列表頁是否顯示此欄位。
    /// </summary>
    public bool list_visible { get; set; }

    /// <summary>
    /// 列表頁顯示順序（數值越小越前面）。
    /// </summary>
    public int list_order { get; set; }

    /// <summary>
    /// 詳細頁顯示順序（數值越小越前面）。
    /// </summary>
    public int detail_order { get; set; }

    /// <summary>
    /// 附加選項（JSONB），例如下拉選項、格式規則、單位等。
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? options { get; set; }

    [InverseProperty("field")]
    public virtual ICollection<portfolio_value> portfolio_values { get; set; } = new List<portfolio_value>();

    [ForeignKey("type_id")]
    [InverseProperty("portfolio_fields")]
    public virtual portfolio_type type { get; set; } = null!;
}
