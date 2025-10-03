using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_permission_organization
{
    /// <summary>
    /// 主鍵ID
    /// </summary>
    public int? id { get; set; }

    /// <summary>
    /// 資料類型：organization
    /// </summary>
    [StringLength(50)]
    public string? type { get; set; }

    /// <summary>
    /// 組織名稱
    /// </summary>
    [StringLength(100)]
    public string? name { get; set; }

    /// <summary>
    /// 組織描述
    /// </summary>
    [StringLength(500)]
    public string? description { get; set; }

    /// <summary>
    /// 組織層級
    /// </summary>
    public int? level { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int? sort_order { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? is_active { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    [StringLength(50)]
    public string? status { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? created_at { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? updated_at { get; set; }

    /// <summary>
    /// 標籤陣列
    /// </summary>
    public List<string>? tags { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? notes { get; set; }

    /// <summary>
    /// 上級組織ID
    /// </summary>
    public int? parent_id { get; set; }

    /// <summary>
    /// 建立者ID
    /// </summary>
    public int? created_by { get; set; }

    /// <summary>
    /// 更新者ID
    /// </summary>
    public int? updated_by { get; set; }

    /// <summary>
    /// 內部備註
    /// </summary>
    public string? internal_notes { get; set; }

    /// <summary>
    /// 用戶名稱陣列
    /// </summary>
    public List<string>? user_names { get; set; }

    /// <summary>
    /// 角色名稱陣列
    /// </summary>
    public List<string>? role_names { get; set; }

    /// <summary>
    /// 角色數量：自動計算該組織下有多少個角色
    /// </summary>
    public long? role_count { get; set; }

    /// <summary>
    /// 用戶數量：自動計算該組織下有多少個用戶
    /// </summary>
    public long? user_count { get; set; }

    /// <summary>
    /// 子組織數量：自動計算該組織下有多少個子組織
    /// </summary>
    public long? child_count { get; set; }

    /// <summary>
    /// 上級組織名稱：從parent_id關聯取得
    /// </summary>
    [StringLength(100)]
    public string? parent_name { get; set; }

    /// <summary>
    /// 上級組織ID：從parent_id取得
    /// </summary>
    public int? parent_organization_id { get; set; }

    /// <summary>
    /// 子組織名稱陣列：該組織下的所有子組織名稱
    /// </summary>
    [Column(TypeName = "character varying[]")]
    public List<string>? child_organizations { get; set; }

    /// <summary>
    /// 子組織ID陣列：該組織下的所有子組織ID
    /// </summary>
    public List<string>? child_organization_ids { get; set; }

    /// <summary>
    /// 分配的角色ID陣列：該組織下的所有角色ID
    /// </summary>
    public List<string>? assigned_roles { get; set; }

    /// <summary>
    /// 所屬用戶ID陣列：該組織下的所有用戶ID
    /// </summary>
    public List<string>? assigned_users { get; set; }

    /// <summary>
    /// 階層等級：從level欄位取得
    /// </summary>
    public int? hierarchy_level { get; set; }

    /// <summary>
    /// 是否為根組織：parent_id為NULL時為true
    /// </summary>
    public bool? is_root_organization { get; set; }

    /// <summary>
    /// 總成員數量：角色數量+用戶數量
    /// </summary>
    public long? total_members_count { get; set; }
}
