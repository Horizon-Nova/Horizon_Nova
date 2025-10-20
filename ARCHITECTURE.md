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
    
    // 複雜的業務邏輯
    public (bool success, string message) ProcessBusinessLogic(...)
    {
        var data = LoadTable1(id);
        if (data == null)
            return (false, "資料不存在");
        
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
- 專案文件、UI 文案、註解、對話一律使用繁體中文（zh-TW）。
- 程式碼中的識別名稱（類別/方法/變數）保持英文；人類可讀的文字（畫面標題、提示、錯誤訊息、註解）使用繁體中文。

### 文案風格
- 用詞一致、正式、避免口語；量詞、標點依照中文書寫習慣。
- 錯誤訊息應清楚指出原因與建議（例如：「上傳失敗：檔案超過 50MB 上限」）。
- 禁止在正式文案中出現簡體中文或英文混雜（必要專有名詞除外）。
- **禁止使用 Emoji**：專案文件、程式碼註解、UI 文案、提交訊息等一律不使用表情符號，保持專業與正式。

### 多語需求
- 若未來需要國際化，統一使用資源檔（.resx）並以 zh-TW 為主檔，其他語系再加分支資源檔。

---

## 前端彈出視窗（Modal）規範

本規範統一彈出視窗的結構、命名與開關 API，避免「只能開一次」「捲軸不恢復」等問題。實作依據：`HNB/wwwroot/Backoffice/js/Modal.js`（v5.1）。

### JS API（唯一）
- `showModal(modalId, options)`：開啟指定 ID 的 Modal（支援靜態顯示與 AJAX 載入）
- `closeModal(modalId)`：關閉指定 ID 的 Modal

使用規則：只允許上述兩個函數；請勿使用 jQuery `.show()`/`.hide()` 或自訂 `showXxxModal()`/`closeXxxModal()`。

### 使用方式

#### 場景 1：靜態 Modal（頁面已有內容）
```html
<!-- 直接顯示 Modal -->
<button onclick="showModal('myModal')">開啟模態框</button>

<div id="myModal" class="modal fade" tabindex="-1">
    <!-- Modal 內容 -->
</div>
```

#### 場景 2：動態 Modal（AJAX 載入 PartialView）
```html
<!-- AJAX 載入後顯示 -->
<button onclick="showModal('editModal', {
    url: '/Backoffice/User/LoadDetail',
    method: 'GET',
    data: { id: 123 },
    container: 'modalContainer'
})">編輯使用者</button>

<!-- Modal 容器 -->
<div id="modalContainer">
    <!-- PartialView 將載入到這裡 -->
</div>
```

#### 參數說明
| 參數 | 類型 | 必填 | 說明 |
|------|------|------|------|
| `modalId` | string | | Modal 的 ID |
| `options.url` | string | X| AJAX 請求 URL |
| `options.method` | string | X| HTTP 方法（預設 `GET`） |
| `options.data` | object | X| 請求資料 |
| `options.container` | string | X| 要替換內容的容器 ID（預設與 `modalId` 相同） |

**重要：** 當指定 `container` 時，`showModal` 會：
1. 使用 AJAX 載入 PartialView
2. 將 HTML 內容替換到 `#container` 容器中
3. 顯示 `#modalId` 的 Modal

#### 完整範例（NavigationManagement.js）
```javascript
// 便捷函數：顯示編輯 Modal
const showEditModal = (id) => showModal('nav-edit-modal', {
    url: '/Backoffice/SidebarNavigation/LoadDetail',
    method: 'GET',
    data: { id: id },
    container: 'navigationModals'  // 替換整個容器
});

// 靜態 Modal：直接顯示
const openIconPicker = () => {
    loadIconsFromAPI();
    showModal('icon-picker-modal');
};
```

### HTML 結構要求（必要）

Modal 使用 **Bootstrap Modal** 標準結構：

#### 最小可用範例
```html
<!-- Modal 容器 -->
<div id="myModal" class="modal fade" tabindex="-1" aria-labelledby="myModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <!-- 標題列 -->
            <div class="modal-header">
                <h5 class="modal-title">標題</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="關閉"></button>
            </div>
            
            <!-- 內容區 -->
            <div class="modal-body">
                <p>Modal 內容</p>
            </div>
            
            <!-- 按鈕區 -->
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                <button type="button" class="btn btn-primary">確認</button>
            </div>
        </div>
    </div>
</div>

<!-- 觸發按鈕 -->
<button onclick="showModal('myModal')" class="btn btn-primary">開啟 Modal</button>
```

#### 結構說明
| 元素 | 類別 | 必要 | 說明 |
|------|------|------|------|
| 外層容器 | `.modal .fade` | | Modal 主容器，需有唯一 `id` |
| 對話框 | `.modal-dialog` | | 控制寬度與位置 |
| 內容容器 | `.modal-content` | | 實際內容區域 |
| 標題列 | `.modal-header` | X| 標題與關閉按鈕 |
| 內容區 | `.modal-body` | | 主要內容 |
| 按鈕區 | `.modal-footer` | X| 操作按鈕 |

#### 尺寸選項
```html
<!-- 小型 -->
<div class="modal-dialog modal-sm">...</div>

<!-- 標準（預設） -->
<div class="modal-dialog">...</div>

<!-- 大型 -->
<div class="modal-dialog modal-lg">...</div>

<!-- 超大型 -->
<div class="modal-dialog modal-xl">...</div>

<!-- 置中顯示 -->
<div class="modal-dialog modal-dialog-centered">...</div>

<!-- 可滾動內容 -->
<div class="modal-dialog modal-dialog-scrollable">...</div>
```

### ID 命名規範（建議）
```javascript
// 表單 Modal
'user-form-modal', 'nav-add-modal', 'nav-edit-modal'

// 詳情 Modal
'user-detail-modal', 'nav-detail-modal'

// 確認 Modal
'delete-modal', 'nav-delete-modal'

// 選擇器 Modal
'icon-picker-modal', 'file-picker-modal'

// 說明 Modal
'navigation-help', 'user-help'
```

### 關閉方式
Bootstrap Modal 支援多種關閉方式：

```html
<!-- 方式 1：使用 data-bs-dismiss（推薦） -->
<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>

<!-- 方式 2：呼叫 closeModal() -->
<button type="button" class="btn btn-secondary" onclick="closeModal('myModal')">取消</button>

<!-- 方式 3：ESC 鍵（自動支援） -->
<!-- 按下 ESC 會自動關閉最上層 Modal -->

<!-- 方式 4：點擊背景遮罩（需配置） -->
<!-- Modal.js 預設使用 backdrop: 'static'，不支援點擊背景關閉 -->
```

### 多層 Modal 與捲軸管理
- `Modal.js` 自動管理 `body` 捲動鎖定
- 開啟任一 Modal 時，`body` 會鎖定捲動
- 關閉所有 Modal 後，自動恢復 `body` 捲動

### 自動功能
以下功能由 `Modal.js` 自動處理，無需額外程式碼：
- Lucide 圖標重新初始化
- Body 捲動鎖定/恢復
- ESC 鍵關閉最上層 Modal
- Bootstrap Modal 事件監聽
- 開啟 Modal 追蹤管理

### 檔案命名與架構規範（強制）

#### 命名格式
```
主畫面：{PageName}.cshtml
Modal 檔案：_{PageName}Modal.cshtml
```

**重要：** 所有 Modal 集中在**一個檔案**中，不要拆分成多個檔案。

#### 標準架構範例

**主畫面：NavigationManagement.cshtml**
```cshtml
@{
    var allNavigations = ViewBag.Navigations as List<vw_sidebar_navigation>;
}

<!-- 動態 Modal 容器（會被 AJAX 重新載入）-->
<div id="navigationModals">
    @await Html.PartialAsync("_NavigationManagementModal", new vw_sidebar_navigation())
</div>

<!-- 靜態 Modal（不會被 AJAX 重新載入）-->
<div id="icon-picker-modal" class="modal fade">
    <div class="modal-body">
        <div id="iconGrid"></div>
    </div>
</div>

<!-- 頁面內容 -->
<button onclick="showModal('nav-add-modal')">新增</button>
<button onclick="showEditModal(123)">編輯</button>
<button onclick="showModal('icon-picker-modal')">選擇圖標</button>
```

**Modal 檔案：_NavigationManagementModal.cshtml**
```cshtml
@model vw_sidebar_navigation
@{
    var model = Model ?? new vw_sidebar_navigation();
}

<!-- 新增 Modal（使用空數據）-->
<div id="nav-add-modal" class="modal fade">
    <form>
        <input type="text" name="title" value="" />
    </form>
</div>

<!-- 編輯 Modal（使用 model 數據）-->
<div id="nav-edit-modal" class="modal fade">
    <form>
        <input type="text" name="title" value="@(model.title ?? "")" />
    </form>
</div>

<!-- 詳情 Modal（使用 model 數據）-->
<div id="nav-detail-modal" class="modal fade">
    <p>@(model.title ?? "-")</p>
</div>

<!-- 刪除確認 Modal（使用 model 數據）-->
<div id="nav-delete-modal" class="modal fade">
    <p>確認刪除：@(model.title ?? "")</p>
</div>

<!-- 注意：icon-picker-modal 不在這裡，因為它是完全靜態的，放在主畫面中 -->
```

#### 核心設計模式（重要）

**這是簡化開發的關鍵設計，雖然不常見，但非常有效：**

1. **一個檔案包含所有需要數據的 Modal**
   - 新增、編輯、詳情、刪除等需要 Model 數據的 Modal 放在 PartialView 中
   - 易於維護和管理

2. **通過 `@model` 控制內容**
   - 頁面初始載入：傳入空 model（用於新增 Modal）
   - 需要數據時：通過 AJAX 重新載入並傳入特定 model

3. **AJAX 重新載入整個 Modal 容器**
   - 不是只載入單個 Modal，而是重新載入整個容器
   - 雖然看似"浪費"，但實際上簡化了架構

4. **完全靜態的 Modal 放在主畫面**（重要例外）
   - 如圖標選擇器、顏色選擇器等**通用且不需要數據**的 Modal
   - 應該放在主畫面中，避免被 AJAX 重新載入覆蓋
   - 這樣可以保持其狀態（例如已載入的圖標列表）

**區分標準：**
- 需要 `@model` 數據 → 放在 `_NavigationManagementModal.cshtml`
- 完全靜態/通用 → 放在主畫面 `NavigationManagement.cshtml`

#### 工作流程

**步驟 1：頁面初始載入**
```cshtml
<!-- 主畫面載入時 -->
<div id="navigationModals">
    @await Html.PartialAsync("_NavigationManagementModal", new vw_sidebar_navigation())
</div>

<!-- 結果：所有 Modal 都已存在，但編輯/詳情/刪除 Modal 是空數據 -->
```

**步驟 2：新增操作（不需要 AJAX）**
```javascript
// 直接顯示已存在的 Modal（空數據正好符合新增需求）
showModal('nav-add-modal');
```

**步驟 3：編輯操作（需要 AJAX 載入數據）**
```javascript
// 通過 AJAX 重新載入整個容器，並傳入特定數據
const showEditModal = (id) => showModal('nav-edit-modal', {
    url: '/Backoffice/SidebarNavigation/LoadDetail',
    method: 'GET',
    data: { id: id },
    container: 'navigationModals'  // 替換整個容器
});

// Modal.js 執行：
// 1. AJAX GET LoadDetail(id)
// 2. Controller 返回 PartialView("_NavigationManagementModal", model)
// 3. 將 HTML 替換到 #navigationModals
// 4. 顯示 #nav-edit-modal
```

**步驟 4：Controller 實作**
```csharp
/// <summary>
/// 載入 Modal（通用：編輯、詳情、刪除都用這個）
/// </summary>
public IActionResult LoadDetail(int id)
{
    var model = service.LoadNavigationById(id);
    return PartialView("_NavigationManagementModal", model);
}
```

#### 設計優點

1. **集中管理**
   - 所有 Modal HTML 在一個檔案中
   - 修改樣式或結構只需改一個檔案

2. **簡化 Controller**
   - 只需要一個 `LoadDetail` 方法
   - 不需要為每個 Modal 建立單獨的 Action

3. **簡化 JavaScript**
   - 不需要複雜的數據填充邏輯
   - Modal 內容由伺服器端渲染

4. **型別安全**
   - 使用強型別 `@model`
   - 編譯時期檢查

5. **易於維護**
   - Modal 之間共用相同的 Model
   - 欄位變更只需修改一次

#### 常見誤解

**誤解 1：每次都重新載入所有 Modal 會影響效能**
- 實際上：Modal 的 HTML 很小，網路傳輸成本極低
- 優點：避免複雜的狀態管理和數據同步

**誤解 2：應該為每個 Modal 建立獨立的 PartialView**
- 實際上：這會導致重複程式碼和維護困難
- 正確做法：使用同一個模板根據數據動態顯示

**誤解 3：這種做法不符合 RESTful 原則**
- 實際上：這是 MVC PartialView 的標準用法
- GET 請求獲取 HTML 片段是常見且合理的設計

#### 錯誤範例

**錯誤 1：拆分成多個檔案**
```
主畫面：NavigationManagement.cshtml

Modal 檔案：
- _NavigationManagementEditModal.cshtml     ← 錯誤：不要拆分
- _NavigationManagementDetailModal.cshtml   ← 錯誤：不要拆分
- _NavigationManagementDeleteModal.cshtml   ← 錯誤：不要拆分
```

**錯誤 2：為每個 Modal 建立專用的 Controller Action**
```csharp
// 錯誤：不需要這麼多 Action
public IActionResult LoadEditModal(int id) { }
public IActionResult LoadDetailModal(int id) { }
public IActionResult LoadDeleteModal(int id) { }

// 正確：只需要一個
public IActionResult LoadDetail(int id) { }
```

**錯誤 3：檔案名稱不包含完整的主畫面名稱**
```
主畫面：NavigationManagement.cshtml
Modal 檔案：_NavigationModal.cshtml    ← 錯誤：缺少 Management
正確：_NavigationManagementModal.cshtml
```

### 實際範例參考
- **完整實作**：`HNB/wwwroot/Backoffice/js/Modal.js` (v5.1)
- **使用範例**：`HNB/wwwroot/Backoffice/js/NavigationManagement.js`
- **HTML 範例**：`HNB/Areas/Backoffice/Views/SidebarNavigation/_NavigationManagementModal.cshtml`
- **頁面範例**：`HNB/Areas/Backoffice/Views/SidebarNavigation/NavigationManagement.cshtml`

### 重要提示

**Lucide 圖標初始化時機：**
- Modal.js 會在 Modal 完全顯示後（`shown.bs.modal` 事件）自動初始化 Lucide 圖標
- 不需要在頁面 JavaScript 中手動調用 `lucide.createIcons()`
- 這確保圖標在 Modal 可見時才初始化，避免初始化失敗

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

### 表單驗證（HTML5 Required）（強制）

**核心原則：** 使用 HTML5 的 `required` 屬性自動驗證，**禁止在 JavaScript 中手動判斷空值**。

#### 正確做法
```html
<!-- HTML：使用 required 屬性 -->
<form id="navForm">
    <label class="form-label fw-semibold">
        目錄名稱 <span class="text-danger">*</span>
    </label>
    <input type="text" id="navTitle" name="title" class="form-control" 
           placeholder="請輸入目錄名稱" required>
    
    <label class="form-label fw-semibold">
        目錄代碼 <span class="text-danger">*</span>
    </label>
    <input type="text" id="navCode" name="code" class="form-control" 
           placeholder="例如: dashboard" required>
    
    <button type="button" onclick="saveNavigation()" class="btn btn-primary">儲存</button>
</form>
```

```javascript
// 正確：不需手動判斷，required 會自動檢查
function saveNavigation() {
    const formData = $('#navForm').serialize();
    $.ajax({
        type: 'POST',
        url: '/Backoffice/Navigation/Submit',
        data: formData,
        success: function(response) {
            if (response.success) {
                alert('儲存成功');
                location.reload();
            } else {
                alert(response.message || '儲存失敗');
            }
        },
        error: () => alert('失敗，系統發生錯誤。')
    });
}

// X錯誤：不要手動判斷空值
function saveNavigation() {
    const title = $('#navTitle').val();
    const code = $('#navCode').val();
    
    // X完全不需要這些判斷
    if (!title || !code) {
        alert('請填寫必填欄位');
        return;
    }
    
    $.ajax({ ... });
}
```

#### 進階：自訂驗證訊息（可選）

```html
<input type="text" name="title" required 
       oninvalid="this.setCustomValidity('請輸入目錄名稱')"
       oninput="this.setCustomValidity('')">
```

#### 其他驗證類型

```html
<!-- Email 驗證 -->
<input type="email" name="email" required placeholder="請輸入 Email">

<!-- 數字範圍驗證 -->
<input type="number" name="sort_order" min="0" max="999" required>

<!-- 長度驗證 -->
<input type="text" name="code" required minlength="3" maxlength="50">

<!-- 正則表達式驗證 -->
<input type="text" name="code" required pattern="[a-z_]+" 
       title="僅允許英文小寫與底線">
```

### 標準範本（表單序列化）

表單序列化會自動包含頁面上的 `@Html.AntiForgeryToken()`，無需手動處理。

```javascript
function submitForm() {
    const formData = $("#FormId").serialize();  // 自動帶 Token
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
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()  // 手動加入
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
    formData.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());  // 手動加入
    
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

### jQuery 使用規範（強制）

**核心原則：** 統一使用 jQuery 語法，**禁止混用原生 JavaScript DOM 操作**。

**適用範圍：** 
- 所有 JavaScript 檔案（包括 `Modal.js`、`NavigationManagement.js` 等）
- 頁面專屬腳本（View 中的 `<script>` 標籤）
- 所有自定義元件與函數

#### 元素選取與操作

```javascript
// 正確：統一使用 jQuery
const value = $('#inputId').val();
$('#elementId').text('新文字');
$('#elementId').html('<strong>HTML</strong>');
$('#checkboxId').prop('checked', true);
$('#selectId').val('option1');

// X錯誤：不要使用 document.getElementById
const value = document.getElementById('inputId').value;
document.getElementById('elementId').textContent = '新文字';
document.getElementById('elementId').innerHTML = '<strong>HTML</strong>';
document.getElementById('checkboxId').checked = true;
```

#### 事件綁定

```javascript
// 正確：使用 jQuery .on()
$('#buttonId').on('click', () => { ... });
$('#inputId').on('input', applyFilters);
$('#selectId').on('change', loadData);

// X錯誤：不要使用 addEventListener
document.getElementById('buttonId').addEventListener('click', () => { ... });
```

#### 表單操作

```javascript
// 正確：使用 jQuery 方法
$('#formId')[0]?.reset();              // 重置表單
const formData = $('#formId').serialize();  // 序列化

// X錯誤：混用原生方法
document.getElementById('formId').reset();
```

### 規範要求

**DO（正確做法）**
- **統一使用 jQuery 語法** - `$('#id')` 取代 `document.getElementById()`
- 統一使用 `$.ajax()` 格式
- 使用 `@Url.Action()` 生成 URL（或直接寫 `/Area/Controller/Action`）
- **使用 HTML5 `required` 屬性驗證必填欄位**，絕對不要在 JavaScript 中手動判斷空值
- 所有必填欄位必須標記 `<span class="text-danger">*</span>`
- 明確指定 `type: 'POST'`、`url:`、`data:`
- 使用 `$('#formId').serialize()` 序列化表單資料
- 使用 jQuery 方法：`.val()`、`.text()`、`.html()`、`.prop()`、`.on()`
- 檢查 `response.success` 並顯示 `response.message`
- 統一錯誤處理 `error: () => alert('失敗，系統發生錯誤。')`
- JSON 請求明確設定 `contentType: 'application/json'`，並在資料物件中加入 `__RequestVerificationToken`
- 檔案上傳設定 `processData: false, contentType: false`，並在 FormData 中 `append('__RequestVerificationToken', ...)`
- 表單序列化（`serialize()`）自動包含 Token，無需手動處理

X**DON'T（錯誤做法）**
- **絕對不要使用 `document.getElementById()`、`document.querySelector()` 等原生方法**
- **絕對不要在 JavaScript 中手動判斷空值**（如：`if (!title) { alert('...'); return; }`）
- 不要混用 `fetch()`、`XMLHttpRequest`、`axios` 等其他方式（檔案下載除外）
- 不要使用 `$.post()` 或 `$.get()` 快捷方法
- 不要使用 `.addEventListener()`（應用 `.on()`）
- 不要使用 `.textContent`、`.innerHTML`、`.value`（應用 `.text()`、`.html()`、`.val()`）
- 不要將 Token 放在 headers（應放在 request body）
- 不要省略錯誤處理
- 不要使用複雜的 Promise/async-await（保持簡單）
- 不要在同一專案混用多種 AJAX 風格
- 不要使用 `$('#input').val()` 來驗證（應用 `required` 屬性）

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
    // ... 複雜的權限計算邏輯
    
    return permissions.Distinct().ToList();
}
```
**正確做法：** Repository 只做簡單查詢，業務邏輯放在 Service 層。

---

## 版本历史

- **v1.3.1** (2025-10-20) - Modal.js 圖標初始化修正
  - 升級 `Modal.js` 至 v5.1：修正 Lucide 圖標初始化時機
  - Lucide 圖標改在 `shown.bs.modal` 事件後初始化，確保 Modal 完全可見
  - 新增靜態 Modal 規範：完全靜態的 Modal（如圖標選擇器）應放在主畫面而非 PartialView
  - 更新設計模式說明：區分需要數據的 Modal 與完全靜態的 Modal

- **v1.3** (2025-10-20) - Modal.js 升級與 jQuery 規範強化
  - 升級 `Modal.js` 至 v5.0：支援靜態與動態 AJAX 載入兩種模式
  - 統一使用 jQuery 語法：禁止混用原生 JavaScript DOM 操作
  - 採用 Bootstrap Modal API：標準化 Modal 結構與行為
  - 新增 Modal AJAX 載入參數：`url`、`method`、`data`、`container`
  - 更新 Modal 規範：完整的使用範例與參數說明
  - 強制要求：所有 DOM 操作必須使用 jQuery（`$('#id')` 而非 `document.getElementById()`）
  - 範例更新：`NavigationManagement.js` 完全符合新規範

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

