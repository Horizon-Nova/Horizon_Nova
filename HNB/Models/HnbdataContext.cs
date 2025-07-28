using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HNB.Models;

public partial class HnbdataContext : DbContext
{
    public HnbdataContext(DbContextOptions<HnbdataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessRecord> AccessRecords { get; set; }

    public virtual DbSet<BlockedIp> BlockedIps { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<SysArea> SysAreas { get; set; }

    public virtual DbSet<SysAutoJob> SysAutoJobs { get; set; }

    public virtual DbSet<SysAutoJobLog> SysAutoJobLogs { get; set; }

    public virtual DbSet<SysDataDict> SysDataDicts { get; set; }

    public virtual DbSet<SysDataDictDetail> SysDataDictDetails { get; set; }

    public virtual DbSet<SysDepartment> SysDepartments { get; set; }

    public virtual DbSet<SysLogApi> SysLogApis { get; set; }

    public virtual DbSet<SysLogLogin> SysLogLogins { get; set; }

    public virtual DbSet<SysLogOperate> SysLogOperates { get; set; }

    public virtual DbSet<SysMenu> SysMenus { get; set; }

    public virtual DbSet<SysMenuAuthorize> SysMenuAuthorizes { get; set; }

    public virtual DbSet<SysNews> SysNews { get; set; }

    public virtual DbSet<SysPosition> SysPositions { get; set; }

    public virtual DbSet<SysRole> SysRoles { get; set; }

    public virtual DbSet<SysUser> SysUsers { get; set; }

    public virtual DbSet<SysUserBelong> SysUserBelongs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("access_records_pkey");

            entity.ToTable("access_records", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            entity.Property(e => e.HttpMethod).HasColumnName("http_method");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.LogType)
                .HasDefaultValueSql("'authorize'::text")
                .HasColumnName("log_type");
            entity.Property(e => e.RequestBody).HasColumnName("request_body");
            entity.Property(e => e.RequestPath).HasColumnName("request_path");
            entity.Property(e => e.ResponseBody).HasColumnName("response_body");
            entity.Property(e => e.Result).HasColumnName("result");
            entity.Property(e => e.Roles).HasColumnName("roles");
            entity.Property(e => e.StatusCode).HasColumnName("status_code");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserName).HasColumnName("user_name");
        });

        modelBuilder.Entity<BlockedIp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("blocked_ips_pkey");

            entity.ToTable("blocked_ips", "dbo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.Reason).HasColumnName("reason");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("error_logs_pkey");

            entity.ToTable("error_logs", "dbo");

            entity.Property(e => e.Id)
                .HasComment("主鍵")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("紀錄建立時間")
                .HasColumnName("created_at");
            entity.Property(e => e.Extra)
                .HasComment("附加資料（如 headers, query, ip, user-agent）")
                .HasColumnType("jsonb")
                .HasColumnName("extra");
            entity.Property(e => e.Function)
                .HasMaxLength(255)
                .HasComment("簡易函數描述（例如 Controller/Action）")
                .HasColumnName("function");
            entity.Property(e => e.FunctionFull)
                .HasComment("方法全名，例如 Namespace.Controller.Action()")
                .HasColumnName("function_full");
            entity.Property(e => e.HttpMethod)
                .HasMaxLength(10)
                .HasComment("HTTP 方法（GET/POST/PUT/...）")
                .HasColumnName("http_method");
            entity.Property(e => e.Layer)
                .HasMaxLength(50)
                .HasComment("層級名稱：Middleware / Filter / ExceptionHandler / Background")
                .HasColumnName("layer");
            entity.Property(e => e.Message)
                .HasComment("錯誤訊息（完整 ToString）")
                .HasColumnName("message");
            entity.Property(e => e.Path)
                .HasComment("請求路徑（Request.Path）")
                .HasColumnName("path");
            entity.Property(e => e.StackTrace)
                .HasComment("堆疊追蹤")
                .HasColumnName("stack_trace");
            entity.Property(e => e.Stage)
                .HasComment("層級代碼：0=Middleware, 1=Filter, 2=ExceptionHandler, 3=Background")
                .HasColumnName("stage");
            entity.Property(e => e.StatusCode)
                .HasComment("回應狀態碼（500/404/...）")
                .HasColumnName("status_code");
            entity.Property(e => e.TraceId)
                .HasMaxLength(100)
                .HasComment("請求唯一識別碼（HttpContext.TraceIdentifier）")
                .HasColumnName("trace_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(64)
                .HasComment("使用者 ID（HttpContext.User.Identity.Name）")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<SysArea>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysArea_pkey");

            entity.ToTable("SysArea", "dbo", tb => tb.HasComment("台灣行政區表"));

            entity.Property(e => e.Id).HasComment("主鍵");
            entity.Property(e => e.AreaCode)
                .HasMaxLength(6)
                .HasComment("地區代碼");
            entity.Property(e => e.AreaLevel).HasComment("地區層級 (1 省 2 市 3 區)");
            entity.Property(e => e.AreaName)
                .HasMaxLength(50)
                .HasComment("地區名稱");
            entity.Property(e => e.BaseCreateTime)
                .HasComment("建立時間")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseCreatorId).HasComment("建立人");
            entity.Property(e => e.BaseIsDelete).HasComment("刪除標記(0正常 1刪除)");
            entity.Property(e => e.BaseModifierId).HasComment("修改人");
            entity.Property(e => e.BaseModifyTime)
                .HasComment("修改時間")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseVersion).HasComment("資料版本(每次更新+1)");
            entity.Property(e => e.ParentAreaCode)
                .HasMaxLength(6)
                .HasComment("父地區代碼");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(50)
                .HasComment("郵遞區號");
        });

        modelBuilder.Entity<SysAutoJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysAutoJob_pkey");

            entity.ToTable("SysAutoJob", "dbo", tb => tb.HasComment("排程任務表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseModifyTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CronExpression)
                .HasMaxLength(50)
                .HasComment("排程 Cron");
            entity.Property(e => e.EndTime)
                .HasComment("結束時間")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.JobGroupName)
                .HasMaxLength(50)
                .HasComment("任務組名稱");
            entity.Property(e => e.JobName)
                .HasMaxLength(50)
                .HasComment("任務名稱");
            entity.Property(e => e.JobStatus).HasComment("任務狀態 (0 停用 1 啟用)");
            entity.Property(e => e.NextStartTime)
                .HasComment("下次執行時間")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Remark).HasComment("備註");
            entity.Property(e => e.StartTime)
                .HasComment("開始時間")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<SysAutoJobLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysAutoJobLog_pkey");

            entity.ToTable("SysAutoJobLog", "dbo", tb => tb.HasComment("排程任務日誌表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.JobGroupName)
                .HasMaxLength(50)
                .HasComment("任務組名稱");
            entity.Property(e => e.JobName)
                .HasMaxLength(50)
                .HasComment("任務名稱");
            entity.Property(e => e.LogStatus).HasComment("執行狀態 (0 失敗 1 成功)");
            entity.Property(e => e.Remark).HasComment("備註");
        });

        modelBuilder.Entity<SysDataDict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysDataDict_pkey");

            entity.ToTable("SysDataDict", "dbo", tb => tb.HasComment("資料字典類型表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseModifyTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DictSort).HasComment("排序");
            entity.Property(e => e.DictType)
                .HasMaxLength(50)
                .HasComment("字典類型");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .HasComment("備註");
        });

        modelBuilder.Entity<SysDataDictDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysDataDictDetail_pkey");

            entity.ToTable("SysDataDictDetail", "dbo", tb => tb.HasComment("字典資料表"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主鍵");
            entity.Property(e => e.BaseCreateTime)
                .HasComment("建立時間")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseCreatorId).HasComment("建立人");
            entity.Property(e => e.BaseIsDelete).HasComment("刪除標記(0正常 1刪除)");
            entity.Property(e => e.BaseModifierId).HasComment("修改人");
            entity.Property(e => e.BaseModifyTime)
                .HasComment("修改時間")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseVersion).HasComment("資料版本");
            entity.Property(e => e.DictKey).HasComment("字典鍵(一般從1開始)");
            entity.Property(e => e.DictSort).HasComment("字典排序");
            entity.Property(e => e.DictStatus).HasComment("字典狀態(0禁用 1啟用)");
            entity.Property(e => e.DictType)
                .HasMaxLength(50)
                .HasComment("字典類型(外鍵)");
            entity.Property(e => e.DictValue)
                .HasMaxLength(50)
                .HasComment("字典值");
            entity.Property(e => e.IsDefault).HasComment("預設選中(0否 1是)");
            entity.Property(e => e.ListClass)
                .HasMaxLength(50)
                .HasComment("顯示樣式(default primary success info warning danger)");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .HasComment("備註");
        });

        modelBuilder.Entity<SysDepartment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysDepartment_pkey");

            entity.ToTable("SysDepartment", "dbo", tb => tb.HasComment("部門表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseModifyTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(50)
                .HasComment("部門名稱");
            entity.Property(e => e.DepartmentSort).HasComment("排序");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasComment("電子郵件");
            entity.Property(e => e.Fax)
                .HasMaxLength(50)
                .HasComment("傳真");
            entity.Property(e => e.ParentId).HasComment("父部門 ID (0 表示根部門)");
            entity.Property(e => e.PrincipalId).HasComment("負責人 ID");
            entity.Property(e => e.Remark).HasComment("備註");
            entity.Property(e => e.Telephone)
                .HasMaxLength(50)
                .HasComment("電話");
        });

        modelBuilder.Entity<SysLogApi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysLogApi_pkey");

            entity.ToTable("SysLogApi", "dbo", tb => tb.HasComment("API 日誌表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ExecuteParam).HasComment("執行參數");
            entity.Property(e => e.ExecuteResult).HasComment("執行結果");
            entity.Property(e => e.ExecuteTime).HasComment("執行時間(ms)");
            entity.Property(e => e.ExecuteUrl)
                .HasMaxLength(100)
                .HasComment("執行 URL");
            entity.Property(e => e.LogStatus).HasComment("執行狀態 (0 失敗 1 成功)");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .HasComment("備註");
        });

        modelBuilder.Entity<SysLogLogin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysLogLogin_pkey");

            entity.ToTable("SysLogLogin", "dbo", tb => tb.HasComment("登入日誌表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Browser)
                .HasMaxLength(50)
                .HasComment("瀏覽器");
            entity.Property(e => e.ExtraRemark).HasComment("額外備註");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(20)
                .HasComment("IP 位址");
            entity.Property(e => e.IpLocation)
                .HasMaxLength(50)
                .HasComment("IP 位置");
            entity.Property(e => e.LogStatus).HasComment("執行狀態 (0 失敗 1 成功)");
            entity.Property(e => e.Os)
                .HasMaxLength(50)
                .HasComment("作業系統")
                .HasColumnName("OS");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .HasComment("備註");
        });

        modelBuilder.Entity<SysLogOperate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysLogOperate_pkey");

            entity.ToTable("SysLogOperate", "dbo", tb => tb.HasComment("操作日誌表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BusinessType)
                .HasMaxLength(50)
                .HasComment("業務類型");
            entity.Property(e => e.ExecuteParam).HasComment("執行參數");
            entity.Property(e => e.ExecuteResult).HasComment("執行結果");
            entity.Property(e => e.ExecuteTime).HasComment("執行時間(ms)");
            entity.Property(e => e.ExecuteUrl)
                .HasMaxLength(100)
                .HasComment("執行 URL");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(20)
                .HasComment("IP 位址");
            entity.Property(e => e.IpLocation)
                .HasMaxLength(50)
                .HasComment("IP 位置");
            entity.Property(e => e.LogStatus).HasComment("執行狀態 (0 失敗 1 成功)");
            entity.Property(e => e.LogType)
                .HasMaxLength(50)
                .HasComment("日誌類型");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .HasComment("備註");
        });

        modelBuilder.Entity<SysMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysMenu_pkey");

            entity.ToTable("SysMenu", "dbo", tb => tb.HasComment("選單表"));

            entity.Property(e => e.Authorize)
                .HasMaxLength(50)
                .HasComment("權限標識");
            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseModifyTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.MenuIcon)
                .HasMaxLength(50)
                .HasComment("選單圖示");
            entity.Property(e => e.MenuName)
                .HasMaxLength(50)
                .HasComment("選單名稱");
            entity.Property(e => e.MenuSort).HasComment("排序");
            entity.Property(e => e.MenuStatus).HasComment("狀態 (0 停用 1 啟用)");
            entity.Property(e => e.MenuTarget)
                .HasMaxLength(50)
                .HasComment("開啟方式");
            entity.Property(e => e.MenuType).HasComment("類型 (1 目錄 2 頁面 3 按鈕)");
            entity.Property(e => e.MenuUrl)
                .HasMaxLength(100)
                .HasComment("選單 URL");
            entity.Property(e => e.ParentId).HasComment("父選單 ID (0 表示根選單)");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .HasComment("備註");
        });

        modelBuilder.Entity<SysMenuAuthorize>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysMenuAuthorize_pkey");

            entity.ToTable("SysMenuAuthorize", "dbo", tb => tb.HasComment("選單權限表"));

            entity.Property(e => e.AuthorizeId).HasComment("授權對象 ID");
            entity.Property(e => e.AuthorizeType).HasComment("授權類型 (1 角色 2 使用者)");
            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.MenuId).HasComment("選單 ID");
        });

        modelBuilder.Entity<SysNews>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysNews_pkey");

            entity.ToTable("SysNews", "dbo", tb => tb.HasComment("新聞表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseModifyTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CityId).HasComment("城市 ID");
            entity.Property(e => e.CountyId).HasComment("區縣 ID");
            entity.Property(e => e.NewsAuthor)
                .HasMaxLength(50)
                .HasComment("作者");
            entity.Property(e => e.NewsContent).HasComment("內容");
            entity.Property(e => e.NewsDate)
                .HasComment("發布時間")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.NewsSort).HasComment("排序");
            entity.Property(e => e.NewsTag)
                .HasMaxLength(200)
                .HasComment("標籤");
            entity.Property(e => e.NewsTitle)
                .HasMaxLength(300)
                .HasComment("標題");
            entity.Property(e => e.NewsType).HasComment("類型");
            entity.Property(e => e.ProvinceId).HasComment("省份 ID");
            entity.Property(e => e.ThumbImage)
                .HasMaxLength(200)
                .HasComment("縮圖");
            entity.Property(e => e.ViewTimes).HasComment("瀏覽次數");
        });

        modelBuilder.Entity<SysPosition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysPosition_pkey");

            entity.ToTable("SysPosition", "dbo", tb => tb.HasComment("職位表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseModifyTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.PositionName)
                .HasMaxLength(50)
                .HasComment("職稱名稱");
            entity.Property(e => e.PositionSort).HasComment("排序");
            entity.Property(e => e.PositionStatus).HasComment("狀態");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .HasComment("備註");
        });

        modelBuilder.Entity<SysRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysRole_pkey");

            entity.ToTable("SysRole", "dbo", tb => tb.HasComment("角色表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseModifyTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Remark)
                .HasMaxLength(50)
                .HasComment("備註");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasComment("角色名稱");
            entity.Property(e => e.RoleSort).HasComment("排序");
            entity.Property(e => e.RoleStatus).HasComment("狀態");
        });

        modelBuilder.Entity<SysUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysUser_pkey");

            entity.ToTable("SysUser", "dbo", tb => tb.HasComment("使用者表"));

            entity.Property(e => e.ApiToken)
                .HasMaxLength(32)
                .HasComment("API Token");
            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BaseModifyTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Birthday)
                .HasMaxLength(10)
                .HasComment("生日");
            entity.Property(e => e.DepartmentId).HasComment("部門 ID");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasComment("電子郵件");
            entity.Property(e => e.FirstVisit)
                .HasComment("首次登入")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Gender).HasComment("性別");
            entity.Property(e => e.IsOnline).HasComment("是否在線");
            entity.Property(e => e.IsSystem).HasComment("系統帳號");
            entity.Property(e => e.LastVisit)
                .HasComment("最後登入")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.LoginCount).HasComment("登入次數");
            entity.Property(e => e.Mobile)
                .HasMaxLength(11)
                .HasComment("手機");
            entity.Property(e => e.Password)
                .HasMaxLength(32)
                .HasComment("密碼");
            entity.Property(e => e.Portrait)
                .HasMaxLength(200)
                .HasComment("頭像");
            entity.Property(e => e.PreviousVisit)
                .HasComment("上次登入")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Qq)
                .HasMaxLength(20)
                .HasComment("QQ")
                .HasColumnName("QQ");
            entity.Property(e => e.RealName)
                .HasMaxLength(20)
                .HasComment("真實姓名");
            entity.Property(e => e.Remark)
                .HasMaxLength(200)
                .HasComment("備註");
            entity.Property(e => e.Salt)
                .HasMaxLength(5)
                .HasComment("鹽值");
            entity.Property(e => e.UserName)
                .HasMaxLength(20)
                .HasComment("帳號");
            entity.Property(e => e.UserStatus).HasComment("狀態");
            entity.Property(e => e.WeChat)
                .HasMaxLength(20)
                .HasComment("微信");
            entity.Property(e => e.WebToken)
                .HasMaxLength(32)
                .HasComment("後台 Token");
        });

        modelBuilder.Entity<SysUserBelong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SysUserBelong_pkey");

            entity.ToTable("SysUserBelong", "dbo", tb => tb.HasComment("使用者歸屬表"));

            entity.Property(e => e.BaseCreateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.BelongId).HasComment("歸屬 ID");
            entity.Property(e => e.BelongType).HasComment("歸屬類型 (1 職位 2 角色)");
            entity.Property(e => e.UserId).HasComment("使用者 ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
