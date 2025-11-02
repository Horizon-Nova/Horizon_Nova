HNB.IIS 服務（基地台架構）

架構說明：
└── Core/                  主服務（啟動入口，基地台/代理）
    ├── Program.cs             - 服務啟動入口
    ├── Controllers/           - ErrorController（共用錯誤）、SiteManagementController（管理介面）
    ├── Views/                 - 錯誤頁面、管理介面
    ├── Services/              - ErrorLogService、IpMiddlewareServices、SiteManagementService
    ├── Repositories/          - ErrorLogRepository、BlockedIpRepository、PermissionRepository
    ├── Middleware/            - ExceptionLoggingMiddleware（日誌）、IpSecurityMiddleware（IP 封鎖）
    ├── Filters/               - RequestResponseLoggerFilter（請求日誌）、PermissionFilter（權限）
    ├── Models/                - error_log、access_record、blocked_ip、permission_management、SiteInfo
    ├── Helpers/               - LogSanitizer
    └── Utilities/             - LogErrorAttribute

Sites/ 下的站台（商品）：
└── TestWeb/               普通的 ASP.NET Core 站台
    - 不引用 Core 項目
    - 可以獨立運行
    - 未來通過 Core 反向代理運行，自動獲得保護

使用方式：
1. 啟動 Core 服務：
   cd HNB.IIS/Core
   dotnet run

2. 訪問管理介面：
   http://localhost:5227/SiteManagement

3. Sites/ 下的站台：
   - 可以獨立開發和測試
   - 發布後放到 Sites/ 文件夾
   - 未來通過 Core 運行，自動獲得保護

Core 提供功能：
- IP 封鎖與安全管理（基地台保護）
- 錯誤日誌與訪問記錄（全面監控）
- 異常處理與日誌記錄（自動捕獲）
- 權限管理（RequireMing）
- 站台管理介面（新增/刪除/監控）
- 未來：反向代理功能（讓 Sites/ 站台通過 Core 運行）
