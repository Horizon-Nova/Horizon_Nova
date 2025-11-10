# 前端命名與文案規範

## 函式命名
- 動詞開頭，小駝峰：`showModal`, `applyFilters`, `submitForm`
- 事件處理：`onSearchInput`, `onStatusChange`
- 能在元素上直接呼叫 API 就直接用，避免多餘包裝
- 選取與元素操作使用 jQuery；「點擊行為」以 HTML inline `onclick` 觸發，不使用 `addEventListener`

```html
<!-- 正確（HTML）：inline onclick 觸發 -->
<button id="saveBtn" type="button" onclick="submitForm()">儲存</button>
```

```javascript
// 錯誤
$('#saveBtn').on('click', submitForm);
document.getElementById('saveBtn').addEventListener('click', submitForm);
```

## 按鈕文案
- 明確動作：`新增`、`編輯`、`刪除`、`儲存`、`關閉`
- 避免含糊：不要用 `確定` 作為通用行為
- 危險動作需次要樣式或確認步驟

## ID 命名
- 使用 kebab-case：`user-form-modal`, `role-detail-modal`, `search-input`
- Modal 集中於 `_{Feature}Modal.cshtml` 檔內：`{feature}-modals` 可作為容器 ID

## 檔名與 Partial 命名
- 主頁：`{Feature}.cshtml`
- Modal 部分視圖：`_{Feature}Modal.cshtml`（可中央式或模組式，均置於 `Partials/`）
- JavaScript：頁面內 `<script>` 放頁面專屬邏輯；通用工具放獨立檔案

---

最後更新：2025-11-10
# 前端命名與文案規範（遷移）

（原 ui/naming.md 遷移，內容保持一致）

最後更新：2025-11-10

