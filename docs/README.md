# Horizon Nova 開發文檔導覽

## 核心規則（重點）
- 規則優先順序：Core Rules（本段） > architecture > frontend > 頁面內明確說明 > 程式碼不可違反約束 > 其它
- 資料傳遞：列表/需迴圈/需分區 → `@model`；單一值/統計數 → `ViewBag`；Modal 特殊情況允許 `ViewBag` 同時帶多種資料
- Modal 模式（二選一，依規模與複用度決策）：
  - 中央式：所有 Modal 集中 `Partials/_{PageName}Modal.cshtml`
  - 模組式：依功能拆分 `Partials/_UsersModal.cshtml`、`_RolesModal.cshtml`…（推薦於大型模組）
  - 不得以 JS 組裝 HTML；只用 `showModal`/`closeModal`；Controller 回傳 Partial
- 嚴禁在 JS 中組裝 HTML（包含彈窗、警告、表單、卡片）。統一：由 Razor 產出 HTML，JS 僅觸發與綁定事件。
- 低階程式防制（強制）：Razor 避免 `@if` 控制顯示，統一使用就地運算與 `?? ""`；點擊行為預設由按鈕 `onclick` 直接呼叫函式（需要動態時再用委派）；AJAX 採最小必要參數與一致回應處理，不增加多餘防呆與暫存變數。

## 詞彙表（簡）
- Modal：Bootstrap 模態視窗（靜態/動態）
- Partial View：可重用視圖片段；Modal 可採中央式或模組式，置於 `Partials/`
- ViewBag：動態資料容器，適用單一值與 Modal 特殊情況
- `@model`：強型別資料來源，適用列表/迴圈/條件分區
- Virtual Path：以 `/` 開頭的路徑（如 `/`, `/Folder/Sub`）

## 文檔結構與責任邊界
本文件（README）僅提供「導覽、核心規則與入口索引」。詳細規範與範例請見對應子文檔：
- 架構規範：`docs/architecture/*`
- 前端規範：`docs/frontend/*`
- FAQ 與故障排除：`docs/faq/*`
文檔角色分工：
- README：導覽與核心規則，不放長篇章節與重複內容
- 子文檔：規則本體、範例與 FAQ

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

### 後端開發規範（索引）
| 文檔 | 說明 |
|------|------|
| [ViewBag vs Model 決策](./architecture/viewbag-model.md) | 數據傳遞規範與決策流程 |
| [Repository 層規範](./architecture/repository.md) | 數據訪問層規範 |
| [Service 層規範](./architecture/service.md) | 業務邏輯層規範 |
| [Controller 層規範](./architecture/controller.md) | 控制器層規範 |

### 前端開發規範（索引）
| 文檔 | 說明 |
|------|------|
| [Bootstrap 使用規範](./frontend/bootstrap.md) | Bootstrap 5 使用規則 |
| [Modal 原則與決策](./frontend/modal.md) | 中央式/模組式決策與規則 |
| [AJAX 規範（含表單提交）](./frontend/ajax.md#表單與提交合併指南) | AJAX 與表單提交規範 |
| [表單設計規範（佈局建議）](./frontend/forms.md) | 表單 UI/佈局建議（提交規範已整合至 AJAX） |
| [DataTable 規範](./frontend/datatable.md) | HNBDataTable 使用 |

### 故障診斷
| 文檔 | 說明 |
|------|------|
| [故障診斷（Diagnostics）](./troubleshooting/development-diagnostics.md) | 通用診斷流程與常見問題 |
| [Modal 常見問題](./frontend/modal.md#常見問題) | Modal 開發疑難排解 |

---

## 開發工作流程（必守）
1. 先讀規範（architecture/ui）→ 明確資料傳遞方式
2. 設計流程自上而下：頁面 → Controller → Service → Repository
3. Modal：先掛 Partial；中央式或模組式擇一；用 `LoadDetail` 回同一 Partial
4. 表單：HTML5 驗證，AJAX 提交，禁止 JS 組裝 HTML
5. 任何錯誤：依 FAQ 分診流程檢查，不做單點猜測

## PR 檢查清單（一致性）
- [ ] 符合資料傳遞規範（列表=`@model`，單值=`ViewBag`，Modal特例）
- [ ] 沒有在 JS 組裝 HTML（彈窗/表單/卡片）
- [ ] Controller 命名與責任（`LoadDetail`、`Submit*`、單一 `Delete`）
- [ ] Service/Repository 命名與責任（`Load*`/`Query*`），無業務混入 Repository
- [ ] Modal 架構模式選用一致（中央式或模組式），動態由 Controller 回傳對應 Partial
- [ ] 表單驗證使用 HTML5；AJAX 以標準範本實作
- [ ] 具備最小重現與日誌/請求片段（如修 bug）

---

## 文檔版本

| 版本 | 日期 | 說明 |
|------|------|------|
| 2.4 | 2025-10-24 | 新增：開發流程與 PR 檢查清單；FAQ 增加分診與匯入檢查 |
| 2.3 | 2025-10-24 | 表單提交規範整合至 AJAX；`forms.md` 保留 UI 建議 |
| 2.2 | 2025-10-24 | 新增：嚴禁以 JS 組裝 HTML 的規則 |
| 2.1 | 2025-10-24 | 精簡：核心規則與詞彙移入 README，刪除冗餘文件 |
| 2.0 | 2025-10-22 | 文檔結構重組：分層架構、通用規範、移除 Emoji |
| 1.0 | 2025-10-01 | 初始版本：單一 ARCHITECTURE.md |

---

最後更新：2025-10-24
