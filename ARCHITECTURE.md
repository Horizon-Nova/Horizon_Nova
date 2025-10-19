# Horizon Nova 架構規範文件

## 目錄
- [概述](#概述)
- [Repository 層規範](#repository-層規範)
- [Service 層規範](#service-層規範)
- [ViewBag 與 Model 規範](#viewbag-與-model-規範)
- [Controller 層規範](#controller-層規範)
- [命名速查表](#命名速查表)
- [程式碼規範](#程式碼規範)
- [語言與文案規範](#語言與文案規範)
- [前端彈出視窗（Modal）規範](#前端彈出視窗modal規範)
- [前端 AJAX 規範](#前端-ajax-規範)
- [範例](#範例)

---

## 概述

本專案採用三層架構：**Controller → Service → Repository → Database**

### 核心原則
1. **一張表只會出現一個 Insert*** - 避免重複的 CRUD 方法
2. **使用幾個表就要做多少 Query*List、Query*** - 確保完整的查詢覆蓋
3. **不使用 Async** - 保持同步操作
4. **不使用 try...catch** - 使用 `=>` 表達式和 `??` 運算子
5. **Repository 只做簡單查詢** - 業務邏輯放在 Service 層
6. **不可使用 Get 命名** - 查詢或呼叫資料庫資料一律使用 `Query*` / `Load*` 命名

---

## Repository 層規範

### 結構範本

```csharp
public class ExampleRepository(DbContext db)
{
    #region 統一的查詢來源
    
    private IQueryable<Table1> ValidTable1s => db.table1s.Where(...).OrderBy(...);
    private IQueryable<Table2> ValidTable2s => db.table2s.Where(...);
    
    #endregion

    #region 專用查詢方法
    
    // 表1 查詢
    public List<Table1> QueryTable1List(參數...) => ValidTable1s.Where(...).ToList();
    public Table1? QueryTable1(int? id = null, string? name = null) { ... }
    
    // 表2 查詢
    public List<Table2> QueryTable2List(參數...) => ValidTable2s.Where(...).ToList();
    public Table2? QueryTable2(int? id = null) => ValidTable2s.FirstOrDefault(...);
    
    #endregion

    #region 基本 CRUD 操作
    
    // 一張表只有一個 Insert 方法
    public Table InsertTable(Table data)
    {
        var existingEntity = db.tables.Find(data.id);
        
        if (existingEntity == null)
        {
            // 新增邏輯
            data.created_at = DateTime.Now;
            db.tables.Add(data);
            db.SaveChanges();
            return data;
        }
        
        // 更新邏輯
        existingEntity.field1 = data.field1;
        // ... 更新所有欄位
        
        // 條件性更新（如密碼）
        if (!string.IsNullOrEmpty(data.password_hash))
        {
            existingEntity.password_hash = data.password_hash;
        }
        
        existingEntity.updated_at = DateTime.Now;
        db.SaveChanges();
        return existingEntity;
    }
    
    public bool DeleteTable(int id)
    {
        var entity = db.tables.Find(id);
        if (entity != null)
        {
            db.tables.Remove(entity);
            db.SaveChanges();
            return true;
        }
        return false;
    }
    
    #endregion
}
```

### 命名規則

| 方法類型 | 命名格式 | 說明 | 範例 |
|---------|---------|------|------|
| 列表查詢 | `Query*表名*List` | 返回列表 | `QueryUserList()` |
| 單一查詢 | `Query*表名*` | 返回單個物件 | `QueryUser(int? id = null)` |
| 插入/更新 | `Insert*表名*` | 新增或更新 | `InsertUser(user)` |
| 刪除 | `Delete*表名*` | 刪除記錄 | `DeleteUser(int id)` |

### 重要規則

####  正確範例

```csharp
// 使用 2 張表的 Repository
public class AuthRepository(DbContext db)
{
    // 表1: vw_permission_user
    public List<vw_permission_user> QueryUserList(...) { }
    public vw_permission_user? QueryUser(...) { }
    
    // 表2: permission_management
    public List<permission_management> QueryPermissionUserList(...) { }
    public permission_management? QueryPermissionUser(...) { }
    
    // 只有 1 個 Insert 方法（針對 permission_management 表）
    public permission_management InsertPermissionUser(permission_management user) { }
}
```

####  錯誤範例

```csharp
//  一張表出现多個 Insert
public permission_management InsertUser(permission_management user) { }
public permission_management InsertRole(permission_management role) { }
public permission_management InsertOrganization(permission_management org) { }

//  使用了 Async
public async Task<User?> QueryUserAsync(int id) { }

//  使用了 try...catch
public User CreateUser(User user)
{
    try
    {
        db.users.Add(user);
        db.SaveChanges();
        return user;
    }
    catch (Exception ex)
    {
        return null;
    }
}

//  缺少對应的 Query 方法
// 使用了 permission_management 表，但没有 QueryPermissionManagementList 和 QueryPermissionManagement
```

---

## Service 層規範

### 结構範本

```csharp
public class ExampleService(ExampleRepository repo)
{
    #region 統一的查詢方法
    
    // 對应 Repository 的 Query 方法
    public List<Table1> LoadTable1List(參數...) => repo.QueryTable1List(參數...);
    public Table1? LoadTable1(int? id = null) => repo.QueryTable1(id);
    
    public List<Table2> LoadTable2List(參數...) => repo.QueryTable2List(參數...);
    public Table2? LoadTable2(int id) => repo.QueryTable2(id);
    
    #endregion

    #region ViewBag 設定方法
    
    public void ViewBagModel(dynamic viewBag, int? id = null)
    {
        viewBag.Id = id;
        viewBag.Table1s = LoadTable1List();
        viewBag.Table1 = id.HasValue ? LoadTable1(id.Value) : null;
        // ...
    }
    
    #endregion

    #region 基本 CRUD 操作
    
    public Table CreateTable(Table data) => repo.InsertTable(data);
    public bool DeleteTable(int id) => repo.DeleteTable(id);
    
    #endregion

    #region 輔助方法
    
    // 复杂的業務邏輯
    public (bool success, string message) ProcessBusinessLogic(...)
    {
        var data = LoadTable1(id);
        if (data == null)
            return (false, "數据不存在");
        
        // 業務邏輯處理
        // ...
        
        return (true, "處理成功");
    }
    
    #endregion
}
```

### 命名規則

| 方法類型 | 命名格式 | 說明 | 範例 |
|---------|---------|------|------|
| 列表載入 | `Load*表名*List` | 呼叫 Query*List | `LoadUserList()` |
| 單一載入 | `Load*表名*` | 呼叫 Query* | `LoadUser(int id)` |
| 創建/更新 | `Create*表名*` | 呼叫 Insert* | `CreateUser(user)` |
| 刪除 | `Delete*表名*` | 呼叫 Delete* | `DeleteUser(int id)` |
| ViewBag | `ViewBagModel` | 設定 ViewBag | `ViewBagModel(viewBag, id)` |

---

## ViewBag 與 Model 規範

### 使用準則（強制）
- 頁面與部分視圖的資料以強型別 Model 為主；不得以 ViewBag 傳遞資料集合或複雜物件。
- 僅在需要傳遞唯一值時使用 ViewBag（或 `ViewBagModel`）：如 `id`、`name`、`currentPath` 等單一字串/數值。
- `ViewBagModel` 的責任：僅設定上述「唯一值/頁面上下文字串」，不放入任何查詢結果或列表。

### Controller 範式
```csharp
// 主頁：Model 承載資料；ViewBag 僅放置唯一值
public IActionResult Page(string? id)
{
    ViewBag.Id = id; // 唯一值允許
    var model = sev.LoadXxxList(...); // 強型別集合
    return View(model);
}

// 局部詳情：以唯一鍵（如 id 或 path+name）載入單筆 Model
[HttpGet]
public IActionResult LoadDetail(int id)
{
    var item = sev.LoadXxx(id);
    if (item == null) return Content("<div class=\"p-6\">找不到項目</div>", "text/html; charset=utf-8");
    return PartialView("_XxxDetail", item);
}
```

### FileManager 作法（推薦）
- 主頁：Controller 載入 `List<vw_file_manager>` 作為 View 的 Model，同時以 `ViewBag.CurrentPath` 傳遞目前路徑（唯一值）。
- 詳情：`LoadDetail(path, name)` 以 `(path, name)` 作唯一鍵，由 Service 載入單筆 `vw_file_manager`，回傳部分視圖。

---

## Controller 層規範

### 控制器原則

#### 1. **查詢方法**
- **`LoadDetail`** - 用於導入 Models 資料（模態框/局部視圖）
  ```csharp
  [HttpGet]
  public IActionResult LoadDetail(int id)
  {
      sev.ViewBagModel(ViewBag, id);
      return PartialView("_DetailModal");
  }
  ```

#### 2. **ViewBag 使用方式**
- 僅在頁面或部分視圖需要「唯一值（如 id、name、currentPath）」時使用 `sev.ViewBagModel(ViewBag, ...)` 或直接設定 ViewBag。
- 嚴禁以 ViewBag 傳遞列表或複雜物件，資料須以強型別 Model 提供。
  ```csharp
  public IActionResult PageName()
  {
      ViewBag.Id = id;  // ← 僅設定唯一值
      var model = sev.LoadXxxList(...);  // ← 資料用 Model 傳遞
      return View(model);
  }
  ```

#### 3. **Submit 方法（統一提交）**
- **命名格式**：`Submit*頁面名称*`
- **用途**：統一處理新增和編輯，由 Repository 根據 `id` 判斷
- **範例**：
  ```csharp
  [HttpPost]
  public IActionResult SubmitNavigation(sidebar_navigation form)
  {
      var result = sev.CreateNavigation(form);
      return Json(new { 
          success = result != null, 
          message = result != null ? "儲存成功" : "儲存失敗" 
      });
  }
  ```

#### 4. **Delete 方法（只能有一個）**
- **原則**：每個 Controller 只能有一個 `Delete` 函數
- **範例**：
  ```csharp
  [HttpPost]
  public IActionResult Delete(int id)
  {
      var result = sev.DeleteModel(id);
      return Json(new { 
          success = result, 
          message = result ? "刪除成功" : "刪除失敗" 
      });
  }
  ```

### 完整範例

```csharp
public class NavigationController(NavigationService sev) : BaseController
{
    // 1. 主頁面 - 設定初始 ViewBag
    public IActionResult NavigationManagement()
    {
        sev.ViewBagModel(ViewBag);
        return View();
    }

    // 2. 查詢詳情 - 用於模態框
    [HttpGet]
    public IActionResult LoadDetail(int id)
    {
        sev.ViewBagModel(ViewBag, id);
        return PartialView("_NavigationModal");
    }

    // 3. 統一提交 - 新增或編輯
    [HttpPost]
    public IActionResult SubmitNavigation(sidebar_navigation form)
    {
        var result = sev.CreateNavigation(form);
        return Json(new { success = result != null, message = result != null ? "儲存成功" : "儲存失敗" });
    }

    // 4. 刪除
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var result = sev.DeleteNavigation(id);
        return Json(new { success = result, message = result ? "刪除成功" : "刪除失敗" });
    }
}
```

---

## 命名速查表

| 層級 | 列表查詢 | 單一查詢 | 新增/更新 | 刪除 | 其他 |
|------|----------|----------|-----------|------|------|
| Repository | `Query*表名*List` | `Query*表名*` | `Insert*表名*` | `Delete*表名*` | — |
| Service | `Load*表名*List` | `Load*表名*` | `Create*表名*` | `Delete*表名*` | `ViewBagModel(viewBag, id)` |

- 禁用：`Get*` 命名、`async/await`、`try...catch`（集中於中介軟體處理例外）。
- 詳細範例已在「Repository 層規範 / Service 層規範」提供，不再於此重複。

---

## 程式碼規範

### 1. 使用表達式 (`=>`)

 **正確**
```csharp
public List<User> QueryUserList(bool? isActive = null)
    => ValidUsers.Where(u => !isActive.HasValue || u.is_active == isActive.Value).ToList();

public User? QueryUser(int id)
    => ValidUsers.FirstOrDefault(u => u.id == id);

public User CreateUser(User user)
    => repo.InsertUser(user);
```

 **錯誤**
```csharp
public List<User> QueryUserList(bool? isActive = null)
{
    return ValidUsers.Where(u => !isActive.HasValue || u.is_active == isActive.Value).ToList();
}
```

### 2. 不使用 Get 命名

 **正確 - Repository 使用 Query***
```csharp
public List<User> QueryUserList() { }
public User? QueryUser(int id) { }
public List<string> QueryOccupiedRoleIdList(int orgId) { }
public Dictionary<string, string> QueryOccupiedRoles(List<string> roleIds) { }
```

 **正確 - Service 使用 Load***
```csharp
public List<User> LoadUserList() { }
public User? LoadUser(int id) { }
```

 **錯誤 - 使用 Get 命名**
```csharp
public User? GetUser(int id) { }
public List<User> GetUsers() { }
public List<string> GetOccupiedRoleIds(int orgId) { }
```

### 3. 不使用 Async

 **正確**
```csharp
public User? QueryUser(int id)
    => db.users.FirstOrDefault(u => u.id == id);
```

 **錯誤**
```csharp
public async Task<User?> QueryUserAsync(int id)
    => await db.users.FirstOrDefaultAsync(u => u.id == id);
```

### 4. 不使用 try...catch

 **正確**
```csharp
// Repository
public User CreateUser(User user)
    => repo.InsertUser(user);

// Controller
public IActionResult Create(User user)
{
    var result = service.CreateUser(user);
    return Json(new { 
        success = result != null, 
        message = result != null ? "創建成功" : "創建失败" 
    });
}
```

 **錯誤**
```csharp
public User CreateUser(User user)
{
    try
    {
        return repo.InsertUser(user);
    }
    catch (Exception ex)
    {
        return null;
    }
}
```

### 5. 可选參數合并查詢

 **正確 - 单個方法使用可选參數**
```csharp
public User? QueryUser(int? userId = null, string? username = null)
{
    if (userId.HasValue)
        return ValidUsers.FirstOrDefault(u => u.id == userId.Value);
    
    if (!string.IsNullOrEmpty(username))
        return ValidUsers.FirstOrDefault(u => u.name == username);
    
    return null;
}

// 呼叫
var user1 = QueryUser(userId: 123);
var user2 = QueryUser(username: "admin");
```

 **錯誤 - 多個重载方法**
```csharp
public User? QueryUser(int userId) { ... }
public User? QueryUser(string username) { ... }
```

### 6. Insert 方法處理條件性更新

 **正確 - 在 Insert 方法内判斷**
```csharp
public permission_management InsertPermissionUser(permission_management user)
{
    var existingEntity = db.permission_managements.Find(user.id);
    
    if (existingEntity == null)
    {
        // 新增
        user.created_at = DateTime.Now;
        db.permission_managements.Add(user);
        db.SaveChanges();
        return user;
    }
    
    // 更新
    existingEntity.name = user.name;
    existingEntity.email = user.email;
    // ... 其他欄位
    
    // 判斷密碼是否為空，有就指定
    if (!string.IsNullOrEmpty(user.password_hash) && !string.IsNullOrEmpty(user.salt))
    {
        existingEntity.password_hash = user.password_hash;
        existingEntity.salt = user.salt;
        existingEntity.last_password_change_at = DateTime.Now;
    }
    
    // 判斷是否更新登录信息
    if (user.last_login_at.HasValue)
    {
        existingEntity.last_login_at = user.last_login_at;
        existingEntity.last_login_ip = user.last_login_ip;
        existingEntity.login_count = user.login_count;
    }
    
    existingEntity.updated_at = DateTime.Now;
    db.SaveChanges();
    return existingEntity;
}
```

 **錯誤 - 多個专门的更新方法**
```csharp
public User InsertUser(User user) { ... }
public User UpdatePassword(int userId, string password) { ... }
public User UpdateLoginInfo(int userId, string ip) { ... }
```

---

## 語言與文案規範

### 語系（強制）
- 專案文件、UI 文案、註解一律使用繁體中文（zh-TW）。
- 程式碼中的識別名稱（類別/方法/變數）保持英文；人類可讀的文字（畫面標題、提示、錯誤訊息、註解）使用繁體中文。

### 文案風格
- 用詞一致、正式、避免口語；量詞、標點依照中文書寫習慣。
- 錯誤訊息應清楚指出原因與建議（例如：「上傳失敗：檔案超過 50MB 上限」）。
- 禁止在正式文案中出現簡體中文或英文混雜（必要專有名詞除外）。

### 多語需求
- 若未來需要國際化，統一使用資源檔（.resx）並以 zh-TW 為主檔，其他語系再加分支資源檔。

---

## 前端彈出視窗（Modal）規範

本規範統一彈出視窗的結構、命名與開關 API，避免「只能開一次」「捲軸不恢復」等問題。實作依據：`HNB/wwwroot/Backoffice/js/Modal.js`（v2.0），測試頁：`HNB/Areas/Backoffice/Views/Test/ModalTest.cshtml`。

### JS API（唯一）
- `showModal(modalId)`：開啟指定 ID 的 Modal
- `closeModal(modalId)`：關閉指定 ID 的 Modal

使用規則：只允許上述兩個函數；請勿使用 jQuery `.show()`/`.hide()` 或自訂 `showXxxModal()`/`closeXxxModal()`。

### HTML 結構要求（必要）
- Modal 容器需具備：
  - 唯一 `id`
  - 類別必含 `fixed` 與 `hidden`
  - 建議遮罩：`inset-0 bg-black bg-opacity-50 backdrop-blur-sm z-50 flex items-center justify-center`
- 內容容器建議：白底、圓角、陰影，並設定最大高度與 `overflow-y-auto`
- 關閉按鈕需呼叫 `closeModal('yourId')`

範例（最小可用）：

```html
<div id="userFormModal" class="hidden fixed inset-0 bg-black bg-opacity-50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
  <div class="bg-white rounded-2xl shadow-2xl max-w-2xl w-full overflow-hidden">
    <div class="px-6 py-4 bg-gradient-to-r from-slate-700 to-slate-800 text-white flex justify-between items-center">
      <h3 class="text-lg font-bold">標題</h3>
      <button type="button" onclick="closeModal('userFormModal')"><i data-lucide="x" class="w-5 h-5"></i></button>
    </div>
    <div class="p-6 overflow-y-auto" style="max-height: calc(90vh - 140px);">
      <!-- 內容區 -->
    </div>
    <div class="px-6 py-4 bg-slate-50 border-t flex justify-end gap-3">
      <button type="button" onclick="closeModal('userFormModal')" class="px-4 py-2 bg-slate-200 rounded-lg">取消</button>
    </div>
  </div>
</div>
<!-- 觸發 -->
<button onclick="showModal('userFormModal')">開啟</button>
```

### ID 命名規範（建議）
- 表單：`{feature}FormModal`（如：`userFormModal`）
- 詳情：`{feature}DetailModal`
- 說明：`{feature}-help`
- 確認：`{action}-modal`（如：`delete-modal`）
- 預覽：`{feature}PreviewModal`

### 關閉互動與鍵盤
- 支援 ESC：自動關閉最上層 Modal（已於 `Modal.js` 內建）
- 關閉方式：X 按鈕、取消按鈕、遮罩點擊（若有實作）均需呼叫 `closeModal()`

### 多層 Modal 與捲軸
- 當存在任一顯示中的 Modal，`body` 會鎖定捲動；全部關閉後自動恢復（`Modal.js` 內建）

### 事件冒泡
- 若按鈕位於可關閉區域內，請使用 `event.stopPropagation()` 再呼叫 `showModal/closeModal`

### 範例參考
- 完整樣式與測試流程：`HNB/Areas/Backoffice/Views/Test/ModalTest.cshtml`
- JS 實作與命名建議：`HNB/wwwroot/Backoffice/js/Modal.js`

---

## 前端 AJAX 規範

本規範統一前端 AJAX 請求的寫法，確保安全性、可讀性與維護性。

### 核心原則（強制）
- 統一使用 jQuery `$.ajax()` 寫法
- 禁止混用 `fetch()`、`XMLHttpRequest`、`$.get()`、`$.post()` 等其他方式
- 所有 POST/PUT/DELETE 請求自動帶 Anti-Forgery Token（已全域配置）
- 錯誤處理統一、簡潔

### 全域配置

在 `_Layout.cshtml` 或每個頁面中加入 Anti-Forgery Token：
```cshtml
@Html.AntiForgeryToken()
```

**重要：** Token 已在 `Program.cs` 全域啟用驗證，所有 POST/PUT/DELETE 請求都會自動檢查。Token 應加在 **請求 body** 中，不是 headers。

### 表單驗證（HTML5 Required）

使用 HTML5 的 `required` 屬性自動驗證，避免手動判斷。

```html
<form id="myForm" onsubmit="submitForm(); return false;">
    <label>
        名稱 <span class="text-red-500">*</span>
    </label>
    <input type="text" name="Name" required placeholder="請輸入名稱">
    
    <button type="submit">提交</button>
</form>
```

```javascript
// ✅ 正確：不需手動判斷，required 會自動檢查
function submitForm() {
    const formData = $("#myForm").serialize();
    $.ajax({ ... });
}

// ❌ 錯誤：不要手動判斷空值
function submitForm() {
    const name = $('#nameInput').val();
    if (!name) { alert('請輸入名稱'); return; }  // ❌ 多餘
    $.ajax({ ... });
}
```

### 標準範本（表單序列化）

表單序列化會自動包含頁面上的 `@Html.AntiForgeryToken()`，無需手動處理。

```javascript
function submitForm() {
    const formData = $("#FormId").serialize();  // ✅ 自動帶 Token
    $.ajax({
        type: 'POST',
        url: '@Url.Action("ActionName", "ControllerName")',
        data: formData,
        success: function (response) {
            if (response.success) {
                alert('操作成功');
                location.reload();
            } else {
                alert(response.message || '操作失敗');
            }
        },
        error: () => alert('系統發生錯誤')
    });
}
```

### 標準範本（JSON）

JSON 請求需手動加入 Token 到資料物件中。

```javascript
function submitData() {
    const data = {
        id: 1,
        name: '測試',
        isActive: true,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()  // ✅ 手動加入
    };
    
    $.ajax({
        type: 'POST',
        url: '@Url.Action("ActionName", "ControllerName")',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                alert('操作成功');
                location.reload();
            } else {
                alert(response.message || '操作失敗');
            }
        },
        error: () => alert('系統發生錯誤')
    });
}
```

### 標準範本（檔案上傳）

手動構建 FormData 時需加入 Token。

```javascript
function uploadFile() {
    const formData = new FormData();
    formData.append('file', $('#fileInput')[0].files[0]);
    formData.append('path', '/uploads');
    formData.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());  // ✅ 手動加入
    
    $.ajax({
        type: 'POST',
        url: '@Url.Action("Upload", "File")',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                alert('上傳成功');
                location.reload();
            } else {
                alert(response.message || '上傳失敗');
            }
        },
        error: () => alert('上傳失敗')
    });
}
```

### 按鈕狀態管理

```javascript
function submitWithButton() {
    const $btn = $('#submitBtn');
    $btn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> 處理中...');
    
    const formData = $("#FormId").serialize();
    $.ajax({
        type: 'POST',
        url: '@Url.Action("ActionName", "ControllerName")',
        data: formData,
        success: function (response) {
            if (response.success) {
                alert('操作成功');
                location.reload();
            } else {
                alert(response.message || '操作失敗');
            }
        },
        error: () => alert('系統發生錯誤'),
        complete: function () {
            $btn.prop('disabled', false).html('<i class="fa fa-save"></i> 提交');
        }
    });
}
```

### 下載檔案

```javascript
function downloadFile(id) {
    const url = '@Url.Action("Download", "File")';
    
    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'id=' + encodeURIComponent(id)
    })
    .then(response => {
        if (!response.ok) throw new Error('下載失敗');
        return response.blob();
    })
    .then(blob => {
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = 'file.zip';
        document.body.appendChild(a);
        a.click();
        a.remove();
    })
    .catch(err => alert(err.message || '下載發生錯誤'));
}
```

### 規範要求

✅ **DO（正確做法）**
- 統一使用 `$.ajax()` 格式
- 使用 `@Url.Action()` 生成 URL
- 使用 HTML5 `required` 屬性驗證必填欄位，不要手動判斷空值
- 按鈕使用 `type="submit"`，表單用 `onsubmit="functionName(); return false;"`
- 明確指定 `type: 'POST'`、`url:`、`data:`
- 檢查 `response.success` 並顯示 `response.message`
- 統一錯誤處理 `error: () => alert('...')`
- JSON 請求明確設定 `contentType: 'application/json'`，並在資料物件中加入 `__RequestVerificationToken`
- 檔案上傳設定 `processData: false, contentType: false`，並在 FormData 中 `append('__RequestVerificationToken', ...)`
- 表單序列化（`serialize()`）自動包含 Token，無需手動處理

❌ **DON'T（錯誤做法）**
- 不要混用 `fetch()`、`XMLHttpRequest`、`axios` 等其他方式（檔案下載除外）
- 不要使用 `$.post()` 或 `$.get()` 快捷方法
- 不要將 Token 放在 headers（應放在 request body）
- 不要在 JS 中手動判斷空值（用 `required` 屬性）
- 不要省略錯誤處理
- 不要使用複雜的 Promise/async-await（保持簡單）
- 不要在同一專案混用多種 AJAX 風格
- 不要在按鈕上用 `onclick="submit()"`（應該用 `type="submit"` + form `onsubmit`）

### Controller 回應格式

統一的 JSON 回應格式：
```csharp
// 成功
return Json(new { success = true, message = "操作成功", data = result });

// 失敗
return Json(new { success = false, message = "操作失敗：原因說明" });
```

### JavaScript 組織原則

- **頁面專屬 CRUD 操作**：寫在 View 的 `<script>` 標籤中（如：`createFolder()`, `confirmDelete()`, `saveFile()`）
- **通用工具函數**：放在獨立 JS 檔案中（如：拖拽上傳、圖片預覽、樹狀結構）

**範例：** `FileManager.cshtml`
- 頁面內 `<script>` 包含：建立/刪除/重新命名/儲存等 CRUD 操作
- `FileManager.js` 包含：拖拽上傳、圖片縮放、目錄樹折疊等通用功能

**優點：**
- 頁面邏輯集中在一個檔案，易於維護
- 可直接使用 `@Url.Action()` 生成 URL
- 減少全域 JS 檔案的複雜度

### 範例參考
- 標準範例：`HistoryManager.cshtml`（專案根目錄）
- 完整實作：`HNB/Areas/Backoffice/Views/FileManager/FileManager.cshtml`

---

## 範例

### 完整範例：AuthRepository

```csharp
public class AuthRepository(HnbHnbBackofficeDbContext db)
{
    #region 統一的查詢來源
    
    private IQueryable<vw_permission_user> ValidUsers 
        => db.vw_permission_users.Where(u => u.is_active == true);
    
    private IQueryable<permission_management> ValidPermissionManagements 
        => db.permission_managements.Where(u => u.type == "user");
    
    #endregion

    #region 專用查詢方法

    // 表1: vw_permission_user
    public List<vw_permission_user> QueryUserList(bool? isActive = null)
        => ValidUsers
            .Where(u => !isActive.HasValue || u.is_active == isActive.Value)
            .ToList();

    public vw_permission_user? QueryUser(int? userId = null, string? usernameOrEmail = null)
    {
        if (userId.HasValue)
            return ValidUsers.FirstOrDefault(u => u.id == userId.Value);
        
        if (!string.IsNullOrEmpty(usernameOrEmail))
            return ValidUsers.FirstOrDefault(u => u.name == usernameOrEmail || u.email == usernameOrEmail);
        
        return null;
    }

    // 表2: permission_management
    public List<permission_management> QueryPermissionUserList(bool? isActive = null)
        => ValidPermissionManagements
            .Where(u => !isActive.HasValue || u.is_active == isActive.Value)
            .ToList();

    public permission_management? QueryPermissionUser(int? userId = null, string? username = null)
    {
        if (userId.HasValue)
            return ValidPermissionManagements.FirstOrDefault(u => u.id == userId.Value);
        
        if (!string.IsNullOrEmpty(username))
            return ValidPermissionManagements.FirstOrDefault(u => u.name == username);
        
        return null;
    }

    #endregion

    #region 基本 CRUD 操作

    public permission_management InsertPermissionUser(permission_management user)
    {
        var existingEntity = db.permission_managements.Find(user.id);
        
        if (existingEntity == null)
        {
            user.type = "user";
            user.created_at = DateTime.Now;
            db.permission_managements.Add(user);
            db.SaveChanges();
            return user;
        }
        
        existingEntity.name = user.name;
        existingEntity.email = user.email;
        // ... 更新所有欄位
        
        if (!string.IsNullOrEmpty(user.password_hash))
        {
            existingEntity.password_hash = user.password_hash;
            existingEntity.salt = user.salt;
        }
        
        existingEntity.updated_at = DateTime.Now;
        db.SaveChanges();
        return existingEntity;
    }

    #endregion
}
```

### 完整範例：AuthService

```csharp
public class AuthService(AuthRepository authRepository)
{
    #region 統一的查詢方法

    public List<vw_permission_user> LoadUserList(bool? isActive = null)
        => authRepository.QueryUserList(isActive);

    public vw_permission_user? LoadUser(int? userId = null, string? usernameOrEmail = null)
        => authRepository.QueryUser(userId, usernameOrEmail);

    public List<permission_management> LoadPermissionUserList(bool? isActive = null)
        => authRepository.QueryPermissionUserList(isActive);

    public permission_management? LoadPermissionUser(int? userId = null, string? username = null)
        => authRepository.QueryPermissionUser(userId, username);

    #endregion

    #region 基本 CRUD 操作

    public permission_management CreatePermissionUser(permission_management user)
        => authRepository.InsertPermissionUser(user);

    #endregion

    #region 輔助方法

    public vw_permission_user? ValidateUserCredentials(string username, string password)
    {
        var user = LoadUser(usernameOrEmail: username);
        if (user == null) return null;

        var isValidPassword = VerifyPassword(password, user.password_hash, user.salt);
        return isValidPassword ? user : null;
    }

    public (bool success, string? errorMessage, vw_permission_user? user) ProcessLogin(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return (false, "請输入帳號和密碼", null);

        var user = ValidateUserCredentials(username, password);
        if (user == null)
            return (false, "帳號或密碼錯誤", null);

        return (true, null, user);
    }

    private bool VerifyPassword(string password, string? hash, string? salt)
    {
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt)) return false;
        // 密碼驗證邏輯
        return true;
    }

    #endregion
}
```

---

## 快速检查清单

在編寫程式碼時，請检查：

### Repository 層
- [ ] 使用了几張表？
- [ ] 每張表都有 `Query*List` 和 `Query*` 方法嗎？
- [ ] 每張表只有一個 `Insert*` 方法嗎？
- [ ] 没有使用 `Get*` 命名嗎？（应使用 `Query*`）
- [ ] 没有使用 `async`/`await` 嗎？
- [ ] 没有使用 `try...catch` 嗎？
- [ ] 使用 `=>` 表達式了嗎？
- [ ] 有統一的查詢来源（`IQueryable`）嗎？

### Service 層
- [ ] 每個 `Query*` 都有對应的 `Load*` 嗎？
- [ ] 每個 `Insert*` 都有對应的 `Create*` 嗎？
- [ ] 没有使用 `Get*` 命名嗎？（应使用 `Load*`）
- [ ] 没有使用 `async`/`await` 嗎？
- [ ] 没有使用 `try...catch` 嗎？
- [ ] 使用 `=>` 表達式了嗎？
- [ ] 業務邏輯放在辅助方法中了嗎？

---

## 常见錯誤

###  錯誤 1：使用 Get 命名
```csharp
// Repository 層
public User? GetUser(int id) { }
public List<User> GetUserList() { }
public List<string> GetOccupiedRoleIds(int orgId) { }

// Service 層
public User? GetUser(int id) { }
public List<User> GetUsers() { }
```
**正確做法：** Repository 使用 `Query*`，Service 使用 `Load*`
```csharp
// Repository 層
public User? QueryUser(int id) { }
public List<User> QueryUserList() { }
public List<string> QueryOccupiedRoleIdList(int orgId) { }

// Service 層
public User? LoadUser(int id) { }
public List<User> LoadUserList() { }
```

###  錯誤 2：一張表多個 Insert
```csharp
public permission_management InsertUser(permission_management user) { }
public permission_management InsertRole(permission_management role) { }
public permission_management InsertOrganization(permission_management org) { }
```
**正確做法：** 合并為一個 `InsertPermissionManagement`，在方法内根據 `type` 欄位處理不同邏輯。

###  錯誤 3：缺少對应的 Query 方法
```csharp
// 使用了 permission_management 表，但只有
public permission_management? QueryUserByName(string name) { }
public permission_management? QueryRoleById(int id) { }
// 缺少 QueryPermissionManagementList 和 QueryPermissionManagement
```
**正確做法：** 為每張表添加完整的 `Query*List` 和 `Query*` 方法。

###  錯誤 4：Repository 包含業務邏輯
```csharp
public List<string> QueryUserNavigationPermissions(string userName)
{
    var permissions = new List<string>();
    permissions.AddRange(new[] { "dashboard", "profile" });
    
    var user = db.users.Where(u => u.name == userName).FirstOrDefault();
    // ... 复杂的权限计算邏輯
    
    return permissions.Distinct().ToList();
}
```
**正確做法：** Repository 只做簡單查詢，業務邏輯放在 Service 層。

---

## 版本历史

- **v1.2** (2025-10-19) - 前端規範與安全性強化
  - 新增「ViewBag 與 Model 規範」：限定 ViewBag 僅傳唯一值，資料用強型別 Model
  - 新增「語言與文案規範」：強制使用繁體中文（zh-TW）
  - 新增「前端彈出視窗（Modal）規範」：統一 Modal API 與 HTML 結構
  - 新增「前端 AJAX 規範」：統一使用 jQuery $.ajax()，禁止混用 fetch/XHR
  - 新增「JavaScript 組織原則」：頁面專屬邏輯放 View 內，通用工具放獨立 JS
  - 簡化「命名規則」為「命名速查表」
  - 加入 HTML5 表單驗證規範（使用 required 屬性）
  - 加入 Anti-Forgery Token 全域配置與使用說明
  - 強化 Controller ViewBag 使用方式說明

- **v1.1** (2025-01-09) - 添加禁止 Get 命名規則
  - 新增核心原則：禁止使用 `Get*` 命名
  - Repository 層必須使用 `Query*` 命名
  - Service 層必須使用 `Load*` 命名
  - 添加相关錯誤範例和检查清单

- **v1.0** (2025-01-09) - 初始版本
  - 建立基础架構規範
  - 定义命名規則
  - 确立核心原則

---

## 參考資料

- 專案结構：`HNB/Areas/Backoffice/`
- 範例 Repository：`AuthRepository.cs`, `PermissionManagementRepository.cs`
- 範例 Service：`AuthService.cs`, `PermissionManagementService.cs`

---

**注意：** 所有開發者在編寫新程式碼或修改現有程式碼時，必須遵循本規範。如有疑問，請參考現有的 `AuthRepository` 和 `AuthService` 作為標準範例。

