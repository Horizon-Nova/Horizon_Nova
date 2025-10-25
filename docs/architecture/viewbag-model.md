# ViewBag 與 Model 規範

> 避免混淆（重要）
> - 本文件是「資料傳遞規範」，不是方法或類別。請不要與 `Service.ViewBagModel(viewBag, ...)` 方法混淆。
> - `ViewBagModel(...)`：是 Service 層的「資料彙整方法名稱」，負責將多個來源的資料放入 `ViewBag` 供 View/Modal 使用；它遵守本文件規範，不代表「全部都用 ViewBag」。
> - 列表/可迴圈/需分區資料 → 使用 `@model`；單一值/統計數字/當前狀態 → 使用 `ViewBag`；Modal 動態載入（特殊情況）允許 `ViewBag` 同時夾帶多種資料。
> - Modal（關鍵規則）：主頁面先包含 `@await Html.PartialAsync("_{PageName}Modal")`；需要資料時呼叫 `LoadDetail` 回傳「同一份 Partial」，前端不需要額外 container。
>
> 可攜性（跨專案適用）
> - 本文件中的範例採用中立命名（例如 `UserDto`、`OrganizationStatDto`），不依賴任何專案特定實體或資料庫結構；請在各專案中映射到實際的型別與欄位即可。
>
> 常見錯誤 vs 正確作法：
> ```cshtml
> <!-- 錯誤：主頁先放空容器，AJAX 再把 HTML 塞進去，導致 ViewBag 不一致與重複 id -->
> <div id="fileManagerModalContainer"></div>
> <button onclick="showModal('detailModal', { url: '...', data: {...}, container: 'fileManagerModalContainer' })">詳情</button>
>
> <!-- 正確：主頁先引用所有 Modal；動態時只呼叫 LoadDetail，由伺服端填好 ViewBag 並回傳同一 Partial -->
> @await Html.PartialAsync("_FileManagerModal")
> <button onclick="showModal('detailModal', { url: '@Url.Action("LoadDetail")', data: {...} })">詳情</button>
> ```

## 核心規則

### 嚴格使用規範

**ViewBag 使用時機：**
1. **單一數值/文字** - 如：當前使用人數、頁面標題
2. **唯一的動態資料** - 如：當前使用者 ID、路徑
3. **單一統計數據** - 如：總用戶數（一個數字）
4. **特殊情況** - Modal 動態載入時需要多種類型資料（組織列表、角色列表、當前實體）

**Model 使用時機：**
1. **列表資料** - 如：使用者列表、角色列表
2. **需要迴圈處理的資料** - 如：`@foreach` 的資料來源
3. **需要條件判斷的複雜資料** - 如：分區塊顯示（A區60人，B區40人）
4. **複雜結構** - 如：包含多個屬性的物件

---

## 決策流程圖

```
資料需要傳遞到 View
        ↓
    是列表嗎？
    ├─ 是 → 使用 Model
    └─ 否 ↓
    需要迴圈處理嗎？
    ├─ 是 → 使用 Model
    └─ 否 ↓
    需要條件判斷（分區塊顯示）嗎？
    ├─ 是 → 使用 Model
    └─ 否 ↓
    是單一值/統計數據嗎？
    ├─ 是 → 使用 ViewBag
    └─ 否 ↓
    是 Modal 動態載入的特殊情況嗎？
    ├─ 是 → 使用 ViewBag（包含列表）
    └─ 否 → 使用 Model
```

---

## 範例說明

### 正確範例

#### 範例 1：列表頁面
```csharp
// Controller
public IActionResult Users()
{
    var model = sev.LoadUserList(organizationId: currentUserOrganizationId);
    return View(model);
}
```

```cshtml
<!-- View -->
@model List<UserDto>

@foreach (var user in Model)
{
    <tr>...</tr>
}
```

**理由：** 列表資料需要迴圈處理，使用 `Model`。

---

#### 範例 2：單一統計數據
```csharp
// Controller
public IActionResult Dashboard()
{
    ViewBag.TotalUsers = sev.GetTotalUsers();
    return View();
}
```

```cshtml
<!-- View -->
<h3>當前使用人數：@ViewBag.TotalUsers</h3>
```

**理由：** 單一數值，直接顯示，使用 `ViewBag`。

---

#### 範例 3：分區塊顯示（需條件判斷）
```csharp
// Controller
public IActionResult Statistics()
{
    var model = sev.LoadUserStatisticsByOrganization(); // 返回 List
    return View(model);
}
```

