using System;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Threading.Tasks;
using YiSha.Util;
using YiSha.Web.Code;
using YiSha.IdGenerator;

namespace YiSha.Entity
{
    /// <summary>
    /// 資料庫實體的基類，所有的資料庫實體属性類型都是可空值類型，為了在做條件查询的時候進行判斷
    /// 虽然是可空值類型，null的属性值，在底層会根据属性類型赋值默認值，字符串是string.empty，數值是0，日期是1970-01-01
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// 所有表的主鍵
        /// long返回到前端js的時候，会丢失精度，所以转成字符串
        /// </summary>
        [JsonConverter(typeof(StringJsonConverter))]
        public long? Id { get; set; }

        /// <summary>
        /// WebApi沒有Cookie和Session，所以需要傳入Token來標識使用者身份
        /// </summary>
        [NotMapped]
        public string Token { get; set; }

        public virtual void Create()
        {
            this.Id = IdGeneratorHelper.Instance.GetId();
        }
    }

    public class BaseCreateEntity : BaseEntity
    {
        /// <summary>
        /// 創建時間
        /// </summary>
        [JsonConverter(typeof(DateTimeJsonConverter))]
        [Description("創建時間")]
        public DateTime? BaseCreateTime { get; set; }

        /// <summary>
        /// 創建人ID
        /// </summary>
        public long? BaseCreatorId { get; set; }

        public new async Task Create()
        {
            base.Create();

            if (this.BaseCreateTime == null)
            {
                this.BaseCreateTime = DateTime.Now;
            }

            if (this.BaseCreatorId == null)
            {
                OperatorInfo user = await Operator.Instance.Current(Token);
                if (user != null)
                {
                    this.BaseCreatorId = user.UserId;
                }
                else
                {
                    if (this.BaseCreatorId == null)
                    {
                        this.BaseCreatorId = 0;
                    }
                }
            }
        }
    }

    public class BaseModifyEntity : BaseCreateEntity
    {
        /// <summary>
        /// 資料更新版本，控制並發
        /// </summary>
        public int? BaseVersion { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [JsonConverter(typeof(DateTimeJsonConverter))]
        [Description("修改時間")]
        public DateTime? BaseModifyTime { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        public long? BaseModifierId { get; set; }

        public async Task Modify()
        {
            this.BaseVersion = 0;
            this.BaseModifyTime = DateTime.Now;

            if (this.BaseModifierId == null)
            {
                OperatorInfo user = await Operator.Instance.Current();
                if (user != null)
                {
                    this.BaseModifierId = user.UserId;
                }
                else
                {
                    if (this.BaseModifierId == null)
                    {
                        this.BaseModifierId = 0;
                    }
                }
            }
        }
    }

    public class BaseExtensionEntity : BaseModifyEntity
    {
        /// <summary>
        /// 是否删除 1是，0否
        /// </summary>
        [JsonIgnore]
        public int? BaseIsDelete { get; set; }

        public new async Task Create()
        {
            this.BaseIsDelete = 0;
         
            await base.Create();

            await base.Modify();
        }

        public new async Task Modify()
        {
            await base.Modify();
        }
    }

    public class BaseField
    {
        public static string[] BaseFieldList = new string[]
        {
            "Id",
            "BaseIsDelete",
            "BaseCreateTime",
            "BaseModifyTime",
            "BaseCreatorId",
            "BaseModifierId",
            "BaseVersion"
        };
    }
}
