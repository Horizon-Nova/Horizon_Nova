using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Util.Model
{
    /// <summary>
    /// 分頁參數
    /// </summary>
    public class Pagination
    {
        public Pagination()
        {
            Sort = "Id"; // 默認按Id排序
            SortType = " desc ";
            PageIndex = 1;
            PageSize = 10;
        }

        /// <summary>
        /// 每頁行數
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 當前頁
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 排序列
        /// </summary>
        public string Sort { get; set; }
        /// <summary>
        /// 排序類型
        /// </summary>
        public string SortType { get; set; }
        /// <summary>
        /// 總記錄數
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPage
        {
            get
            {
                if (TotalCount > 0)
                {
                    return TotalCount % this.PageSize == 0 ? TotalCount / this.PageSize : TotalCount / this.PageSize + 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
