using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 權限管理資料存取層，負責處理用戶、角色、組織的資料存取功能
/// </summary>
public class PermissionManagementRepository(HnbHnbBackofficeDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的用戶查詢來源
    /// </summary>
    private IQueryable<vw_permission_user> ValidUsers => db.vw_permission_users.Where(u => u.type == "user").OrderBy(u => u.full_name);
    
    /// <summary>
    /// 有效的角色查詢來源
    /// </summary>
    private IQueryable<vw_permission_role> ValidRoles => db.vw_permission_roles.OrderBy(r => r.name);
    
    /// <summary>
    /// 有效的組織查詢來源
    /// </summary>
    private IQueryable<vw_permission_organization> ValidOrganizations => db.vw_permission_organizations.OrderBy(o => o.name);
    
    /// <summary>
    /// 有效的權限管理查詢來源
    /// </summary>
    private IQueryable<permission_management> ValidPermissionManagements => db.permission_managements;
    #endregion

    #region 通用查詢方法
    /// <summary>
    /// 通用查詢方法 - 根據條件查詢單一實體
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    /// <param name="query">查詢來源</param>
    /// <param name="id">ID</param>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="organization">組織篩選</param>
    /// <param name="role">角色篩選</param>
    /// <param name="level">層級篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <returns>單一實體或null</returns>
    private T? QueryEntity<T>(
        IQueryable<T> query,
        int? id = null,
        string? searchTerm = null,
        string? organization = null,
        string? role = null,
        int? level = null,
        bool? isActive = null) where T : class
    {
        if (!id.HasValue) return null;

        return typeof(T) switch
        {
            Type t when t == typeof(vw_permission_user) => 
                query.Cast<vw_permission_user>().FirstOrDefault(u => u.id == id) as T,
            Type t when t == typeof(vw_permission_role) => 
                query.Cast<vw_permission_role>().FirstOrDefault(r => r.id == id) as T,
            Type t when t == typeof(vw_permission_organization) => 
                query.Cast<vw_permission_organization>().FirstOrDefault(o => o.id == id) as T,
            _ => null
        };
    }

    /// <summary>
    /// 通用查詢方法 - 根據條件查詢實體列表
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    /// <param name="query">查詢來源</param>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="organization">組織篩選</param>
    /// <param name="role">角色篩選</param>
    /// <param name="level">層級篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <param name="orderBy">排序欄位</param>
    /// <returns>實體列表</returns>
    private List<T> QueryEntityList<T>(
        IQueryable<T> query,
        string? searchTerm = null,
        string? organization = null,
        string? role = null,
        int? level = null,
        bool? isActive = null,
        string orderBy = "name") where T : class
    {
        // 根據類型動態應用篩選條件
        if (typeof(T) == typeof(vw_permission_user))
        {
            return query.Cast<vw_permission_user>()
                .Where(u =>
                    (string.IsNullOrEmpty(searchTerm) || 
                        (u.full_name != null && u.full_name.Contains(searchTerm)) ||
                        (u.username != null && u.username.Contains(searchTerm)) ||
                        (u.email != null && u.email.Contains(searchTerm))) &&
                    (string.IsNullOrEmpty(organization) || u.organization_name == organization) &&
                    (string.IsNullOrEmpty(role) || u.role_name == role) &&
                    (!isActive.HasValue || u.is_active == isActive.Value)
                )
                .Cast<T>()
                .ToList();
        }
        else if (typeof(T) == typeof(vw_permission_role))
        {
            return query.Cast<vw_permission_role>()
                .Where(r =>
                    (string.IsNullOrEmpty(searchTerm) || 
                        (r.name != null && r.name.Contains(searchTerm)) ||
                        (r.description != null && r.description.Contains(searchTerm))) &&
                    (string.IsNullOrEmpty(organization) || r.organization_name == organization) &&
                    (!isActive.HasValue || r.is_active == isActive.Value)
                )
                .Cast<T>()
                .ToList();
        }
        else if (typeof(T) == typeof(vw_permission_organization))
        {
            return query.Cast<vw_permission_organization>()
                .Where(o =>
                    (string.IsNullOrEmpty(searchTerm) || 
                        (o.name != null && o.name.Contains(searchTerm)) ||
                        (o.description != null && o.description.Contains(searchTerm))) &&
                    (!level.HasValue || o.level == level.Value) &&
                    (!isActive.HasValue || o.is_active == isActive.Value)
                )
                .Cast<T>()
                .ToList();
        }

        return query.ToList();
    }

    /// <summary>
    /// 填充用戶的組織和角色資訊
    /// </summary>
    /// <param name="users">用戶列表</param>
    private void PopulateUserRelations(List<vw_permission_user> users)
    {
        foreach (var user in users)
        {
            if (user.id.HasValue)
            {
                var userEntity = ValidPermissionManagements.FirstOrDefault(pm => pm.id == user.id.Value && pm.type == "user");
                if (userEntity != null)
                {
                    // 填充組織名稱
                    if (userEntity.parent_id.HasValue && string.IsNullOrEmpty(user.organization_name))
                    {
                        var org = ValidPermissionManagements.FirstOrDefault(pm => pm.id == userEntity.parent_id.Value && pm.type == "organization");
                        if (org != null)
                        {
                            user.organization_name = org.name;
                        }
                    }
                    
                    // 填充角色名稱和角色列表
                    if (userEntity.roles != null && userEntity.roles.Any())
                    {
                        // 填充角色名稱
                        if (string.IsNullOrEmpty(user.role_name))
                        {
                            var roleIds = userEntity.roles.Where(r => int.TryParse(r, out _)).Select(r => int.Parse(r)).ToList();
                            if (roleIds.Any())
                            {
                                var roleNames = ValidPermissionManagements
                                    .Where(pm => roleIds.Contains(pm.id) && pm.type == "role")
                                    .Select(pm => pm.name)
                                    .ToList();
                                user.role_name = string.Join(", ", roleNames);
                            }
                        }
                        
                        // 填充角色ID列表（用於表單選擇）
                        user.roles = userEntity.roles;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 填充單一用戶的組織和角色資訊
    /// </summary>
    /// <param name="user">用戶</param>
    private void PopulateUserRelations(vw_permission_user? user)
    {
        if (user?.id == null) return;

        var userEntity = ValidPermissionManagements.FirstOrDefault(pm => pm.id == user.id.Value && pm.type == "user");
        if (userEntity != null)
        {
            // 手動填充 username 和 full_name（確保欄位對應正確）
            if (string.IsNullOrEmpty(user.username))
            {
                user.username = userEntity.name;
            }
            if (string.IsNullOrEmpty(user.full_name))
            {
                user.full_name = userEntity.full_name;
            }
            
            // 填充組織名稱
            if (userEntity.parent_id.HasValue)
            {
                var org = ValidPermissionManagements.FirstOrDefault(pm => pm.id == userEntity.parent_id.Value && pm.type == "organization");
                if (org != null && string.IsNullOrEmpty(user.organization_name))
                {
                    user.organization_name = org.name;
                }
            }
            
            // 填充角色名稱和角色列表
            if (userEntity.roles != null && userEntity.roles.Any())
            {
                // 填充角色名稱
                if (string.IsNullOrEmpty(user.role_name))
                {
                    var roleIds = userEntity.roles.Where(r => int.TryParse(r, out _)).Select(r => int.Parse(r)).ToList();
                    if (roleIds.Any())
                    {
                        var roleNames = ValidPermissionManagements
                            .Where(pm => roleIds.Contains(pm.id) && pm.type == "role")
                            .Select(pm => pm.name)
                            .ToList();
                        user.role_name = string.Join(", ", roleNames);
                    }
                }
                
                // 填充角色ID列表（用於表單選擇）
                user.roles = userEntity.roles;
            }
        }
    }
    #endregion

    #region 帳號管理
    /// <summary>
    /// 載入所有啟用的用戶
    /// </summary>
    public List<vw_permission_user> LoadUsers()
    {
        var users = QueryEntityList(ValidUsers, isActive: true);
        PopulateUserRelations(users);
        return users;
    }

    /// <summary>
    /// 根據ID載入用戶
    /// </summary>
    public vw_permission_user? LoadUserById(int id)
    {
        var user = QueryEntity(ValidUsers, id: id);
        PopulateUserRelations(user);
        return user;
    }

    /// <summary>
    /// 搜尋用戶
    /// </summary>
    public List<vw_permission_user> SearchUsers(string? searchTerm, string? organization, string? role, bool? isActive)
    {
        var users = QueryEntityList(ValidUsers, searchTerm, organization, role, isActive: isActive);
        PopulateUserRelations(users);
        return users;
    }

    /// <summary>
    /// 載入用戶詳細資料
    /// </summary>
    public vw_permission_user? LoadUserDetails(int id)
    {
        var user = QueryEntity(ValidUsers, id: id);
        PopulateUserRelations(user);
        return user;
    }

    /// <summary>
    /// 取得用戶組織ID
    /// </summary>
    public int? GetUserOrganizationId(int userId)
        => ValidPermissionManagements
            .FirstOrDefault(pm => pm.id == userId && pm.type == "user")?.parent_id;
    #endregion

    #region 角色管理
    /// <summary>
    /// 載入所有啟用的角色
    /// </summary>
    public List<vw_permission_role> LoadRoles()
        => QueryEntityList(ValidRoles, isActive: true);

    /// <summary>
    /// 根據ID載入角色
    /// </summary>
    public vw_permission_role? LoadRoleById(int id)
        => QueryEntity(ValidRoles, id: id);

    /// <summary>
    /// 搜尋角色
    /// </summary>
    public List<vw_permission_role> SearchRoles(string? searchTerm, string? organization, bool? isActive)
        => QueryEntityList(ValidRoles, searchTerm, organization, isActive: isActive);

    /// <summary>
    /// 載入角色詳細資料
    /// </summary>
    public vw_permission_role? LoadRoleDetails(int id)
        => QueryEntity(ValidRoles, id: id);

    /// <summary>
    /// 取得角色組織ID
    /// </summary>
    public int? GetRoleOrganizationId(int roleId)
        => ValidPermissionManagements
            .FirstOrDefault(pm => pm.id == roleId && pm.type == "role")?.parent_id;

    /// <summary>
    /// 取得可用角色（未分配給其他組織的角色）
    /// </summary>
    /// <param name="organizationId">組織ID（編輯時使用，0表示新增）</param>
    /// <returns>可用角色列表</returns>
    public List<vw_permission_role> GetAvailableRoles(int organizationId = 0)
    {
        // 取得所有已分配給組織的角色ID（排除當前編輯的組織）
        // 使用 AsNoTracking 避免實體追蹤衝突
        var assignedRoleIds = db.permission_managements
            .AsNoTracking()
            .Where(pm => pm.type == "organization" && pm.id != organizationId && pm.is_active == true)
            .SelectMany(pm => pm.roles ?? new List<string>())
            .Where(roleIdStr => !string.IsNullOrEmpty(roleIdStr) && roleIdStr.All(char.IsDigit))
            .Select(roleIdStr => int.Parse(roleIdStr))
            .ToHashSet();

        // 返回未分配的角色
        return ValidRoles
            .Where(r => r.is_active == true && !assignedRoleIds.Contains(r.id ?? 0))
            .ToList();
    }

    /// <summary>
    /// 取得角色的導航權限
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns>導航權限列表</returns>
    public List<string> GetRoleNavigationPermissions(int roleId)
    {
        var role = db.permission_managements
            .AsNoTracking()
            .FirstOrDefault(pm => pm.id == roleId && pm.type == "role");
            
        return role?.navigation_permissions ?? new List<string>();
    }
    #endregion

    #region 組織管理
    /// <summary>
    /// 載入所有啟用的組織
    /// </summary>
    public List<vw_permission_organization> LoadOrganizations()
        => QueryEntityList(ValidOrganizations, isActive: true);

    /// <summary>
    /// 根據ID載入組織
    /// </summary>
    public vw_permission_organization? LoadOrganizationById(int id)
        => QueryEntity(ValidOrganizations, id: id);

    /// <summary>
    /// 搜尋組織
    /// </summary>
    public List<vw_permission_organization> SearchOrganizations(string? searchTerm, int? level, bool? isActive)
        => QueryEntityList(ValidOrganizations, searchTerm, level: level, isActive: isActive);

    /// <summary>
    /// 載入組織詳細資料
    /// </summary>
    public vw_permission_organization? LoadOrganizationDetails(int id)
        => QueryEntity(ValidOrganizations, id: id);
    #endregion

    #region 基本 CRUD 操作
    /// <summary>
    /// 根據ID載入權限管理資料
    /// </summary>
    public permission_management? LoadPermissionManagementById(int id)
        => ValidPermissionManagements.FirstOrDefault(pm => pm.id == id);

    /// <summary>
    /// 根據ID載入權限管理資料
    /// </summary>
    public permission_management? GetPermissionManagementById(int id)
        => ValidPermissionManagements.FirstOrDefault(pm => pm.id == id);

    /// <summary>
    /// 建立權限管理資料
    /// </summary>
    public permission_management CreatePermissionManagement(permission_management entity)
    {
        db.permission_managements.Add(entity);
        db.SaveChanges();
        return entity;
    }

    /// <summary>
    /// 更新權限管理資料
    /// </summary>
    public permission_management UpdatePermissionManagement(permission_management entity)
    {
        // EF Core 8 需要先查詢實體再更新
        var existingEntity = db.permission_managements.Find(entity.id);
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"找不到 ID 為 {entity.id} 的權限管理資料");
        }

        // 更新屬性
        existingEntity.type = entity.type;
        existingEntity.name = entity.name;
        existingEntity.email = entity.email;
        existingEntity.phone = entity.phone;
        existingEntity.gender = entity.gender;
        existingEntity.full_name = entity.full_name;
        existingEntity.is_active = entity.is_active;
        existingEntity.nickname = entity.nickname;
        existingEntity.zodiac_sign = entity.zodiac_sign;
        existingEntity.favorite_color = entity.favorite_color;
        existingEntity.location = entity.location;
        existingEntity.bio = entity.bio;
        existingEntity.description = entity.description;
        existingEntity.level = entity.level;
        existingEntity.parent_id = entity.parent_id;
        existingEntity.roles = entity.roles;
        existingEntity.navigation_permissions = entity.navigation_permissions;
        
        if (!string.IsNullOrEmpty(entity.password_hash))
        {
            existingEntity.password_hash = entity.password_hash;
            existingEntity.salt = entity.salt;
        }

        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 刪除權限管理資料
    /// </summary>
    public bool DeletePermissionManagement(int id)
    {
        var entity = db.permission_managements.Find(id);
        if (entity != null)
        {
            db.permission_managements.Remove(entity);
            db.SaveChanges();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 根據類型載入權限管理資料
    /// </summary>
    public List<permission_management> LoadPermissionManagementsByType(string type)
        => ValidPermissionManagements.Where(pm => pm.type == type && pm.is_active == true)
                                     .OrderBy(pm => pm.name)
                                     .ToList();

    /// <summary>
    /// 更新角色的parent_id
    /// </summary>
    public void UpdateRoleParentId(int roleId, int organizationId)
    {
        // 使用 ExecuteUpdate 避免實體追蹤問題
        db.permission_managements
            .Where(pm => pm.id == roleId && pm.type == "role")
            .ExecuteUpdate(setters => setters.SetProperty(p => p.parent_id, organizationId));
    }

    /// <summary>
    /// 清除組織的所有角色關聯
    /// </summary>
    public void ClearRoleParentIdsByOrganization(int organizationId)
    {
        // 使用 ExecuteUpdate 避免實體追蹤問題
        db.permission_managements
            .Where(pm => pm.type == "role" && pm.parent_id == organizationId)
            .ExecuteUpdate(setters => setters.SetProperty(p => p.parent_id, (int?)null));
    }
    #endregion
}