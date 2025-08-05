using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Enum.SystemManage
{
    public enum MenuTypeEnum
    {
        [Description("目錄")]
        Directory = 1,

        [Description("頁面")]
        Menu = 2,

        [Description("按鈕")]
        Button = 3
    }
}
