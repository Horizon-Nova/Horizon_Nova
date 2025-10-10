# Horizon Nova 架構規範文件

## 目錄
- [概述](#概述)
- [Repository 層規範](#repository-層規範)
- [Service 層規範](#service-層規範)
- [Controller 層規範](#controller-層規範)
- [命名規則](#命名規則)
- [程式碼規範](#程式碼規範)
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
- 在主頁面的 Action 中呼叫 `sev.ViewBagModel(ViewBag)`
  ```csharp
  public IActionResult PageName()
  {
      sev.ViewBagModel(ViewBag);  // ← 設定初始資料
      return View();
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

## 命名規則

### Repository 層

```csharp
// 格式：Query + 表名 + List/无后缀
QueryUserList()              // 用户列表
QueryUser(int? id = null)    // 单個用户

QueryPermissionUserList()    // 权限用户列表
QueryPermissionUser(int id)  // 单個权限用户

// 格式：Insert/Delete + 表名
InsertUser(user)             // 插入/更新用户
DeleteUser(int id)           // 刪除用户
```

### Service 層

```csharp
// 格式：Load + 表名 + List/无后缀
LoadUserList()               // 載入用户列表
LoadUser(int id)             // 載入单個用户

// 格式：Create/Delete + 表名
CreateUser(user)             // 創建/更新用户
DeleteUser(int id)           // 刪除用户

// ViewBag 方法
ViewBagModel(viewBag, id)    // 設定 ViewBag
```

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

