# 前端命名與文案規範

## 函式命名
- 動詞開頭，小駝峰：`showModal`, `applyFilters`, `submitForm`
- 事件處理：`onSearchInput`, `onStatusChange`
- 避免包裝函式，能在元素上直接呼叫 API 就直接用
- 事件綁定與元素操作一律使用 jQuery，不使用 `document.getElementById` / `addEventListener`

```javascript
// 正確
$('#saveBtn').on('click', submitForm);

// 錯誤
document.getElementById('saveBtn').addEventListener('click', submitForm);
```

## 按鈕文案
- 明確動作：`新增`、`編輯`、`刪除`、`儲存`、`關閉`
- 避免含糊：不要用 `確定` 做為通用行為
- 危險動作需次要樣式或確認步驟

## ID 命名
- 使用 kebab-case：`user-form-modal`, `role-detail-modal`, `search-input`
- Modal 集中：`{feature}-modals` 作為容器 ID

## 檔名與 Partial 命名
- 主頁：`{Feature}.cshtml`，例如 `Users.cshtml`
- Modal 部分視圖：`_{Feature}Modal.cshtml`，集中所有 Modal
- JavaScript：頁面內 `<script>` 放頁面專屬 CRUD，通用工具放獨立檔案

---

最後更新：2025-10-22
