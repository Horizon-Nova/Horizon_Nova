using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNB.Areas.HNB_WEB.Enum;

public enum StatusEnum
{
    [Description("啟用")]
    Yes = 1,

    [Description("停用")]
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
    [Description("大頭照")]
    Portrait = 1,

    [Description("新聞圖片")]
    News = 2,

    [Description("匯入的檔案")]
    Import = 10
}

public enum PlatformEnum
{
    [Description("Web 後台")]
    Web = 1,

    [Description("Web API")]
    WebApi = 2
}

public enum PayStatusEnum
{
    [Description("未知")]
    Unknown = 0,

    [Description("已付款")]
    Success = 1,

    [Description("已退款")]
    Refund = 2,

    [Description("未付款")]
    NotPay = 3,

    [Description("已關閉")]
    Closed = 4,

    [Description("已撤銷（付款碼支付）")]
    Revoked = 5,

    [Description("用戶付款中（付款碼支付）")]
    UserPaying = 6,

    [Description("付款失敗（其他原因，如銀行拒絕）")]
    PayError = 7
}
