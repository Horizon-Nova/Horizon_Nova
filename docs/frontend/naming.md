# 前端命名與文案規範

## 函式命名
- 動詞開頭，小駝峰：`showModal`, `applyFilters`, `submitForm`
- 事件處理命名：`onSearchInput`, `onStatusChange`（處理器命名）
- 點擊行為預設使用按鈕 `onclick` 直接呼叫函式；需要動態載入才使用委派事件

```javascript
// 正確：委派與命名空間
function submitForm(){ /* ... */ }
$(document)
  .off('click.form', '.js-save')
  .on('click.form',  '.js-save', submitForm);
```

```html
<!-- 預設：按鈕 onclick 觸發 -->
<button type="button" class="btn btn-primary" onclick="submitForm()">儲存</button>
```

## 按鈕文案
- 明確動作：`新增`、`編輯`、`刪除`、`儲存`、`關閉`
- 避免含糊：不要用 `確定` 作為通用行為
- 危險動作需次要樣式或確認步驟

## ID 命名
- 使用 kebab-case：`user-form-modal`, `role-detail-modal`, `search-input`
- Modal 應放於 `Partials/{Feature}/Modal/_Xxx.cshtml`；容器 ID 以 `{feature}-modals` 或具語意命名

## 檔名與 Partial 命名
- 主頁：`{Feature}/Index.cshtml`（主畫面一律使用 `Index`）
- Partial 規則：`Partials/{Feature}/_Component.cshtml`
- Modal 規則：`Partials/{Feature}/Modal/_Name.cshtml`
- Scripts Partial：`Partials/{Feature}/_Scripts.cshtml`（頁面專屬腳本集中於此）

---

最後更新：2025-11-10
# 前端命名與文案規範（遷移）

（原 ui/naming.md 遷移，內容保持一致）

最後更新：2025-11-10

