using System;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using YiSha.Util;
using YiSha.Entity.SystemManage;

namespace YiSha.Entity.OrganizationManage
{
    [Table("SysNews")]
    public class NewsEntity : BaseAreaEntity
    {
        /// <summary>
        /// 文章標題
        /// </summary>
        /// <returns></returns>
        public string NewsTitle { get; set; }
        /// <summary>
        /// 文章內容
        /// </summary>
        /// <returns></returns>
        public string NewsContent { get; set; }
        /// <summary>
        /// 文章標籤
        /// </summary>
        public string NewsTag { get; set; }
        /// <summary>
        /// 縮略圖
        /// </summary>
        /// <returns></returns>
        public string ThumbImage { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        /// <returns></returns>
        public string NewsAuthor { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        /// <returns></returns>
        public int? NewsSort { get; set; }
        /// <summary>
        /// 發布時間
        /// </summary>
        /// <returns></returns>
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime? NewsDate { get; set; }
        /// <summary>
        /// 文章類別
        /// </summary>
        /// <returns></returns>
        public int? NewsType { get; set; }
        /// <summary>
        /// 閱讀量
        /// </summary>
        /// <returns></returns>
        public int? ViewTimes { get; set; }

    }
}
