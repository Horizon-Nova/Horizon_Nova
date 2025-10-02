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
    private IQueryable<vw_permission_user> ValidUsers => db.vw_permission_users.Where(u => u.type == "user");
    
    /// <summary>
    /// 有效的角色查詢來源
    /// </summary>
    private IQueryable<vw_permission_role> ValidRoles => db.vw_permission_roles;
    
    /// <summary>
    /// 有效的組織查詢來源
    /// </summary>
    private IQueryable<vw_permission_organization> ValidOrganizations => db.vw_permission_organizations;
    
    /// <summary>
    /// 有效的權限管理查詢來源
    /// </summary>
    private IQueryable<permission_management> ValidPermissionManagements => db.permission_managements;
    #endregion

    #region 人員管理
    /// <summary>
    /// 載入所有啟用的用戶
    /// </summary>
    public List<vw_permission_user> LoadUsers()
    {
        var users = ValidUsers.Where(u => u.is_active == true)
                             .OrderBy(u => u.full_name)
                             .ToList();
        
        // 手動填充組織名稱和角色名稱
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
                    
                    // 填充角色名稱
                    if (userEntity.roles != null && userEntity.roles.Any() && string.IsNullOrEmpty(user.role_name))
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
                }
            }
        }
        
        return users;
    }

    /// <summary>
    /// 根據ID載入用戶
    /// </summary>
    public vw_permission_user? LoadUserById(int id)
        => ValidUsers.FirstOrDefault(u => u.id == id);

    /// <summary>
    /// 搜尋用戶
    /// </summary>
    public List<vw_permission_user> SearchUsers(string? searchTerm, string? organization, string? role, bool? isActive)
    {
        var query = ValidUsers.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => 
                (u.full_name != null && u.full_name.Contains(searchTerm)) ||
                (u.username != null && u.username.Contains(searchTerm)) ||
                (u.email != null && u.email.Contains(searchTerm)));
        }

        if (!string.IsNullOrEmpty(organization))
            query = query.Where(u => u.organization_name == organization);

        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.role_name == role);

        if (isActive.HasValue)
            query = query.Where(u => u.is_active == isActive.Value);

        return query.OrderBy(u => u.full_name).ToList();
    }

    /// <summary>
    /// 載入用戶詳細資料
    /// </summary>
    public vw_permission_user? LoadUserDetails(int id)
    {
        var user = ValidUsers.FirstOrDefault(u => u.id == id);
        if (user != null)
        {
            // 手動填充組織名稱和角色名稱（因為視圖可能沒有正確關聯）
            var userEntity = ValidPermissionManagements.FirstOrDefault(pm => pm.id == id && pm.type == "user");
            if (userEntity != null)
            {
                // 填充組織名稱
                if (userEntity.parent_id.HasValue)
                {
                    var org = ValidPermissionManagements.FirstOrDefault(pm => pm.id == userEntity.parent_id.Value && pm.type == "organization");
                    if (org != null && string.IsNullOrEmpty(user.organization_name))
                    {
                        user.organization_name = org.name;
                    }
                }
                
                // 填充角色名稱
                if (userEntity.roles != null && userEntity.roles.Any() && string.IsNullOrEmpty(user.role_name))
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
            }
        }
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
        => ValidRoles.Where(r => r.is_active == true)
                     .OrderBy(r => r.name)
                     .ToList();

    /// <summary>
    /// 根據ID載入角色
    /// </summary>
    public vw_permission_role? LoadRoleById(int id)
        => ValidRoles.FirstOrDefault(r => r.id == id);

    /// <summary>
    /// 搜尋角色
    /// </summary>
    public List<vw_permission_role> SearchRoles(string? searchTerm, string? organization, bool? isActive)
    {
        var query = ValidRoles.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(r => 
                (r.name != null && r.name.Contains(searchTerm)) ||
                (r.description != null && r.description.Contains(searchTerm)));
        }

        if (!string.IsNullOrEmpty(organization))
            query = query.Where(r => r.organization_name == organization);

        if (isActive.HasValue)
            query = query.Where(r => r.is_active == isActive.Value);

        return query.OrderBy(r => r.name).ToList();
    }

    /// <summary>
    /// 載入角色詳細資料
    /// </summary>
    public vw_permission_role? LoadRoleDetails(int id)
        => ValidRoles.FirstOrDefault(r => r.id == id);

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
            .OrderBy(r => r.name)
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
        => ValidOrganizations.Where(o => o.is_active == true)
                             .OrderBy(o => o.name)
                             .ToList();

    /// <summary>
    /// 根據ID載入組織
    /// </summary>
    public vw_permission_organization? LoadOrganizationById(int id)
        => ValidOrganizations.FirstOrDefault(o => o.id == id);

    /// <summary>
    /// 搜尋組織
    /// </summary>
    public List<vw_permission_organization> SearchOrganizations(string? searchTerm, int? level, bool? isActive)
    {
        var query = ValidOrganizations.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(o => 
                (o.name != null && o.name.Contains(searchTerm)) ||
                (o.description != null && o.description.Contains(searchTerm)));
        }

        if (level.HasValue)
            query = query.Where(o => o.level == level.Value);

        if (isActive.HasValue)
            query = query.Where(o => o.is_active == isActive.Value);

        return query.OrderBy(o => o.name).ToList();
    }

    /// <summary>
    /// 載入組織詳細資料
    /// </summary>
    public vw_permission_organization? LoadOrganizationDetails(int id)
        => ValidOrganizations.FirstOrDefault(o => o.id == id);
    #endregion

    #region 基本 CRUD 操作
    /// <summary>
    /// 根據ID載入權限管理資料
    /// </summary>
    public permission_management? LoadPermissionManagementById(int id)
        => ValidPermissionManagements.FirstOrDefault(pm => pm.id == id);

    /// <summary>
    /// 根據ID載入權限管理資料 (Async)
    /// </summary>
    public async Task<permission_management?> GetPermissionManagementByIdAsync(int id)
        => await ValidPermissionManagements.FirstOrDefaultAsync(pm => pm.id == id);

    /// <summary>
    /// 根據ID載入權限管理資料 (同步版本)
    /// </summary>
    public permission_management? GetPermissionManagementById(int id)
        => ValidPermissionManagements.FirstOrDefault(pm => pm.id == id);

    /// <summary>
    /// 建立權限管理資料
    /// </summary>
    public async Task<permission_management> CreatePermissionManagementAsync(permission_management entity)
    {
        db.permission_managements.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// 更新權限管理資料
    /// </summary>
    public async Task<permission_management> UpdatePermissionManagementAsync(permission_management entity)
    {
        // EF Core 8 需要先查詢實體再更新
        var existingEntity = await db.permission_managements.FindAsync(entity.id);
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
        
        // 只有在密碼有變更時才更新
        if (!string.IsNullOrEmpty(entity.password_hash))
        {
            existingEntity.password_hash = entity.password_hash;
            existingEntity.salt = entity.salt;
        }

        existingEntity.updated_at = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return existingEntity;
    }

    /// <summary>
    /// 建立權限管理資料 (同步版本)
    /// </summary>
    public permission_management CreatePermissionManagement(permission_management entity)
    {
        db.permission_managements.Add(entity);
        db.SaveChanges();
        return entity;
    }

    /// <summary>
    /// 更新權限管理資料 (同步版本)
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
    public async Task<bool> DeletePermissionManagementAsync(int id)
    {
        var entity = await db.permission_managements.FindAsync(id);
        if (entity != null)
        {
            db.permission_managements.Remove(entity);
            await db.SaveChangesAsync();
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