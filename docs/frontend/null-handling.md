## 前端空值處理與介面一致性（Razor/Controller/JS）

目的
- 避免為了「空值」做多餘的 if 判斷與樣式切換，造成 UI 不一致與維護成本上升。
- 以最小變動原則顯示資料：空值顯示為空字串，不改變元件樣式或版面。

總原則
- 表達式優先：優先使用語言原生的空值處理（??、?.），避免 if/else 控制 UI。
- 介面一致：欠缺資料時，不以「提示樣式」取代原有元件；保持相同結構與樣式。
- 單一責任：後端負責提供資料（可為 null），前端負責防呆顯示；避免跨層判斷耦合。

Razor 檢視
- 對所有可能為 null 的字串，使用空值合併運算 `?? ""`：
  - `@(model.FieldA ?? "")`
  - `@(ViewBag.Title as string ?? "")`
- 對集合/計數：
  - 在檢視頂部將集合正規化為非 null，降低後續判斷：
    - `var rows = ViewBag.Rows as List<RowDto> ?? new();`
    - 顯示計數時不改變樣式：`@((rows.Count > 0 ? rows.Count.ToString() : null) ?? "")`
  - 若不想在頂部正規化，可用 null 傳播：
    - `@((rows?.Count?.ToString()) ?? "")`
- 布林/徽章等狀態顯示，保持固定元件，不因空值改成文字提示：
  - 好：`<span class="badge @(isActive ? "bg-success" : "bg-danger")">@(isActive ? "啟用" : "停用")</span>`
  - 不建議：空值改顯示為「無資料」文字或不同樣式的段落
- 避免用條件片段切換整塊畫面（例如 `@if (!list.Any())` 換成提示版），應保留原有骨架：
  - 表格表頭仍顯示；無資料時欄位值以 `""` 呈現，不額外插入提示區塊。

Controller（ASP.NET Core MVC）
- 不為 UI 進行條件判斷（例如「參數缺漏就改回傳提示畫面」）；改為資料正規化：
  - 參數可用 `?? string.Empty` 正規化：
    - `var currentTable = tableName ?? string.Empty;`
  - 回傳集合用 `?? new List<>()` 避免前端 null：
    - `ViewBag.TableColumns = result.Columns ?? new List<TableColumnDto>();`
- 移除不必要的 `[HttpGet]` 標註（遵循專案約定）。

JavaScript / AJAX
- 不冗餘判斷 `response.success`；直接以 `showToast` 統一回饋（成功/失敗訊息由後端或預設字串提供）。
- 文字處理以 `?? ""` 或預設字串處理，而非切換 DOM 結構。

範例：資料表詳情（節錄）
```cshtml
@{
    var tableColumns = ViewBag.TableColumns as List<TableColumnDto> ?? new();
}

<!-- 統計卡：僅顯示數字或空字串，不切樣式 -->
<h5 class="mb-0">@((tableColumns.Count > 0 ? tableColumns.Count.ToString() : null) ?? "")</h5>

<!-- 表格：表頭恆定，欄位值用 ?? "" -->
<td><strong>@(column.ColumnName ?? "")</strong></td>
<td><span class="badge bg-primary">@(column.DataType ?? "")</span></td>
<td>@(column.Length ?? "")</td>
<td>
  <span class="badge @(column.IsNullable ? "bg-success" : "bg-danger")">
    @(column.IsNullable ? "允許" : "必填")
  </span>
</td>
<td>@(column.DefaultValue ?? "")</td>
```

Do / Don’t
- Do：`@(value ?? "")`、`@((list?.Count?.ToString()) ?? "")`、保留既有元件骨架
- Don’t：為空值切換整段 DOM 或改用提示卡片、在前端/後端新增不必要 if

備註
- 若遇到特殊情境需顯示系統層級錯誤，仍以統一的 `showToast` 呈現，不在畫面結構中插入例外樣式。

