# Horizon Nova 開發文檔導覽

## 文檔結構

本專案的開發文檔為**通用規範**，適用於所有專案開發，而非特定模組的使用說明。

### 全域規範（強制）
- 語言：統一使用繁體中文（zh-TW）
- 禁止使用 Emoji（文件、UI、程式碼註解、提交訊息）
- 前端選取與事件：一律使用 jQuery API，禁止 `document.getElementById` / `addEventListener`

---

## 快速開始

### 新人入職必讀
1. [ViewBag vs Model 決策](./architecture/viewbag-model.md) - 數據傳遞規範
2. [Repository 規範](./architecture/repository.md)
3. [Service 規範](./architecture/service.md)
4. [Controller 規範](./architecture/controller.md)

### 後端開發規範
| 文檔 | 說明 |
|------|------|
| [ViewBag vs Model 決策](./architecture/viewbag-model.md) | 數據傳遞規範與決策流程 |
| [Repository 層規範](./architecture/repository.md) | 數據訪問層規範 |
| [Service 層規範](./architecture/service.md) | 業務邏輯層規範 |
| [Controller 層規範](./architecture/controller.md) | 控制器層規範 |

### 前端開發規範
| 文檔 | 說明 |
|------|------|
| [Bootstrap 使用規範](./ui/bootstrap.md) | Bootstrap 5 使用規則 |
| [Modal 完整指南](./ui/modal.md) | 彈出視窗開發規範 |
| [AJAX 規範](./ui/ajax.md) | AJAX 請求規範 |
| [表單設計規範](./ui/forms.md) | 表單與驗證 |
| [命名與文案規範](./ui/naming.md) | 前端命名、按鈕文案 |
| [DataTable 規範](./ui/datatable.md) | HNBDataTable 使用 |

### FAQ 與故障排除
| 文檔 | 說明 |
|------|------|
| [開發常見問題](./faq/development-faq.md) | 通用開發問題與解決方案 |
| [Modal 常見問題](./ui/modal.md#常見問題) | Modal 開發疑難排解 |

---

## 快速查找

### 我想要...
- **了解資料傳遞** → [ViewBag vs Model](./architecture/viewbag-model.md)
- **查詢 Repository 規範** → [Repository](./architecture/repository.md)
- **查詢 Service/Controller** → [Service](./architecture/service.md)、[Controller](./architecture/controller.md)
- **判斷用 ViewBag 或 Model** → [ViewBag vs Model 決策](./architecture/viewbag-model.md)
- **開發 Modal** → [Modal 完整指南](./ui/modal.md)
- **設計表單** → [Bootstrap 使用規範](./ui/bootstrap.md)

### 我遇到問題...
- **Modal 只能開一次** → [Modal FAQ](./ui/modal.md#常見問題)
- **不知道用 ViewBag 還是 Model** → [ViewBag vs Model](./architecture/viewbag-model.md#決策流程圖)
- **AJAX 請求失敗** → [AJAX 規範](./ui/ajax.md)

---

## 文檔維護指南

### 何時更新文檔？
- 新增架構規範時 → 更新 `ARCHITECTURE.md` 或對應的 `docs/architecture/` 文件
- 修改 UI 規範時 → 更新 `docs/ui/` 對應文件
- 遇到新的通用開發問題時 → 在 `docs/faq/development-faq.md` 新增 QA

### 文檔撰寫原則
1. 標題清晰 - 使用明確的標題和子標題
2. 範例豐富 - 提供實際代碼範例
3. 包含 FAQ - 記錄常見開發問題和解決方案
4. 通用性 - 規範應適用於所有專案，不針對特定模組

---

## 文檔版本

| 版本 | 日期 | 說明 |
|------|------|------|
| 2.0 | 2025-10-22 | 文檔結構重組：分層架構、通用規範、移除 Emoji |
| 1.0 | 2025-10-01 | 初始版本：單一 ARCHITECTURE.md |

---

最後更新：2025-10-22
