# DataTable 與 HNBDataTable 規範

## 最小初始化（推薦）
```javascript
const table = HNBDataTable.init('#myTable', {
  order: [[0, 'desc']],
  columnDefs: [{ orderable: false, targets: [5] }]
});
```

## 必要元素
```html
<table id="myTable" class="table table-hover mb-0">
  <thead>...</thead>
  <tbody>...</tbody>
</table>
```
- 不需要額外包 `div` 容器；長度、搜尋、分頁、資訊會自動插入。

## 常見設定
- 隱藏內建搜尋與長度：在 `initComplete` 內 `$('.dataTables_filter,.dataTables_length').hide();`
- 自訂排序：`order: [[n, 'desc']]`
- 停用排序欄：`columnDefs: [{ orderable: false, targets: [idx...] }]`

## 樣式
- 使用 `hnb-datatable.css` 與 Bootstrap 5 一致的 muted 風格（`#6c757d`）
- 分頁按鈕無外框與背景，採文字樣式

## FAQ
- 分頁看起來在表格外面？→ 不需額外容器；若要卡片外觀，由外層頁面自行加卡片
- 為何沒有搜尋輸入框？→ 已在頁面使用自訂搜尋，內建已隱藏

---

最後更新：2025-10-22
