# Modal 規範（原則與決策）

> 目標：以最小規則確保一致行為，支援不同規模的模組，不與特定專案耦合。

## 快速規範

- 模式（二擇一）：中央式或模組式（見決策表）
- 只用 `showModal()` / `closeModal()`；禁止以 JS 組裝 HTML
- 位置：一律放於對應頁面的 `Partials/` 目錄
- 尺寸：表單=`modal-lg`、詳情=預設、說明=`modal-xl`
- Controller 回傳 Partial，前端以 `showModal('id', { url, method, data })` 載入（不需要額外 container）
- 低階防制：觸發以 HTML inline `onclick` 直接呼叫 `showModal(...)`；Partial 內避免 `@if` 控制顯示，統一用 `?? ""` 與樣式處理占位或空值；不建立多餘暫存變數或代理函式

### 模式決策表
| 場景 | 建議模式 | 理由 |
|------|----------|------|
| 中小頁面、Modal 少 | 中央式 | 集中維護、易於掃描 |
| 大型模組、職責清晰 | 模組式 | 模組內聚、避免單檔過大 |
| 跨模組重用 | 模組式 + 共用 Partial | 提升可重用性 |

## 結構與 API

- HTML 結構：Bootstrap 標準 Modal 結構
- API：
```javascript
showModal('modalId', { url, method: 'GET', data: {} })
closeModal('modalId')
```

## 範例（中性化）

### 主頁面（擷取；點擊以 inline onclick 觸發，無容器）
```html
<!-- 觸發按鈕（inline onclick） -->
<button type="button" onclick="showModal('entityFormModal', { url: '/api/entity/form', data: { id: null } })">新增</button>

<!-- 引用 Partial（預先放在頁面底部） -->
@await Html.PartialAsync("Partials/_EntityModal")
```

### Controller（擷取）
```csharp
public IActionResult LoadDetail(int? id = null, string? type = null) {
  return PartialView("Partials/_EntityModal");
}
```

### Partial（擷取）
```html
<div id="entityFormModal" class="modal fade" tabindex="-1">
  <div class="modal-dialog modal-dialog-centered modal-lg">
    <div class="modal-content">...</div>
  </div>
</div>
```

## 反模式
- 在 JS 拼接 Modal HTML
- 將 Modal 散落於頁面根目錄（不放 Partials/）
- 範例綁定特定專案名稱、URL 或資料表

---
最後更新：2025-11-10

