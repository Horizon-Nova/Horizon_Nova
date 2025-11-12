# Modal 規範（原則與決策）

> 目標：以最小規則確保一致行為，支援不同規模的模組，不與特定專案耦合。

## 快速規範

- 模式（二擇一）：中央式或模組式（見決策表）
- 只用 `showModal()` / `closeModal()`；禁止以 JS 組裝 HTML
- 位置：一律放於對應頁面的 `Partials/` 目錄
- 尺寸：詳情=預設、說明=`modal-xl`；表單尺寸見 `frontend/forms.md`
- Controller 回傳 Partial，前端以 `showModal('id', { url, method, data })` 載入（不需要額外 container）
- 事件綁定：預設由按鈕 `onclick` 觸發；需要動態內容時，再使用 jQuery 委派事件
- Partial 內避免 `@if` 切換整塊結構；以 `?? ""` 與樣式處理占位或空值；不建立多餘暫存變數或代理函式

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

### 主頁面（擷取；按鈕 onclick 觸發）
```html
<button type="button" onclick="openEntityForm()">新增</button>

<!-- 引用 Partial（預先放在頁面底部或專屬 Scripts 區塊） -->
@await Html.PartialAsync("Partials/_EntityModal")
```

```javascript
function openEntityForm(){
  showModal('entityFormModal', { url: '/api/entity/form', method: 'GET', data: { id: null } });
}
```

### 動態內容（例外：委派事件觸發）
```javascript
$(document)
  .off('click.entity', '.js-open-entity-form')
  .on('click.entity', '.js-open-entity-form', function () {
    showModal('entityFormModal', { url: '/api/entity/form', method: 'GET', data: { id: null } });
  });
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

