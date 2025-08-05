using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiSha.Util.Extension;

namespace YiSha.Model.Param
{
    public class DateTimeParam
    {
        /// <summary>
        /// 查詢條件開始時間
        /// </summary>
        [QueryCompareAttribute(FieldName = "BaseModifyTime",Compare =CompareEnum.GreaterThanOrEquals)]
        public virtual DateTime? StartTime { get; set; }

        /// <summary>
        /// 查詢條件結束時間
        /// </summary>
        [QueryCompareAttribute(FieldName = "BaseModifyTime", Compare = CompareEnum.LessThanOrEquals)]
        public virtual DateTime? EndTime { get; set; }
    }
}
