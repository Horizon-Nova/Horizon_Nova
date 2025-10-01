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
        => ValidUsers.Where(u => u.is_active == true)
                     .OrderBy(u => u.full_name)
                     .ToList();

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
        => ValidUsers.FirstOrDefault(u => u.id == id);

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
        db.permission_managements.Update(entity);
        await db.SaveChangesAsync();
        return entity;
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
    #endregion
}