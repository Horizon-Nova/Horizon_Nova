# Horizon Nova 架构规范文档

## 📋 目录
- [概述](#概述)
- [Repository 层规范](#repository-层规范)
- [Service 层规范](#service-层规范)
- [命名规则](#命名规则)
- [代码规范](#代码规范)
- [示例](#示例)

---

## 概述

本项目采用三层架构：**Controller → Service → Repository → Database**

### 核心原则
1. **一张表只会出现一个 Insert*** - 避免重复的 CRUD 方法
2. **使用几个表就要做多少 Query*List、Query*** - 确保完整的查询覆盖
3. **不使用 Async** - 保持同步操作
4. **不使用 try...catch** - 使用 `=>` 表达式和 `??` 操作符
5. **Repository 只做简单查询** - 业务逻辑放在 Service 层
6. **不可使用 Get 命名** - 查询或调用数据库资料一律使用 `Query*` / `Load*` 命名

---

## Repository 层规范

### 结构模板

```csharp
public class ExampleRepository(DbContext db)
{
    #region 統一的查詢來源
    
    private IQueryable<Table1> ValidTable1s => db.table1s.Where(...).OrderBy(...);
    private IQueryable<Table2> ValidTable2s => db.table2s.Where(...);
    
    #endregion

    #region 專用查詢方法
    
    // 表1 查询
    public List<Table1> QueryTable1List(参数...) => ValidTable1s.Where(...).ToList();
    public Table1? QueryTable1(int? id = null, string? name = null) { ... }
    
    // 表2 查询
    public List<Table2> QueryTable2List(参数...) => ValidTable2s.Where(...).ToList();
    public Table2? QueryTable2(int? id = null) => ValidTable2s.FirstOrDefault(...);
    
    #endregion

    #region 基本 CRUD 操作
    
    // 一张表只有一个 Insert 方法
    public Table InsertTable(Table data)
    {
        var existingEntity = db.tables.Find(data.id);
        
        if (existingEntity == null)
        {
            // 新增逻辑
            data.created_at = DateTime.Now;
            db.tables.Add(data);
            db.SaveChanges();
            return data;
        }
        
        // 更新逻辑
        existingEntity.field1 = data.field1;
        // ... 更新所有字段
        
        // 条件性更新（如密码）
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

### 命名规则

| 方法类型 | 命名格式 | 说明 | 示例 |
|---------|---------|------|------|
| 列表查询 | `Query*表名*List` | 返回列表 | `QueryUserList()` |
| 单一查询 | `Query*表名*` | 返回单个对象 | `QueryUser(int? id = null)` |
| 插入/更新 | `Insert*表名*` | 新增或更新 | `InsertUser(user)` |
| 删除 | `Delete*表名*` | 删除记录 | `DeleteUser(int id)` |

### 重要规则

#### ✅ 正确示例

```csharp
// 使用 2 张表的 Repository
public class AuthRepository(DbContext db)
{
    // 表1: vw_permission_user
    public List<vw_permission_user> QueryUserList(...) { }
    public vw_permission_user? QueryUser(...) { }
    
    // 表2: permission_management
    public List<permission_management> QueryPermissionUserList(...) { }
    public permission_management? QueryPermissionUser(...) { }
    
    // 只有 1 个 Insert 方法（针对 permission_management 表）
    public permission_management InsertPermissionUser(permission_management user) { }
}
```

#### ❌ 错误示例

```csharp
// ❌ 一张表出现多个 Insert
public permission_management InsertUser(permission_management user) { }
public permission_management InsertRole(permission_management role) { }
public permission_management InsertOrganization(permission_management org) { }

// ❌ 使用了 Async
public async Task<User?> QueryUserAsync(int id) { }

// ❌ 使用了 try...catch
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

// ❌ 缺少对应的 Query 方法
// 使用了 permission_management 表，但没有 QueryPermissionManagementList 和 QueryPermissionManagement
```

---

## Service 层规范

### 结构模板

```csharp
public class ExampleService(ExampleRepository repo)
{
    #region 統一的查詢方法
    
    // 对应 Repository 的 Query 方法
    public List<Table1> LoadTable1List(参数...) => repo.QueryTable1List(参数...);
    public Table1? LoadTable1(int? id = null) => repo.QueryTable1(id);
    
    public List<Table2> LoadTable2List(参数...) => repo.QueryTable2List(参数...);
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
    
    // 复杂的业务逻辑
    public (bool success, string message) ProcessBusinessLogic(...)
    {
        var data = LoadTable1(id);
        if (data == null)
            return (false, "数据不存在");
        
        // 业务逻辑处理
        // ...
        
        return (true, "处理成功");
    }
    
    #endregion
}
```

### 命名规则

| 方法类型 | 命名格式 | 说明 | 示例 |
|---------|---------|------|------|
| 列表载入 | `Load*表名*List` | 调用 Query*List | `LoadUserList()` |
| 单一载入 | `Load*表名*` | 调用 Query* | `LoadUser(int id)` |
| 创建/更新 | `Create*表名*` | 调用 Insert* | `CreateUser(user)` |
| 删除 | `Delete*表名*` | 调用 Delete* | `DeleteUser(int id)` |
| ViewBag | `ViewBagModel` | 设置 ViewBag | `ViewBagModel(viewBag, id)` |

---

## 命名规则

### Repository 层

```csharp
// 格式：Query + 表名 + List/无后缀
QueryUserList()              // 用户列表
QueryUser(int? id = null)    // 单个用户

QueryPermissionUserList()    // 权限用户列表
QueryPermissionUser(int id)  // 单个权限用户

// 格式：Insert/Delete + 表名
InsertUser(user)             // 插入/更新用户
DeleteUser(int id)           // 删除用户
```

### Service 层

```csharp
// 格式：Load + 表名 + List/无后缀
LoadUserList()               // 载入用户列表
LoadUser(int id)             // 载入单个用户

// 格式：Create/Delete + 表名
CreateUser(user)             // 创建/更新用户
DeleteUser(int id)           // 删除用户

// ViewBag 方法
ViewBagModel(viewBag, id)    // 设置 ViewBag
```

---

## 代码规范

### 1. 使用表达式 (`=>`)

✅ **正确**
```csharp
public List<User> QueryUserList(bool? isActive = null)
    => ValidUsers.Where(u => !isActive.HasValue || u.is_active == isActive.Value).ToList();

public User? QueryUser(int id)
    => ValidUsers.FirstOrDefault(u => u.id == id);

public User CreateUser(User user)
    => repo.InsertUser(user);
```

❌ **错误**
```csharp
public List<User> QueryUserList(bool? isActive = null)
{
    return ValidUsers.Where(u => !isActive.HasValue || u.is_active == isActive.Value).ToList();
}
```

### 2. 不使用 Get 命名

✅ **正确 - Repository 使用 Query***
```csharp
public List<User> QueryUserList() { }
public User? QueryUser(int id) { }
public List<string> QueryOccupiedRoleIdList(int orgId) { }
public Dictionary<string, string> QueryOccupiedRoles(List<string> roleIds) { }
```

✅ **正确 - Service 使用 Load***
```csharp
public List<User> LoadUserList() { }
public User? LoadUser(int id) { }
```

❌ **错误 - 使用 Get 命名**
```csharp
public User? GetUser(int id) { }
public List<User> GetUsers() { }
public List<string> GetOccupiedRoleIds(int orgId) { }
```

### 3. 不使用 Async

✅ **正确**
```csharp
public User? QueryUser(int id)
    => db.users.FirstOrDefault(u => u.id == id);
```

❌ **错误**
```csharp
public async Task<User?> QueryUserAsync(int id)
    => await db.users.FirstOrDefaultAsync(u => u.id == id);
```

### 4. 不使用 try...catch

✅ **正确**
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
        message = result != null ? "创建成功" : "创建失败" 
    });
}
```

❌ **错误**
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

### 5. 可选参数合并查询

✅ **正确 - 单个方法使用可选参数**
```csharp
public User? QueryUser(int? userId = null, string? username = null)
{
    if (userId.HasValue)
        return ValidUsers.FirstOrDefault(u => u.id == userId.Value);
    
    if (!string.IsNullOrEmpty(username))
        return ValidUsers.FirstOrDefault(u => u.name == username);
    
    return null;
}

// 调用
var user1 = QueryUser(userId: 123);
var user2 = QueryUser(username: "admin");
```

❌ **错误 - 多个重载方法**
```csharp
public User? QueryUser(int userId) { ... }
public User? QueryUser(string username) { ... }
```

### 6. Insert 方法处理条件性更新

✅ **正确 - 在 Insert 方法内判断**
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
    // ... 其他字段
    
    // 判断密码是否为空，有就指定
    if (!string.IsNullOrEmpty(user.password_hash) && !string.IsNullOrEmpty(user.salt))
    {
        existingEntity.password_hash = user.password_hash;
        existingEntity.salt = user.salt;
        existingEntity.last_password_change_at = DateTime.Now;
    }
    
    // 判断是否更新登录信息
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

❌ **错误 - 多个专门的更新方法**
```csharp
public User InsertUser(User user) { ... }
public User UpdatePassword(int userId, string password) { ... }
public User UpdateLoginInfo(int userId, string ip) { ... }
```

---

## 示例

### 完整示例：AuthRepository

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
        // ... 更新所有字段
        
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

### 完整示例：AuthService

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
            return (false, "请输入账号和密码", null);

        var user = ValidateUserCredentials(username, password);
        if (user == null)
            return (false, "账号或密码错误", null);

        return (true, null, user);
    }

    private bool VerifyPassword(string password, string? hash, string? salt)
    {
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt)) return false;
        // 密码验证逻辑
        return true;
    }

    #endregion
}
```

---

## 快速检查清单

在编写代码时，请检查：

### Repository 层
- [ ] 使用了几张表？
- [ ] 每张表都有 `Query*List` 和 `Query*` 方法吗？
- [ ] 每张表只有一个 `Insert*` 方法吗？
- [ ] 没有使用 `Get*` 命名吗？（应使用 `Query*`）
- [ ] 没有使用 `async`/`await` 吗？
- [ ] 没有使用 `try...catch` 吗？
- [ ] 使用 `=>` 表达式了吗？
- [ ] 有统一的查询来源（`IQueryable`）吗？

### Service 层
- [ ] 每个 `Query*` 都有对应的 `Load*` 吗？
- [ ] 每个 `Insert*` 都有对应的 `Create*` 吗？
- [ ] 没有使用 `Get*` 命名吗？（应使用 `Load*`）
- [ ] 没有使用 `async`/`await` 吗？
- [ ] 没有使用 `try...catch` 吗？
- [ ] 使用 `=>` 表达式了吗？
- [ ] 业务逻辑放在辅助方法中了吗？

---

## 常见错误

### ❌ 错误 1：使用 Get 命名
```csharp
// Repository 层
public User? GetUser(int id) { }
public List<User> GetUserList() { }
public List<string> GetOccupiedRoleIds(int orgId) { }

// Service 层
public User? GetUser(int id) { }
public List<User> GetUsers() { }
```
**正确做法：** Repository 使用 `Query*`，Service 使用 `Load*`
```csharp
// Repository 层
public User? QueryUser(int id) { }
public List<User> QueryUserList() { }
public List<string> QueryOccupiedRoleIdList(int orgId) { }

// Service 层
public User? LoadUser(int id) { }
public List<User> LoadUserList() { }
```

### ❌ 错误 2：一张表多个 Insert
```csharp
public permission_management InsertUser(permission_management user) { }
public permission_management InsertRole(permission_management role) { }
public permission_management InsertOrganization(permission_management org) { }
```
**正确做法：** 合并为一个 `InsertPermissionManagement`，在方法内根据 `type` 字段处理不同逻辑。

### ❌ 错误 3：缺少对应的 Query 方法
```csharp
// 使用了 permission_management 表，但只有
public permission_management? QueryUserByName(string name) { }
public permission_management? QueryRoleById(int id) { }
// 缺少 QueryPermissionManagementList 和 QueryPermissionManagement
```
**正确做法：** 为每张表添加完整的 `Query*List` 和 `Query*` 方法。

### ❌ 错误 4：Repository 包含业务逻辑
```csharp
public List<string> QueryUserNavigationPermissions(string userName)
{
    var permissions = new List<string>();
    permissions.AddRange(new[] { "dashboard", "profile" });
    
    var user = db.users.Where(u => u.name == userName).FirstOrDefault();
    // ... 复杂的权限计算逻辑
    
    return permissions.Distinct().ToList();
}
```
**正确做法：** Repository 只做简单查询，业务逻辑放在 Service 层。

---

## 版本历史

- **v1.1** (2025-01-09) - 添加禁止 Get 命名规则
  - 新增核心原则：禁止使用 `Get*` 命名
  - Repository 层必须使用 `Query*` 命名
  - Service 层必须使用 `Load*` 命名
  - 添加相关错误示例和检查清单

- **v1.0** (2025-01-09) - 初始版本
  - 建立基础架构规范
  - 定义命名规则
  - 确立核心原则

---

## 参考资料

- 项目结构：`HNB/Areas/Backoffice/`
- 示例 Repository：`AuthRepository.cs`, `PermissionManagementRepository.cs`
- 示例 Service：`AuthService.cs`, `PermissionManagementService.cs`

---

**注意：** 所有开发者在编写新代码或修改现有代码时，必须遵循本规范。如有疑问，请参考现有的 `AuthRepository` 和 `AuthService` 作为标准示例。

