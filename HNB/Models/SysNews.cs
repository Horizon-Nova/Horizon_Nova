using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 新聞表
/// </summary>
public partial class SysNews
{
    public long Id { get; set; }

    public int BaseIsDelete { get; set; }

    public DateTime BaseCreateTime { get; set; }

    public DateTime BaseModifyTime { get; set; }

    public long BaseCreatorId { get; set; }

    public long BaseModifierId { get; set; }

    public int BaseVersion { get; set; }

    /// <summary>
    /// 標題
    /// </summary>
    public string NewsTitle { get; set; } = null!;

    /// <summary>
    /// 內容
    /// </summary>
    public string NewsContent { get; set; } = null!;

    /// <summary>
    /// 標籤
    /// </summary>
    public string NewsTag { get; set; } = null!;

    /// <summary>
    /// 省份 ID
    /// </summary>
    public long ProvinceId { get; set; }

    /// <summary>
    /// 城市 ID
    /// </summary>
    public long CityId { get; set; }

    /// <summary>
    /// 區縣 ID
    /// </summary>
    public long CountyId { get; set; }

    /// <summary>
    /// 縮圖
    /// </summary>
    public string ThumbImage { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int NewsSort { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    public string NewsAuthor { get; set; } = null!;

    /// <summary>
    /// 發布時間
    /// </summary>
    public DateTime NewsDate { get; set; }

    /// <summary>
    /// 類型
    /// </summary>
    public int NewsType { get; set; }

    /// <summary>
    /// 瀏覽次數
    /// </summary>
    public int ViewTimes { get; set; }
}
