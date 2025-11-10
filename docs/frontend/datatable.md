# DataTable（Bootstrap 5 版）規範

## 必要資源（Bootstrap 5 + DataTables + Buttons）
請確保版型載入以下資源（順序很重要）：

```html
<!-- jQuery -->
<script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>

<!-- Bootstrap 5 -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">

<!-- DataTables with Bootstrap 5 theme -->
<link rel="stylesheet" href="https://cdn.datatables.net/1.13.7/css/dataTables.bootstrap5.min.css">
<script src="https://cdn.datatables.net/1.13.7/js/jquery.dataTables.min.js"></script>
<script src="https://cdn.datatables.net/1.13.7/js/dataTables.bootstrap5.min.js"></script>

<!-- Buttons extension（匯出）-->
<link rel="stylesheet" href="https://cdn.datatables.net/buttons/2.4.2/css/buttons.bootstrap5.min.css">
<script src="https://cdn.datatables.net/buttons/2.4.2/js/dataTables.buttons.min.js"></script>
<script src="https://cdn.datatables.net/buttons/2.4.2/js/buttons.bootstrap5.min.js"></script>
<script src="https://cdn.datatables.net/buttons/2.4.2/js/buttons.html5.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/jszip/3.10.1/jszip.min.js"></script>
```

> 圖示可選：如需在按鈕上顯示圖示，可自行加入 Lucide 或其他圖示庫。

---

## HTML 結構（不額外包容器）
```html
<table id="myTable" class="table table-hover table-sm align-middle mb-0">
  <thead>
    <tr>
      <th>欄一</th>
      <th>欄二</th>
      <th>操作</th>
    </tr>
  </thead>
  <tbody>...</tbody>
</table>
```

- 不需要額外包裝 `div` 容器；長度、搜尋、分頁、資訊區塊會由外掛自動插入在表格相鄰位置。
- 建議 class：`table table-hover table-sm align-middle mb-0`（清爽、間距合理）。

---

## 初始化（Bootstrap 化 DOM 配置 + 匯出按鈕）
```javascript
const table = $('#myTable').DataTable({
  // Bootstrap 風格版位：上方工具列（長度/搜尋/按鈕）、表格本體、下方資訊/分頁
  dom:
    "<'row align-items-center g-2'<'col-auto'l><'col ms-auto'f><'col-auto'B>>" +
    "<'row'<'col-12'tr>>" +
    "<'row align-items-center g-2'<'col'i><'col ms-auto'p>>",

  buttons: [
    {
      extend: 'excelHtml5',
      text: '匯出 Excel',
      className: 'btn btn-sm btn-outline-primary'
    }
  ],

  order: [[0, 'desc']],
  columnDefs: [{ orderable: false, targets: [2] }]
});
```

說明：
- `dom` 使用 Bootstrap Row/Col 與工具列的對齊方式，版面更清晰。
- `B` 代表 Buttons 區，若按鈕未顯示，多半是 Buttons/JSZip 未載入或 dom 未包含 `B`。
- 匯出按鈕以文字為主；如需圖示可自行加入 `<i>` 或使用圖示庫。

---

## 常見客製（選用）
- 隱藏內建搜尋與長度（若使用自訂搜尋/長度）：
  ```javascript
  initComplete: function () {
    $('.dataTables_filter,.dataTables_length').hide();
  }
  ```
- 自訂排序：`order: [[n, 'desc']]`
- 停用排序欄：`columnDefs: [{ orderable: false, targets: [idx...] }]`

---

## 樣式建議
- 與 Bootstrap 5 一致的 muted 風格（`text-muted`, `small`）作為輔助資訊樣式。
- 分頁建議以文字樣式為主；避免過度強調按鈕外框/實心配色，保持清爽。
- 若要卡片外觀，請由外層頁面加上卡片樣式，不需要為 DataTable 另外包容器。

---

## FAQ
1) 匯出按鈕沒有顯示？  
   - 確認已載入 Buttons CSS/JS、`buttons.html5.js`、`JSZip`。
   - 確認 `dom` 內含 `B`，且 `buttons` 選項已設定。

2) 佈局錯位或工具列擠在一起？  
   - 檢查 `dom` 是否使用了 Bootstrap Row/Col 結構（見範例）。
   - 確保未額外包裹多層容器導致版位計算錯誤。

3) 搜尋框或長度選擇要自訂位置？  
   - 使用自訂工具列（建議透過 `dom` 配置）；或隱藏內建元素後自行在頁面上方提供控制元件。

---

最後更新：2025-11-10