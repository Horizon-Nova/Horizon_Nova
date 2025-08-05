using System;
using System.Collections.Generic;
using System.Text;

namespace YiSha.Model.Param
{
    public class ImportParam
    {
        /// <summary>
        /// 導入文件上傳服務器後的路徑
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 是否更新已有的資料
        /// </summary>
        public int? IsOverride { get; set; }
    }
}
