HNB.IIS 服務中心

結構：
├── Core/                  共用安全服務類庫
│   ├── Models/              - error_log, access_record, blocked_ip
│   ├── Services/            - ErrorLogService, IpMiddlewareServices  
│   ├── Repositories/        - ErrorLogRepository, BlockedIpRepository
│   ├── Middleware/          - ExceptionLoggingMiddleware, IpSecurityMiddleware
│   ├── Filters/             - RequestResponseLoggerFilter
│   ├── Utilities/           - LogErrorAttribute
│   ├── Helpers/             - LogSanitizer
│   └── Extensions/          - ServiceRegistrationExtensions
│
└── Sites/                 IIS 站台發布檔案存放區
    ├── Web/                 - 發布後的 Web 站台
    ├── API/                 - 發布後的 API 站台
    └── AIWeb/               - 發布後的 AIWeb 站台

使用方式：
1. 開發新專案時引用 Core 項目
2. 發布到 Sites/ 對應資料夾
3. IIS 指向 Sites/ 對應資料夾運行

Core 提供功能：
- IP 封鎖
- 錯誤日誌
- 訪問日誌  
- 異常處理