```cshtml
<!-- View -->
@model List<OrganizationStatDto>

@foreach (var stat in Model)
{
    <div class="card">
        <h4>@stat.OrganizationName</h4>
        <p>使用人數：@stat.UserCount</p>
    </div>
}
```

**理由：** 有判斷區塊（A區60人，B區40人），使用 `Model`。

---

#### 範例 4：Modal 特殊情況
```csharp
// Controller
public IActionResult LoadDetail(int? id)
{
    ViewBag.User = sev.LoadUser(id);              // 單一實體（UserDto）
    ViewBag.Organizations = sev.LoadOrganizationList();  // 列表（下拉選單）
    ViewBag.Roles = sev.LoadRoleList();           // 列表（多選）
    return PartialView("_UsersModal");
}
```

```cshtml
<!-- Modal Partial View -->
<input type="text" value="@(ViewBag.User?.FullName ?? "")" />

<select>
    @foreach (var org in ViewBag.Organizations)
    {
        <option value="@org.Id">@org.Name</option>
    }
</select>
```

**理由：** Modal 需要多種類型的資料（單一實體 + 多個列表），且動態載入無法使用 `@model`。這是「特殊情況」，允許使用 `ViewBag` 包含列表。

---

### 錯誤範例

#### 錯誤 1：列表使用 ViewBag
```csharp
// 錯誤
public IActionResult Users()
{
    ViewBag.Users = sev.LoadUserList();  // 列表不應用 ViewBag
    return View();
}
```

```cshtml
<!-- 錯誤 -->
@foreach (var user in ViewBag.Users)
{
    <tr>...</tr>
}
```

**正確做法：** 使用 `Model`。

---

#### 錯誤 2：單一值使用 Model
```csharp
// 錯誤
public IActionResult Dashboard()
{
    var model = new { TotalUsers = sev.GetTotalUsers() };
    return View(model);
}
```

**正確做法：** 使用 `ViewBag.TotalUsers`。

---

## 特殊情況詳解

### Modal 動態載入

**為什麼 Modal 可以用 ViewBag 傳列表？**

1. **技術限制**  
   Modal 透過 AJAX 載入，Partial View 無法使用強型別 `@model`（因為需要多種資料類型）。

2. **資料需求**  
   - 當前實體（如：編輯的使用者）
   - 下拉選單資料（如：組織列表）
   - 多選資料（如：角色列表）
   - 權限資料（如：導航權限列表）

3. **架構決策**  
   在「無法使用 ViewModel」且「需要多種資料」的情況下，允許使用 `ViewBag`。這是權衡之下的最佳方案。

**替代方案（不推薦）：**
- 創建 ViewModel - **絕對禁止**
- 拆分成多個 Partial View - 增加複雜度
- 使用 JavaScript 載入資料 - 增加前端邏輯複雜度

---

## 檢查清單

在傳遞資料到 View 前，問自己：

- [ ] 這是列表嗎？ → 是 = 使用 `Model`
- [ ] 需要用 `@foreach` 嗎？ → 是 = 使用 `Model`
- [ ] 需要條件判斷（分區塊顯示）嗎？ → 是 = 使用 `Model`
- [ ] 是單一值或統計數據嗎？ → 是 = 使用 `ViewBag`
- [ ] 是 Modal 動態載入的特殊情況嗎？ → 是 = 使用 `ViewBag`（允許包含列表）

---

## 常見問題

### Q：為什麼要這麼嚴格區分 ViewBag 和 Model？

**A：** 
1. **可維護性** - 明確的規則讓代碼一致
2. **可讀性** - 看到 `Model` 就知道是列表，看到 `ViewBag` 就知道是單一值
3. **型別安全** - `Model` 有型別檢查，減少錯誤
4. **團隊協作** - 統一規範避免各自為政

### Q：極端情況（5-10 人開發團隊）怎麼辦？

**A：**  
即使在大型團隊中，也應遵守此規範。特殊情況（如 Modal）已有明確例外規則。如果遇到新的特殊情況，應：
1. 記錄在本文檔的「特殊情況」章節
2. 說明為什麼需要例外
3. 提供範例代碼

---

**相關文檔：**
- [Service 層規範](./service.md)
- [Controller 層規範](./controller.md)
- [Modal 完整使用指南](../ui/modal.md)

---

最後更新：2025-10-24

