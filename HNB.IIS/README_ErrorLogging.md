# 錯誤捕捉系統使用指南

## 📋 概述

HNB.IIS Core 專案現在已經實現完整的**專案層級錯誤捕捉系統**，能夠在最外層（Middleware 層）捕捉所有未處理的異常，並自動記錄到資料庫中。

## 🎯 特點

### ✅ 完整捕捉
- **Middleware 層級捕捉**：在最外層捕捉所有未處理的異常
- **自動記錄**：異常自動記錄到資料庫和日誌系統
- **詳細資訊**：包含堆疊追蹤、HTTP 資訊、使用者資訊等

### 🎨 開發體驗
- **開發模式**：顯示詳細錯誤頁面，包含完整堆疊追蹤
- **生產模式**：重定向到標準錯誤頁面，保護敏感資訊
- **美觀的錯誤頁面**：現代化的 UI 設計

### 📊 管理介面
- **錯誤日誌列表**：查看所有捕捉到的錯誤
- **統計資訊**：今日錯誤、各層級錯誤數量等
- **詳細檢視**：點擊查看完整錯誤資訊

## 📁 檔案結構

```
HNB.IIS/Core/
├── Middleware/
│   └── ExceptionLoggingMiddleware.cs      # 全局異常捕捉中間件
├── Services/
│   └── ErrorLogService.cs                  # 錯誤日誌服務
├── Repositories/
│   └── ErrorLogRepository.cs               # 錯誤日誌資料存取
├── Utilities/
│   └── ErrorUtility.cs                     # 錯誤處理工具類
├── Controllers/
│   └── ErrorLogController.cs               # 錯誤日誌管理控制器
└── Views/
    └── ErrorLog/
        └── Index.cshtml                    # 錯誤日誌列表頁面
```

## 🚀 使用方法

### 1. 查看錯誤日誌

訪問錯誤日誌管理頁面：
```
http://localhost:5000/ErrorLog
```

或從側邊欄導航選擇「錯誤日誌」

### 2. 測試錯誤捕捉

系統提供多種測試端點：

#### 基本測試
```
GET /ErrorLog/TestException
```
拋出一個基本的 `InvalidOperationException`

#### 不同類型的錯誤
```
GET /ErrorLog/TestErrorType?type=null          # ArgumentNullException
GET /ErrorLog/TestErrorType?type=notfound      # KeyNotFoundException
GET /ErrorLog/TestErrorType?type=unauthorized  # UnauthorizedAccessException
GET /ErrorLog/TestErrorType?type=argument      # ArgumentException
GET /ErrorLog/TestErrorType?type=divide        # DivideByZeroException
GET /ErrorLog/TestErrorType?type=outofrange    # IndexOutOfRangeException
GET /ErrorLog/TestErrorType?type=nested        # 巢狀異常
```

#### 正常測試（應該不會產生錯誤日誌）
```
GET /ErrorLog/TestNormal
```

### 3. 在自己的程式碼中使用

錯誤捕捉是自動的，不需要任何額外的程式碼。任何未處理的異常都會被自動捕捉並記錄。

#### 手動記錄錯誤（可選）

如果你想在 catch 區塊中手動記錄錯誤：

```csharp
public class MyController : Controller
{
    private readonly ErrorLogService _errorLogService;

    public MyController(ErrorLogService errorLogService)
    {
        _errorLogService = errorLogService;
    }

    public async Task<IActionResult> MyAction()
    {
        try
        {
            // 你的程式碼
        }
        catch (Exception ex)
        {
            // 手動記錄錯誤（stage = 1 表示 Filter 層級）
            await _errorLogService.LogExceptionAsync(ex, HttpContext, stage: 1, "MyController/MyAction");
            
            // 回傳錯誤回應
            return StatusCode(500, "Internal server error");
        }
    }
}
```

## 🔧 設定說明

### Program.cs 設定

```csharp
// 註冊服務
builder.Services.AddScoped<ErrorLogRepository>();
builder.Services.AddScoped<ErrorLogService>();

// 註冊中間件（必須在最前面）
app.UseMiddleware<ExceptionLoggingMiddleware>();
```

### 層級說明

錯誤日誌系統使用 `stage` 欄位來區分錯誤捕捉的層級：

- **0 - Middleware**: 在 Middleware 層級捕捉的錯誤（最外層）
- **1 - Filter**: 在 Filter 層級捕捉的錯誤
- **2 - ExceptionHandler**: 在 Exception Handler 中捕捉的錯誤
- **3 - Background**: 在背景服務中捕捉的錯誤

