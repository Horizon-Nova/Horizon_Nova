using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

/// <summary>
/// 員工基本資料表
/// </summary>
[Table("user_profiles", Schema = "dbo")]
public partial class user_profile
{
    /// <summary>
    /// 員工代號/別名
    /// </summary>
    [StringLength(20)]
    public string alias { get; set; } = null!;

    /// <summary>
    /// 中文姓名
    /// </summary>
    [StringLength(100)]
    public string? name { get; set; }

    /// <summary>
    /// 英文姓名
    /// </summary>
    [StringLength(100)]
    public string? name2 { get; set; }

    /// <summary>
    /// 部門代號（對應 department.id）
    /// </summary>
    [StringLength(100)]
    public string? deptid { get; set; }

    /// <summary>
    /// 職稱代號
    /// </summary>
    [StringLength(100)]
    public string titleid { get; set; } = null!;

    /// <summary>
    /// 職稱名稱
    /// </summary>
    [StringLength(100)]
    public string? titlename { get; set; }

    /// <summary>
    /// 職等
    /// </summary>
    public int level { get; set; }

    /// <summary>
    /// 職位代碼
    /// </summary>
    public int positioncode { get; set; }

    /// <summary>
    /// 代理人員工ID
    /// </summary>
    [StringLength(10)]
    public string? agentid { get; set; }

    /// <summary>
    /// 代理開始時間
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? agentstarttime { get; set; }

    /// <summary>
    /// 代理結束時間
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? agentendtime { get; set; }

    /// <summary>
    /// 是否離開/暫離
    /// </summary>
    public bool away { get; set; }

    /// <summary>
    /// 電子郵件（公司信箱）
    /// </summary>
    [StringLength(100)]
    public string? email { get; set; }

    /// <summary>
    /// 公司電話
    /// </summary>
    [StringLength(100)]
    public string? phone { get; set; }

    /// <summary>
    /// 行動電話
    /// </summary>
    [StringLength(100)]
    public string? mobile { get; set; }

    /// <summary>
    /// 成本中心（ERP）
    /// </summary>
    [StringLength(20)]
    public string? costcenter { get; set; }

    /// <summary>
    /// 通知方式（系統設定）
    /// </summary>
    public int notifytype { get; set; }

    /// <summary>
    /// 是否兼任
    /// </summary>
    public bool pluralism { get; set; }

    /// <summary>
    /// 第二職位代碼
    /// </summary>
    [StringLength(8)]
    public string? positioncode2 { get; set; }

    /// <summary>
    /// 是否主管
    /// </summary>
    public bool? manager { get; set; }

    /// <summary>
    /// 是否部門主管
    /// </summary>
    public bool? isdeptmanager { get; set; }

    /// <summary>
    /// 狀態碼（在職/停用等）
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// 身分證字號
    /// </summary>
    [StringLength(20)]
    public string pid { get; set; } = null!;

    /// <summary>
    /// 生日
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime birthday { get; set; }

    /// <summary>
    /// 星座（依生日推算）
    /// </summary>
    [StringLength(12)]
    public string? constellation { get; set; }

    /// <summary>
    /// 性別（M/F）
    /// </summary>
    [MaxLength(1)]
    public char sex { get; set; }

    /// <summary>
    /// 工作狀態（現職/離職）
    /// </summary>
    [StringLength(10)]
    public string job_status { get; set; } = null!;

    /// <summary>
    /// 任職日期
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? regest_date { get; set; }

    /// <summary>
    /// 調入日期
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? callin_date { get; set; }

    /// <summary>
    /// 調出日期
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? callout_date { get; set; }

    /// <summary>
    /// 代理生效日期
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? replace_date { get; set; }

    /// <summary>
    /// 離職日期
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? leave_date { get; set; }

    /// <summary>
    /// 離職原因
    /// </summary>
    [StringLength(100)]
    public string? leave_reason { get; set; }

    /// <summary>
    /// 公司代號
    /// </summary>
    [StringLength(10)]
    public string? comp_cde { get; set; }

    /// <summary>
    /// 區域代號
    /// </summary>
    [StringLength(10)]
    public string? area_cde { get; set; }

    /// <summary>
    /// 職務類別（全職/兼職等）
    /// </summary>
    [StringLength(10)]
    public string? job_type { get; set; }

    /// <summary>
    /// 職銜1
    /// </summary>
    [StringLength(10)]
    public string? title1 { get; set; }

    /// <summary>
    /// 職銜2
    /// </summary>
    [StringLength(10)]
    public string? title2 { get; set; }

    /// <summary>
    /// 資本職位1（補充欄位）
    /// </summary>
    [StringLength(10)]
    public string? capital_position1 { get; set; }

    /// <summary>
    /// 資本職位2（補充欄位）
    /// </summary>
    [StringLength(10)]
    public string? capital_position2 { get; set; }

    /// <summary>
    /// 資本職位生效日
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? cp_date { get; set; }

    /// <summary>
    /// 職務代碼1
    /// </summary>
    [StringLength(10)]
    public string? position1 { get; set; }

    /// <summary>
    /// 職務代碼2
    /// </summary>
    [StringLength(10)]
    public string? position2 { get; set; }

    /// <summary>
    /// 職位生效日期
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? position_date { get; set; }

    /// <summary>
    /// 公司電話
    /// </summary>
    [StringLength(20)]
    public string? comp_phone { get; set; }

    /// <summary>
    /// 分機號碼（預設空字串）
    /// </summary>
    [StringLength(50)]
    public string extension { get; set; } = null!;

    /// <summary>
    /// 國籍
    /// </summary>
    [StringLength(10)]
    public string? nationality { get; set; }

    /// <summary>
    /// 血型
    /// </summary>
    [StringLength(2)]
    public string? blood_type { get; set; }

    /// <summary>
    /// 建立者
    /// </summary>
    [StringLength(50)]
    public string cr_user { get; set; } = null!;

    /// <summary>
    /// 建立日期時間
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime cr_date { get; set; }

    /// <summary>
    /// 最後異動人
    /// </summary>
    [StringLength(50)]
    public string? userstamp { get; set; }

    /// <summary>
    /// 最後異動時間
    /// </summary>
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? datestamp { get; set; }

    /// <summary>
    /// 工作地點
    /// </summary>
    [StringLength(100)]
    public string? work_place { get; set; }

    /// <summary>
    /// ERP 員工代號
    /// </summary>
    [StringLength(10)]
    public string? erp_id { get; set; }

    /// <summary>
    /// 工種 / 工作類別
    /// </summary>
    [StringLength(50)]
    public string? workers { get; set; }

    /// <summary>
    /// 中心
    /// </summary>
    [StringLength(20)]
    public string? center { get; set; }

    /// <summary>
    /// 計算欄位：由 Email 自動產生 etatung\帳號
    /// </summary>
    public string account { get; set; } = null!;

    [Key]
    public long id { get; set; }
}
