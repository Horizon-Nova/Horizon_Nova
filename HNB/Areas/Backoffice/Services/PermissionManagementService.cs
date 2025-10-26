using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;
using HNB.Areas.Backoffice.Core;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HNB.Areas.Backoffice.Services;

public class PermissionManagementService(HnbHnbBackofficeDbContext db, PermissionManagementRepository repo, SidebarNavigationService sidebarService)
{
    #region 私有輔助方法

    private static List<vw_permission_user> FilterUsers(IEnumerable<vw_permission_user> source, string? searchTerm, string? organization, string? role, bool? isActive)
        => source
            .Where(u => string.IsNullOrWhiteSpace(searchTerm)
                || (u.full_name?.Contains(searchTerm) ?? false)
                || (u.name?.Contains(searchTerm) ?? false)
                || (u.email?.Contains(searchTerm) ?? false))
            .Where(u => string.IsNullOrWhiteSpace(organization) || u.organization_name == organization)
            .Where(u => string.IsNullOrWhiteSpace(role) || u.role_name == role)
            .Where(u => !isActive.HasValue || u.is_active == isActive.Value)
            .ToList();

    private List<vw_permission_user> ResolveUsersScoped(int? organizationId, string? searchTerm, string? organization, string? role, bool? isActive)
        => OrganizationScope.ResolveUsers(
            db,
            organizationId.HasValue ? new permission_management { parent_id = new List<string> { organizationId.Value.ToString() } } : null,
            null,
            searchTerm,
            organization,
            role,
            isActive,
            includeAllWhenNoScope: !organizationId.HasValue
        );

    private static List<vw_permission_role> FilterRoles(IEnumerable<vw_permission_role> source, string? searchTerm, string? organization, bool? isActive)
        => source
            .Where(r => string.IsNullOrWhiteSpace(searchTerm)
                || (r.name?.Contains(searchTerm) ?? false)
                || (r.description?.Contains(searchTerm) ?? false))
            .Where(r => string.IsNullOrWhiteSpace(organization) || r.organization_name == organization)
            .Where(r => !isActive.HasValue || r.is_active == isActive.Value)
            .ToList();

    private List<vw_permission_role> ResolveRolesScoped(int? organizationId, string? searchTerm, string? organization, bool? isActive)
        => OrganizationScope.ResolveRoles(
            db,
            organizationId.HasValue ? new permission_management { parent_id = new List<string> { organizationId.Value.ToString() } } : null,
            null,
            searchTerm,
            organization,
            isActive,
            includeAllWhenNoScope: !organizationId.HasValue
        );

    private static List<vw_permission_organization> FilterOrganizations(IEnumerable<vw_permission_organization> source, string? searchTerm, int? level, bool? isActive)
        => source
            .Where(o => string.IsNullOrWhiteSpace(searchTerm)
                || (o.organization_name?.Contains(searchTerm) ?? false)
                || (o.organization_description?.Contains(searchTerm) ?? false))
            .Where(o => !level.HasValue || o.organization_level == level.Value)
            .Where(o => !isActive.HasValue || o.is_active == isActive.Value)
            .OrderBy(o => o.organization_name)
            .ToList();

    private List<vw_permission_organization> ResolveOrganizationsScoped(int? organizationId, string? searchTerm, int? level, bool? isActive)
        => OrganizationScope.ResolveOrganizations(
            db,
            organizationId.HasValue ? new permission_management { parent_id = new List<string> { organizationId.Value.ToString() } } : null,
            null,
            searchTerm,
            level,
            isActive,
            includeAllWhenNoScope: !organizationId.HasValue
        );

    #endregion

    #region 統一的查詢方法

    /// <summary>
    /// 載入用戶列表
    /// </summary>
    public List<vw_permission_user> LoadUserList(string? searchTerm = null, string? organization = null, string? role = null, bool? isActive = null, int? organizationId = null)
        => ResolveUsersScoped(organizationId, searchTerm, organization, role, isActive);

    /// <summary>
    /// 載入用戶
    /// </summary>
    public vw_permission_user? LoadUser(int id)
        => OrganizationScope.ResolveUsers(db, new permission_management { type = "user", id = id }).FirstOrDefault();

    /// <summary>
    /// 載入角色列表
    /// </summary>
    public List<vw_permission_role> LoadRoleList(string? searchTerm = null, string? organization = null, bool? isActive = null, int? organizationId = null)
        => ResolveRolesScoped(organizationId, searchTerm, organization, isActive);

    /// <summary>
    /// 載入角色
    /// </summary>
    public vw_permission_role? LoadRole(int id)
        => OrganizationScope.ResolveRole(db, id);

    /// <summary>
    /// 載入組織列表
    /// </summary>
    public List<vw_permission_organization> LoadOrganizationList(string? searchTerm = null, int? level = null, bool? isActive = null, int? organizationId = null)
        => ResolveOrganizationsScoped(organizationId, searchTerm, level, isActive);

    /// <summary>
    /// 載入組織
    /// </summary>
    public vw_permission_organization? LoadOrganization(int id)
        => OrganizationScope.ResolveOrganization(db, id);