## 📊 資料庫結構

錯誤日誌儲存在 `dbo.error_logs` 資料表中：

| 欄位 | 類型 | 說明 |
|------|------|------|
| id | int | 主鍵 |
| stage | short | 層級代碼 (0-3) |
| layer | string | 層級名稱 |
| function | string | 簡易函數描述 |
| function_full | string | 完整函數名稱 |
| message | text | 錯誤訊息 |
| stack_trace | text | 堆疊追蹤 |
| path | string | 請求路徑 |
| http_method | string | HTTP 方法 |
| status_code | int | 回應狀態碼 |
| user_id | string | 使用者 ID |
| trace_id | string | 追蹤識別碼 |
| extra | jsonb | 額外資料（JSON 格式） |
| created_at | timestamp | 建立時間 |

## 🛡️ 安全性

### 敏感資訊保護

系統自動過濾以下敏感標頭：
- Authorization
- Cookie
- X-API-Key
- X-Auth-Token

### 日誌清理

所有記錄的資料都會經過 `LogSanitizer.Clean()` 處理，移除：
- Null 字元 (`\0`)
- 控制字元

### 環境區分

- **開發環境**: 顯示詳細錯誤資訊
- **生產環境**: 重定向到標準錯誤頁面

## 📈 最佳實踐

### 1. 監控錯誤趨勢
定期查看錯誤日誌管理頁面，關注：
- 今日錯誤數量
- 各層級錯誤分布
- 最近一小時的錯誤

### 2. 處理常見錯誤
對於頻繁出現的錯誤：
1. 查看詳細堆疊追蹤
2. 定位問題程式碼
3. 修復並測試
4. 監控是否再次出現

### 3. 效能考量
- 錯誤日誌寫入是非同步的，不會阻塞主要請求
- 如果記錄失敗，錯誤會寫入日誌系統
- 考慮定期清理舊的錯誤日誌

## 🔍 工具類別

### ErrorUtility

提供多種實用的錯誤處理方法：

```csharp
// 取得完整異常訊息（包含內部異常）
string fullMessage = ErrorUtility.GetFullExceptionMessage(exception);

// 取得簡化的錯誤訊息
string simpleMessage = ErrorUtility.GetSimplifiedErrorMessage(exception);

// 判斷是否為致命錯誤
bool isFatal = ErrorUtility.IsFatalException(exception);

// 取得異常嚴重程度
string severity = ErrorUtility.GetExceptionSeverity(exception);

// 建立錯誤摘要
string summary = ErrorUtility.CreateErrorSummary(exception, httpContext);
```

## 🎯 測試檢查清單

完成以下測試以確保錯誤捕捉系統正常運作：

- [ ] 訪問 `/ErrorLog` 頁面，確認可以正常顯示
- [ ] 點擊「基本測試」按鈕，確認會產生錯誤日誌
- [ ] 重新整理 `/ErrorLog` 頁面，確認新的錯誤已記錄
- [ ] 點擊「詳情」按鈕，確認可以查看完整錯誤資訊
- [ ] 測試不同類型的錯誤，確認都能正確捕捉
- [ ] 點擊「正常測試」按鈕，確認不會產生錯誤日誌
- [ ] 在開發環境中測試，確認顯示詳細錯誤頁面
- [ ] 查看資料庫中的 `error_logs` 資料表，確認資料正確儲存

## 📞 疑難排解

### 錯誤沒有被記錄
1. 確認 `ExceptionLoggingMiddleware` 已註冊且在最前面
2. 確認資料庫連線正常
3. 檢查應用程式日誌中是否有錯誤訊息

### 無法訪問錯誤日誌頁面
1. 確認路由設定正確
2. 確認有適當的權限
3. 檢查 `PermissionFilter` 設定

### 錯誤頁面顯示異常
1. 確認 Bootstrap 和 JavaScript 函式庫已正確載入
2. 檢查瀏覽器控制台是否有 JavaScript 錯誤

## 🎉 總結

現在你的 HNB.IIS Core 專案擁有完整的專案層級錯誤捕捉系統！

- ✅ 所有未處理的異常都會被自動捕捉
- ✅ 錯誤會自動記錄到資料庫
- ✅ 開發環境顯示詳細錯誤資訊
- ✅ 生產環境保護敏感資訊
- ✅ 提供完整的管理介面

享受穩定可靠的錯誤監控體驗！🚀

