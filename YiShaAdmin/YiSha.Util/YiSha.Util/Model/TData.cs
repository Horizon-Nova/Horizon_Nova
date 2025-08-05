using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Util.Model
{
    /// <summary>
    /// 資料傳輸對象
    /// </summary>
    public class TData
    {
        /// <summary>
        /// 操作結果，Tag為1代表成功，0代表失敗，其他的驗證返回結果，可根据需要設置
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// 提示信息或異常信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 扩展Message
        /// </summary>
        public string Description { get; set; }
    }

    public class TData<T> : TData
    {
        /// <summary>
        /// 列表的記錄數
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 資料
        /// </summary>
        public T Data { get; set; }
    }
}
