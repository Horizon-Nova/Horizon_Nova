using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Enum
{
    public enum StatusEnum
    {
        [Description("啟用")]
        Yes = 1,

        [Description("禁用")]
        No = 0
    }

    public enum IsEnum
    {
        [Description("是")]
        Yes = 1,

        [Description("否")]
        No = 0
    }

    public enum NeedEnum
    {
        [Description("不需要")]
        NotNeed = 0,

        [Description("需要")]
        Need = 1
    }

    public enum OperateStatusEnum
    {
        [Description("失敗")]
        Fail = 0,

        [Description("成功")]
        Success = 1
    }

    public enum UploadFileType
    {
        [Description("頭像")]
        Portrait = 1,

        [Description("新闻圖片")]
        News = 2,

        [Description("導入的文件")]
        Import = 10
    }

    public enum PlatformEnum
    {
        [Description("Web後台")]
        Web = 1,

        [Description("WebApi")]
        WebApi = 2
    }

    public enum PayStatusEnum
    {
        [Description("未知")]
        Unknown = 0,

        [Description("已支付")]
        Success = 1,

        [Description("转入退款")]
        Refund = 2,

        [Description("未支付")]
        NotPay = 3,

        [Description("已關閉")]
        Closed = 4,

        [Description("已撤銷（付款碼支付）")]
        Revoked = 5,

        [Description("使用者支付中（付款碼支付）")]
        UserPaying = 6,

        [Description("支付失敗(其他原因，如银行返回失敗)")]
        PayError = 7
    }

}
