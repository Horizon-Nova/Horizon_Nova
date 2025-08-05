using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace YiSha.Util.Model
{
    /// <summary>
    /// 這個是移動端Api用的
    /// </summary>
    public class BaseApiToken
    {
        [NotMapped]
        [Description("WebApi沒有Cookie和Session，所以需要傳入Token來標識使用者身份，請加在Url後面")]
        public string Token { get; set; }
    }
}
