namespace YiSha.Util.Model
{
    public class SystemConfig
    {
        public SystemConfig()
        {
            DBSlowSqlLogTime = 5;
        }

        /// <summary>
        /// 是否是Demo模式
        /// </summary>
        public bool Demo { get; set; }

        /// <summary>
        /// 是否是調试模式
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// 允許一個使用者在多個電脑同時登入
        /// </summary>
        public bool LoginMultiple { get; set; }

        public string LoginProvider { get; set; }

        /// <summary>
        /// Snow Flake Worker Id
        /// </summary>
        public int SnowFlakeWorkerId { get; set; }

        /// <summary>
        /// api地址
        /// </summary>
        public string ApiSite { get; set; }

        /// <summary>
        /// 允許跨域的站點
        /// </summary>
        public string AllowCorsSite { get; set; }

        /// <summary>
        /// 网站虛擬目錄
        /// </summary>
        public string VirtualDirectory { get; set; }

        public string DBProvider { get; set; }

        public string DBConnectionString { get; set; }

        /// <summary>
        ///  資料庫超時間（秒）
        /// </summary>
        public int DBCommandTimeout { get; set; }

        /// <summary>
        /// 慢查询記錄Sql(秒),保存到文件以便分析
        /// </summary>
        public int DBSlowSqlLogTime { get; set; }

        /// <summary>
        /// 資料庫備份路徑
        /// </summary>
        public string DBBackup { get; set; }

        public string CacheProvider { get; set; }

        public string RedisConnectionString { get; set; }
    }
}