    /// <summary>
    /// 載入權限管理列表
    /// </summary>
    public List<permission_management> LoadPermissionManagementList(string? type = null, bool? isActive = null)
        => repo.QueryPermissionManagementList(type, isActive);

    /// <summary>
    /// 載入權限管理
    /// </summary>
    public permission_management? LoadPermissionManagement(int? id = null, string? name = null, string? type = null)
        => repo.QueryPermissionManagement(id, name, type);

    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 創建權限管理資料（新增或更新）
    /// </summary>
    public (bool success, string? errorMessage, permission_management? result) CreatePermissionManagement(permission_management data)
    {
        if (data.type == "organization" && data.roles != null && data.roles.Any())
        {
            var occupiedRoles = repo.QueryOccupiedRoles(data.roles, data.id);
            if (occupiedRoles.Any())
            {
                var errorMessages = occupiedRoles.Select(kvp => 
                {
                    var roleName = OrganizationScope.ResolveRole(db, int.Parse(kvp.Key))?.name ?? kvp.Key;
                    return $"「{roleName}」已被「{kvp.Value}」組織使用";
                });
                return (false, $"角色分配失敗：{string.Join("、", errorMessages)}", null);
            }
        }
        
        var result = repo.InsertPermissionManagement(data);
        return (result != null, result != null ? null : "創建失敗", result);
    }

    /// <summary>
    /// 創建用戶
    /// </summary>
    public (bool success, string message) CreateUser(permission_management data)
    {
        data.type = "user";
        var (success, errorMessage, result) = CreatePermissionManagement(data);
        return (success, errorMessage ?? (success ? "創建成功" : "創建失敗"));
    }

    /// <summary>
    /// 創建角色
    /// </summary>
    public (bool success, string message) CreateRole(permission_management data)
    {
        data.type = "role";
        var (success, errorMessage, result) = CreatePermissionManagement(data);
        return (success, errorMessage ?? (success ? "創建成功" : "創建失敗"));
    }

    /// <summary>
    /// 創建組織
    /// </summary>
    public (bool success, string message) CreateOrganization(permission_management data)
    {
        data.type = "organization";
        var (success, errorMessage, result) = CreatePermissionManagement(data);
        return (success, errorMessage ?? (success ? "創建成功" : "創建失敗"));
    }

    /// <summary>
    /// 刪除權限管理資料
    /// </summary>
    public bool Delete(int id)
        => repo.DeletePermissionManagement(id);

    /// <summary>
    /// 刪除權限管理資料
    /// </summary>
    public bool DeletePermissionManagement(int id)
        => repo.DeletePermissionManagement(id);

    #endregion

    #region 權限控制輔助方法

    /// <summary>
    /// 根據用戶名獲取用戶的組織ID
    /// </summary>
    /// <param name="username">用戶名</param>
    /// <returns>組織ID，如果找不到則返回null</returns>
    public int? LoadUserOrganizationId(string username)
    {
        var user = OrganizationScope.ResolveUsers(db, new permission_management { type = "user", name = username }).FirstOrDefault();
        return user?.organization_id;
    }

    

    /// <summary>
    /// 根據用戶名獲取用戶資訊
    /// </summary>
    /// <param name="username">用戶名</param>
    /// <returns>用戶資訊或null</returns>
    public vw_permission_user? LoadUserByName(string username)
        => OrganizationScope.ResolveUsers(db, new permission_management { type = "user", name = username }).FirstOrDefault();

    #endregion

    #region ViewBag 設定方法

    /// <summary>
    /// 設定權限管理統一的 ViewBag 資料（帶權限控制）
    /// </summary>
    /// <param name="viewBag">ViewBag 物件</param>
    /// <param name="id">項目ID</param>
    /// <param name="currentUserOrganizationId">當前用戶的組織ID（用於權限控制）</param>
    public void ViewBagModel(dynamic viewBag, int? id = null, int? currentUserOrganizationId = null)
    {
        viewBag.Id = id;
        viewBag.Organizations = LoadOrganizationList(organizationId: currentUserOrganizationId);
        viewBag.Organization = id.HasValue ? LoadOrganization(id.Value) : null;

        viewBag.Roles = LoadRoleList(organizationId: currentUserOrganizationId);
        viewBag.Role = id.HasValue ? LoadRole(id.Value) : null;

        viewBag.Users = LoadUserList(organizationId: currentUserOrganizationId);
        viewBag.User = id.HasValue ? LoadUser(id.Value) : null;

        viewBag.Navigations = sidebarService.LoadNavigations();
        
        if (id.HasValue)
        {
            var roleData = LoadPermissionManagement(id: id.Value, type: "role");
            viewBag.RoleNavigationPermissions = roleData?.navigation_permissions ?? new List<string>();
            
            var organizationData = LoadPermissionManagement(id: id.Value, type: "organization");
            viewBag.OrganizationRoles = organizationData?.roles ?? new List<string>();
            
            viewBag.OccupiedRoleIds = repo.QueryOccupiedRoleIdList(id.Value);
        }
        else
        {
            viewBag.RoleNavigationPermissions = new List<string>();
            viewBag.OrganizationRoles = new List<string>();
            
            viewBag.OccupiedRoleIds = repo.QueryOccupiedRoleIdList(0);
        }
    }

    #endregion

}