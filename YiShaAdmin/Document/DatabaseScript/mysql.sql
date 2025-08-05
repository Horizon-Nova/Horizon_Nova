/* 執行脚本前，請先選擇資料庫，脚本会先删除表，然後再創建表，請谨慎執行！！！ */

/* use YiShaAdmin; */

DROP TABLE IF EXISTS `SysArea`;
CREATE TABLE IF NOT EXISTS `SysArea` (
  `Id`                   bigint(20)     NOT NULL     COMMENT '主鍵',
  `BaseIsDelete`       int(11)        NOT NULL     COMMENT '删除標記(0正常 1删除)',
  `BaseCreateTime`     datetime       NOT NULL     COMMENT '創建時間',
  `BaseModifyTime`     datetime       NOT NULL     COMMENT '修改時間',
  `BaseCreatorId`      bigint(20)     NOT NULL     COMMENT '創建人',
  `BaseModifierId`     bigint(20)	    NOT NULL     COMMENT '修改人',
  `BaseVersion`         int(11)        NOT NULL     COMMENT '資料版本(每次更新+1)',
  `AreaCode`            varchar(6)     NOT NULL     COMMENT '地區編碼',
  `ParentAreaCode`     varchar(6)     NOT NULL     COMMENT '父地區編碼',
  `AreaName`            varchar(50)    NOT NULL     COMMENT '地區名稱',
  `ZipCode`             varchar(50)    NOT NULL     COMMENT '邮政編碼',
  `AreaLevel`           int(11)        NOT NULL     COMMENT '地區層級(1省份 2城市 3區县)',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '中国省市县表';
 
DROP TABLE IF EXISTS `SysAutoJob`;
CREATE TABLE IF NOT EXISTS `SysAutoJob` (
  `Id`                  bigint(20)		NOT NULL,
  `BaseIsDelete`		int(11)			NOT NULL,
  `BaseCreateTime`	datetime		NOT NULL,
  `BaseModifyTime`	datetime		NOT NULL,
  `BaseCreatorId`		bigint(20)		NOT NULL,
  `BaseModifierId`	bigint(20)		NOT NULL,
  `BaseVersion`		int(11)			NOT NULL,
  `JobGroupName`		varchar(50)		NOT NULL       COMMENT '任務組名稱',
  `JobName`			varchar(50)		NOT NULL       COMMENT '任務名稱',
  `JobStatus`			int(11)			NOT NULL       COMMENT '任務狀態(0禁用 1啟用)',
  `CronExpression`		varchar(50)		NOT NULL       COMMENT 'cron表達式',
  `StartTime`			datetime		NOT NULL       COMMENT '運行開始時間',
  `EndTime`			datetime		NOT NULL       COMMENT '運行結束時間',
  `NextStartTime`		datetime		NOT NULL       COMMENT '下次執行時間',
  `Remark`				text			NOT NULL       COMMENT '備注',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '定時任務表';

DROP TABLE IF EXISTS `SysDataDict`;
CREATE TABLE IF NOT EXISTS `SysDataDict` (
  `Id`                  bigint(20)      NOT NULL,
  `BaseIsDelete`      int(11)         NOT NULL,     
  `BaseCreateTime`    datetime        NOT NULL,
  `BaseModifyTime`    datetime        NOT NULL,
  `BaseCreatorId`     bigint(20)      NOT NULL,
  `BaseModifierId`    bigint(20)      NOT NULL,
  `BaseVersion`        int(11)         NOT NULL,
  `DictType`           varchar(50)     NOT NULL       COMMENT '字典類型',
  `DictSort`           int(11)         NOT NULL       COMMENT '字典排序',
  `Remark`              varchar(50)     NOT NULL       COMMENT '備注',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '字典類型表';

DROP TABLE IF EXISTS `SysDataDictDetail`;
CREATE TABLE IF NOT EXISTS `SysDataDictDetail` (
  `Id`                  bigint(20)      NOT NULL,
  `BaseIsDelete`      int(11)         NOT NULL,
  `BaseCreateTime`    datetime        NOT NULL,
  `BaseModifyTime`    datetime        NOT NULL,
  `BaseCreatorId`     bigint(20)      NOT NULL,
  `BaseModifierId`    bigint(20)      NOT NULL,
  `BaseVersion`        int(11)         NOT NULL,
  `DictType`           varchar(50)     NOT NULL       COMMENT '字典類型(外鍵)',
  `DictSort`           int(11)         NOT NULL       COMMENT '字典排序',
  `DictKey`            int(11)         NOT NULL       COMMENT '字典鍵(一般從1開始)',
  `DictValue`          varchar(50)     NOT NULL       COMMENT '字典值',
  `ListClass`          varchar(50)     NOT NULL       COMMENT '顯示樣式(default primary success info warning danger)',
  `DictStatus`         int(11)         NOT NULL       COMMENT '字典狀態(0禁用 1啟用)',
  `IsDefault`          int(11)         NOT NULL       COMMENT '默認選中(0不是 1是)',
  `Remark`              varchar(50)     NOT NULL       COMMENT '備注',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '字典資料表';

DROP TABLE IF EXISTS `SysDepartment`;
CREATE TABLE IF NOT EXISTS `SysDepartment` (
  `Id`                      bigint(20)      NOT NULL,       
  `BaseIsDelete`          int(11)         NOT NULL,      
  `BaseCreateTime`        datetime        NOT NULL,       
  `BaseModifyTime`        datetime        NOT NULL,       
  `BaseCreatorId`         bigint(20)      NOT NULL,       
  `BaseModifierId`        bigint(20)      NOT NULL,
  `BaseVersion`            int(11)         NOT NULL,
  `ParentId`               bigint(20)      NOT NULL       COMMENT '父部門Id(0表示是根部門)',
  `DepartmentName`         varchar(50)     NOT NULL       COMMENT '部門名稱',
  `Telephone`               varchar(50)     NOT NULL       COMMENT '部門電話',
  `Fax`                     varchar(50)     NOT NULL       COMMENT '部門傳真',
  `Email`                   varchar(50)     NOT NULL       COMMENT '部門Email',
  `PrincipalId`            bigint(20)      NOT NULL       COMMENT '部門負責人Id',
  `DepartmentSort`         int(11)         NOT NULL       COMMENT '部門排序',
  `Remark`                  text            NOT NULL       COMMENT '備注',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '部門表';

DROP TABLE IF EXISTS `SysMenu`;
CREATE TABLE IF NOT EXISTS `SysMenu` (
  `Id`                      bigint(20)      NOT NULL,
  `BaseIsDelete`          int(11)         NOT NULL,
  `BaseCreateTime`        datetime        NOT NULL,
  `BaseModifyTime`        datetime        NOT NULL,
  `BaseCreatorId`         bigint(20)      NOT NULL,
  `BaseModifierId`        bigint(20)      NOT NULL,
  `BaseVersion`            int(11)         NOT NULL,
  `ParentId`               bigint(20)      NOT NULL       COMMENT '父選單Id(0表示是根選單)',
  `MenuName`               varchar(50)     NOT NULL       COMMENT '選單名稱',
  `MenuIcon`               varchar(50)     NOT NULL       COMMENT '選單圖標',  
  `MenuUrl`                varchar(100)    NOT NULL       COMMENT '選單Url',
  `MenuTarget`             varchar(50)     NOT NULL       COMMENT '連結打開方式',
  `MenuSort`               int(11)         NOT NULL       COMMENT '選單排序',
  `MenuType`               int(11)         NOT NULL       COMMENT '選單類型(1目錄 2頁面 3按鈕)',
  `MenuStatus`             int(11)         NOT NULL       COMMENT '選單狀態(0禁用 1啟用)',     
  `Authorize`               varchar(50)     NOT NULL       COMMENT '選單權限標識',
  `Remark`                  varchar(50)     NOT NULL       COMMENT '備注',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '選單表';

DROP TABLE IF EXISTS `SysMenuAuthorize`;
CREATE TABLE IF NOT EXISTS `SysMenuAuthorize` (
  `Id`                      bigint(20)      NOT NULL,
  `BaseCreateTime`        datetime        NOT NULL,
  `BaseCreatorId`         bigint(20)      NOT NULL,
  `MenuId`                 bigint(20)      NOT NULL       COMMENT '選單Id',
  `AuthorizeId`            bigint(20)      NOT NULL       COMMENT '授權Id(角色Id或者使用者Id)',
  `AuthorizeType`          int(11)         NOT NULL       COMMENT '授權類型(1角色 2使用者)',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '選單權限表';

DROP TABLE IF EXISTS `SysNews`;
CREATE TABLE IF NOT EXISTS `SysNews` (
  `Id`                  bigint(20)      NOT NULL,
  `BaseIsDelete`      int(11)         NOT NULL,
  `BaseCreateTime`    datetime        NOT NULL,
  `BaseModifyTime`    datetime        NOT NULL,
  `BaseCreatorId`     bigint(20)      NOT NULL,
  `BaseModifierId`    bigint(20)      NOT NULL,
  `BaseVersion`        int(11)         NOT NULL,
  `NewsTitle`          varchar(300)    NOT NULL       COMMENT '新闻標題',
  `NewsContent`        longtext        NOT NULL       COMMENT '新闻內容',
  `NewsTag`            varchar(200)    NOT NULL       COMMENT '新闻標籤',
  `ProvinceId`         bigint(20)      NOT NULL       COMMENT '省份Id',
  `CityId`             bigint(20)      NOT NULL       COMMENT '城市Id',
  `CountyId`           bigint(20)      NOT NULL       COMMENT '區县Id',
  `ThumbImage`         varchar(200)    NOT NULL       COMMENT '縮略圖',
  `NewsSort`           int(11)         NOT NULL       COMMENT '新闻排序',
  `NewsAuthor`         varchar(50)     NOT NULL       COMMENT '發布者',
  `NewsDate`           datetime        NOT NULL       COMMENT '發布時間',
  `NewsType`           int(11)         NOT NULL       COMMENT '新闻類型(1產品案例 2行業新闻)',
  `ViewTimes`          int(11)         NOT NULL       COMMENT '查看次數',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '新闻表';

DROP TABLE IF EXISTS `SysPosition`;
CREATE TABLE IF NOT EXISTS `SysPosition` (
  `Id`                  bigint(20)      NOT NULL,
  `BaseIsDelete`      int(11)         NOT NULL,
  `BaseCreateTime`    datetime        NOT NULL,
  `BaseModifyTime`    datetime        NOT NULL,
  `BaseCreatorId`     bigint(20)      NOT NULL,
  `BaseModifierId`    bigint(20)      NOT NULL,
  `BaseVersion`        int(11)         NOT NULL,
  `PositionName`       varchar(50)     NOT NULL       COMMENT '職位名稱',
  `PositionSort`       int(11)         NOT NULL       COMMENT '職位排序',
  `PositionStatus`     int(11)         NOT NULL       COMMENT '職位狀態(0禁用 1啟用)',
  `Remark`              varchar(50)     NOT NULL       COMMENT '備注',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '職位表';

DROP TABLE IF EXISTS `SysRole`;
CREATE TABLE IF NOT EXISTS `SysRole` (
  `Id`                  bigint(20)      NOT NULL,
  `BaseIsDelete`      int(11)         NOT NULL,
  `BaseCreateTime`    datetime        NOT NULL,
  `BaseModifyTime`    datetime        NOT NULL,
  `BaseCreatorId`     bigint(20)      NOT NULL,
  `BaseModifierId`    bigint(20)      NOT NULL,
  `BaseVersion`        int(11)         NOT NULL,
  `RoleName`           varchar(50)     NOT NULL       COMMENT '角色名稱',
  `RoleSort`           int(11)         NOT NULL       COMMENT '角色排序',
  `RoleStatus`         int(11)         NOT NULL       COMMENT '角色狀態(0禁用 1啟用)',
  `Remark`              varchar(50)     NOT NULL       COMMENT '備注',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '角色表';

DROP TABLE IF EXISTS `SysUser`;
CREATE TABLE IF NOT EXISTS `SysUser` (
  `Id`                  bigint(20)          NOT NULL,
  `BaseIsDelete`      int(11)             NOT NULL,
  `BaseCreateTime`    datetime            NOT NULL,
  `BaseModifyTime`    datetime            NOT NULL,
  `BaseCreatorId`     bigint(20)          NOT NULL,
  `BaseModifierId`    bigint(20)          NOT NULL,
  `BaseVersion`        int(11)             NOT NULL,
  `UserName`           varchar(20)         NOT NULL       COMMENT '使用者名',
  `Password`            varchar(32)         NOT NULL       COMMENT '密碼',
  `Salt`                varchar(5)          NOT NULL       COMMENT '密碼盐值',
  `RealName`           varchar(20)         NOT NULL       COMMENT '姓名',
  `DepartmentId`       bigint(20)          NOT NULL       COMMENT '所属部門Id',
  `Gender`              int(11)             NOT NULL       COMMENT '性別(0未知 1男 2女)',
  `Birthday`            varchar(10)         NOT NULL       COMMENT '出生日期',
  `Portrait`            varchar(200)        NOT NULL       COMMENT '頭像',
  `Email`               varchar(50)         NOT NULL       COMMENT 'Email',
  `Mobile`              varchar(11)         NOT NULL       COMMENT '手機',
  `QQ`                  varchar(20)         NOT NULL       COMMENT 'QQ',
  `WeChat`              varchar(20)         NOT NULL       COMMENT '微信',
  `LoginCount`         int(11)             NOT NULL       COMMENT '登錄次數',
  `UserStatus`         int(11)             NOT NULL       COMMENT '使用者狀態(0禁用 1啟用)',
  `IsSystem`           int(11)             NOT NULL       COMMENT '系统使用者(0不是 1是[系统使用者拥有所有的權限])',
  `IsOnline`           int(11)             NOT NULL       COMMENT '在線(0不是 1是)',
  `FirstVisit`         datetime            NOT NULL       COMMENT '首次登錄時間',
  `PreviousVisit`      datetime            NOT NULL       COMMENT '上一次登錄時間',
  `LastVisit`          datetime            NOT NULL       COMMENT '最後一次登錄時間',
  `Remark`              varchar(200)        NOT NULL       COMMENT '備注',
  `WebToken`           varchar(32)         NOT NULL       COMMENT '後台Token',
  `ApiToken`           varchar(32)         NOT NULL       COMMENT 'ApiToken',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '使用者表';

DROP TABLE IF EXISTS `SysUserBelong`;
CREATE TABLE IF NOT EXISTS `SysUserBelong` (
  `Id`                  bigint(20)      NOT NULL,
  `BaseCreateTime`    datetime        NOT NULL,
  `BaseCreatorId`     bigint(20)      NOT NULL,
  `UserId`             bigint(20)      NOT NULL       COMMENT '使用者Id',
  `BelongId`           bigint(20)      NOT NULL       COMMENT '職位Id或者角色Id',
  `BelongType`         int(11)         NOT NULL       COMMENT '所属類型(1職位 2角色)',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '使用者所属表';

DROP TABLE IF EXISTS `SysAutoJobLog`;
CREATE TABLE IF NOT EXISTS `SysAutoJobLog` (
  `Id`                  bigint(20)      NOT NULL,
  `BaseCreateTime`    datetime        NOT NULL,
  `BaseCreatorId`     bigint(20)      NOT NULL,
  `JobGroupName`      varchar(50)     NOT NULL       COMMENT '任務組名稱',
  `JobName`            varchar(50)     NOT NULL       COMMENT '任務名稱',
  `LogStatus`          int(11)         NOT NULL       COMMENT '執行狀態(0失敗 1成功)',
  `Remark`              text            NOT NULL       COMMENT '備注',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '使用者所属表';

DROP TABLE IF EXISTS `SysLogApi`;
CREATE TABLE IF NOT EXISTS `SysLogApi` (
  `Id`                  bigint(20)      NOT NULL,
  `BaseCreateTime`    datetime        NOT NULL,
  `BaseCreatorId`     bigint(20)      NOT NULL,
  `LogStatus`          int(11)         NOT NULL       COMMENT '執行狀態(0失敗 1成功)',
  `Remark`              varchar(50)     NOT NULL       COMMENT '備注',
  `ExecuteUrl`         varchar(100)    NOT NULL       COMMENT '接口地址',
  `ExecuteParam`       text            NOT NULL       COMMENT '請求參數',
  `ExecuteResult`      text            NOT NULL       COMMENT '請求結果',
  `ExecuteTime`        int(11)         NOT NULL       COMMENT '執行時間',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT 'Api日誌表';

DROP TABLE IF EXISTS `SysLogLogin`;
CREATE TABLE IF NOT EXISTS `SysLogLogin` (
  `Id`                  bigint(20)      NOT NULL,
  `BaseCreateTime`    datetime        NOT NULL,
  `BaseCreatorId`     bigint(20)      NOT NULL,
  `LogStatus`          int(11)         NOT NULL       COMMENT '執行狀態(0失敗 1成功)',
  `IpAddress`          varchar(20)     NOT NULL       COMMENT 'ip地址',
  `IpLocation`         varchar(50)     NOT NULL       COMMENT 'ip位置',
  `Browser`             varchar(50)     NOT NULL       COMMENT '瀏覽器',
  `OS`                  varchar(50)     NOT NULL       COMMENT '操作系统',
  `Remark`              varchar(50)     NOT NULL       COMMENT '備注',
  `ExtraRemark`        text            NOT NULL       COMMENT '額外備注',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '登錄日誌表';

DROP TABLE IF EXISTS `SysLogOperate`;
CREATE TABLE IF NOT EXISTS `SysLogOperate` (
  `Id`                  bigint(20)      NOT NULL,
  `BaseCreateTime`    datetime        NOT NULL,
  `BaseCreatorId`     bigint(20)      NOT NULL,
  `LogStatus`          int(11)         NOT NULL       COMMENT '執行狀態(0失敗 1成功)',
  `IpAddress`          varchar(20)     NOT NULL       COMMENT 'ip地址',
  `IpLocation`         varchar(50)     NOT NULL       COMMENT 'ip位置',
  `Remark`              varchar(50)     NOT NULL       COMMENT '備注',
  `LogType`            varchar(50)     NOT NULL       COMMENT '日誌類型(暂未用到)',
  `BusinessType`       varchar(50)     NOT NULL       COMMENT '業務類型(暂未用到)',
  `ExecuteUrl`         varchar(100)    NOT NULL       COMMENT '頁面地址',
  `ExecuteParam`       text            NOT NULL       COMMENT '請求參數',
  `ExecuteResult`      text            NOT NULL       COMMENT '請求結果',
  `ExecuteTime`        int(11)         NOT NULL       COMMENT '執行時間',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB COMMENT '操作日誌表';
