using Microsoft.EntityFrameworkCore;
using Models.HnbBackoffice;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 權限管理資料存取層，負責處理權限管理資料的存取功能
/// </summary>
public class PermissionManagementRepository(HnbBackofficeDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 權限管理主表
    /// </summary>
    private IQueryable<permission_management> ValidPermissionManagements => db.permission_managements;

    /// <summary>
    /// 使用者視圖查詢來源
    /// </summary>
    private IQueryable<vw_permission_user> ValidUsers => db.vw_permission_users.Where(u => u.type == "user");

    /// <summary>
    /// 角色視圖查詢來源
    /// </summary>
    private IQueryable<vw_permission_role> ValidRoles => db.vw_permission_roles;

    /// <summary>
    /// 組織視圖查詢來源
    /// </summary>
    private IQueryable<vw_permission_organization> ValidOrganizations => db.vw_permission_organizations;

    /// <summary>
    /// 導航項目查詢來源
    /// </summary>
    private IQueryable<vw_sidebar_navigation> ValidNavigations => db.vw_sidebar_navigations.OrderBy(n => n.sort_order);

    #endregion

    #region 專用查詢方法

    /// <summary>
    /// 查詢權限管理列表
    /// </summary>
    /// <param name="type">類型篩選 (user/role/organization)</param>
    /// <param name="isActive">啟用狀態篩選</param>
    public List<permission_management> QueryPermissionManagementList(string? type = null, bool? isActive = null)
        => ValidPermissionManagements
            .Where(pm =>
                (string.IsNullOrEmpty(type) || pm.type == type) &&
                (!isActive.HasValue || pm.is_active == isActive.Value)
            )
            .ToList();

    /// <summary>
    /// 查詢權限管理
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="name">名稱</param>
    /// <param name="type">類型</param>
    public permission_management? QueryPermissionManagement(int? id = null, string? name = null, string? type = null)
    {
        if (id.HasValue)
            return ValidPermissionManagements.FirstOrDefault(pm => pm.id == id.Value);

        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type))
            return ValidPermissionManagements.FirstOrDefault(pm => pm.name == name && pm.type == type);

        return null;
    }

    /// <summary>
    /// 查詢使用者列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="organization">組織名稱篩選</param>
    /// <param name="role">角色名稱篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <param name="organizationIds">組織 ID 列表篩選（用於組織範圍過濾）</param>
    public List<vw_permission_user> QueryUserList(string? searchTerm = null, string? organization = null, string? role = null, bool? isActive = null, List<int>? organizationIds = null)
        => ValidUsers
            .Where(u =>
                (string.IsNullOrEmpty(searchTerm) ||
                    (u.full_name != null && u.full_name.Contains(searchTerm)) ||
                    (u.name != null && u.name.Contains(searchTerm)) ||
                    (u.email != null && u.email.Contains(searchTerm))) &&
                (string.IsNullOrEmpty(organization) || u.organization_name == organization) &&
                (string.IsNullOrEmpty(role) || u.role_name == role) &&
                (!isActive.HasValue || u.is_active == isActive.Value) &&
                (organizationIds == null || !organizationIds.Any() || (u.organization_id.HasValue && organizationIds.Contains(u.organization_id.Value)))
            )
            .ToList();

    /// <summary>
    /// 查詢使用者
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <param name="name">使用者名稱</param>
    public vw_permission_user? QueryUser(int? id = null, string? name = null)
    {
        if (id.HasValue)
            return ValidUsers.FirstOrDefault(u => u.id == id.Value);

        if (!string.IsNullOrEmpty(name))
            return ValidUsers.FirstOrDefault(u => u.name == name);

        return null;
    }

    /// <summary>
    /// 查詢角色列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="organization">組織名稱篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <param name="organizationIds">組織 ID 列表篩選（用於組織範圍過濾）</param>
    public List<vw_permission_role> QueryRoleList(string? searchTerm = null, string? organization = null, bool? isActive = null, List<int>? organizationIds = null)
        => ValidRoles
            .Where(r =>
                (string.IsNullOrEmpty(searchTerm) ||
                    (r.name != null && r.name.Contains(searchTerm)) ||
                    (r.description != null && r.description.Contains(searchTerm))) &&
                (string.IsNullOrEmpty(organization) || r.organization_name == organization) &&
                (!isActive.HasValue || r.is_active == isActive.Value) &&
                (organizationIds == null || !organizationIds.Any() || (r.organization_id.HasValue && organizationIds.Contains(r.organization_id.Value)))
            )
            .ToList();

    /// <summary>
    /// 查詢角色
    /// </summary>
    /// <param name="id">角色 ID</param>
    public vw_permission_role? QueryRole(int? id = null)
        => id.HasValue ? ValidRoles.FirstOrDefault(r => r.id == id) : null;

    /// <summary>
    /// 查詢組織列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="level">組織層級篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <param name="organizationIds">組織 ID 列表篩選（用於組織範圍過濾）</param>
    public List<vw_permission_organization> QueryOrganizationList(string? searchTerm = null, int? level = null, bool? isActive = null, List<int>? organizationIds = null)
        => ValidOrganizations
            .Where(o =>
                (string.IsNullOrEmpty(searchTerm) ||
                    (o.organization_name != null && o.organization_name.Contains(searchTerm)) ||
                    (o.organization_description != null && o.organization_description.Contains(searchTerm))) &&
                (!level.HasValue || o.organization_level == level.Value) &&
                (!isActive.HasValue || o.is_active == isActive.Value) &&
                (organizationIds == null || !organizationIds.Any() || (o.id.HasValue && organizationIds.Contains(o.id.Value)))
            )
            .OrderBy(o => o.organization_name)
            .ToList();

    /// <summary>
    /// 查詢組織
    /// </summary>
    /// <param name="id">組織 ID</param>
    public vw_permission_organization? QueryOrganization(int? id = null)
        => id.HasValue ? ValidOrganizations.FirstOrDefault(o => o.id == id) : null;

    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 插入權限管理資料（新增或更新）
    /// </summary>
    public permission_management InsertPermissionManagement(permission_management data)
    {
        var existingEntity = db.permission_managements.Find(data.id);

        if (existingEntity == null)
        {
            data.created_at = DateTime.Now;
            data.updated_at = null;
            db.permission_managements.Add(data);
            db.SaveChanges();
            return data;
        }

        existingEntity.type = data.type;
        existingEntity.name = data.name;
        existingEntity.description = data.description;
        existingEntity.email = data.email;
        existingEntity.phone = data.phone;
        existingEntity.gender = data.gender;
        existingEntity.full_name = data.full_name;
        existingEntity.is_active = data.is_active;
        existingEntity.nickname = data.nickname;
        existingEntity.zodiac_sign = data.zodiac_sign;
        existingEntity.favorite_color = data.favorite_color;
        existingEntity.location = data.location;
        existingEntity.bio = data.bio;
        existingEntity.level = data.level;
        existingEntity.parent_id = data.parent_id;
        existingEntity.roles = data.roles;
        existingEntity.navigation_permissions = data.navigation_permissions;

        if (!string.IsNullOrEmpty(data.password_hash) && !string.IsNullOrEmpty(data.salt))
        {
            existingEntity.password_hash = data.password_hash;
            existingEntity.salt = data.salt;
            existingEntity.last_password_change_at = DateTime.Now;
        }

        existingEntity.updated_at = DateTime.Now;
        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 刪除權限管理資料
    /// </summary>
    public bool DeletePermissionManagement(int id)
    {
        var entity = ValidPermissionManagements.FirstOrDefault(pm => pm.id == id);
        if (entity == null) return false;
        db.permission_managements.Remove(entity);
        db.SaveChanges();
        return true;
    }

    #endregion

    #region 特殊查詢

    /// <summary>
    /// 查詢導航項目列表（返回所有啟用的導覽項目）
    /// </summary>
    public List<vw_sidebar_navigation> QueryNavigationList()
        => ValidNavigations.Where(na => na.is_active == true).ToList();

    #endregion

    #region 特殊查詢*未來須修正*

    /// <summary>
    /// 查詢被占用的角色（檢查角色是否已被其他組織使用）
    /// </summary>
    /// <param name="roleIds">要檢查的角色ID列表</param>
    /// <param name="currentOrganizationId">當前組織ID（編輯時排除自己）</param>
    /// <returns>已被占用的角色ID和組織名稱的字典</returns>
    public Dictionary<string, string> QueryOccupiedRoles(List<string> roleIds, int currentOrganizationId = 0)
    {
        var occupiedRoles = new Dictionary<string, string>();

        var otherOrganizations = ValidPermissionManagements
            .Where(pm => pm.type == "organization" && pm.id != currentOrganizationId && pm.roles != null)
            .ToList();

        foreach (var roleId in roleIds)
        {
            var occupyingOrg = otherOrganizations.FirstOrDefault(org => org.roles != null && org.roles.Contains(roleId));
            if (occupyingOrg != null)
            {
                occupiedRoles[roleId] = occupyingOrg.name ?? "未知組織";
            }
        }

        return occupiedRoles;
    }

    /// <summary>
    /// 查詢被占用的角色ID列表
    /// </summary>
    /// <param name="currentOrganizationId">當前組織ID（編輯時排除自己）</param>
    /// <returns>已被占用的角色ID列表</returns>
    public List<string> QueryOccupiedRoleIdList(int currentOrganizationId = 0)
        => ValidPermissionManagements
            .Where(pm => pm.type == "organization" && pm.id != currentOrganizationId && pm.roles != null)
            .AsEnumerable()
            .SelectMany(pm => pm.roles!)
            .Distinct()
            .ToList();

    #endregion

}