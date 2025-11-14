using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

public class PermissionManagementService(PermissionManagementRepository repo, SidebarNavigationService sidebarService)
{
    #region 統一的查詢方法

    /// <summary>
    /// 載入用戶列表
    /// </summary>
    /// <param name="organizationIds">組織 ID 列表（用於組織範圍過濾，應使用 OrganizationScope.ScopeIds）</param>
    public List<vw_permission_user> LoadUserList(string? searchTerm = null, string? organization = null, string? role = null, bool? isActive = null, List<int>? organizationIds = null)
        => repo.QueryUserList(searchTerm, organization, role, isActive, organizationIds);

    /// <summary>
    /// 載入用戶
    /// </summary>
    public vw_permission_user? LoadUser(int id)
        => repo.QueryUser(id: id);

    /// <summary>
    /// 載入角色列表
    /// </summary>
    /// <param name="organizationIds">組織 ID 列表（用於組織範圍過濾，應使用 OrganizationScope.ScopeIds）</param>
    public List<vw_permission_role> LoadRoleList(string? searchTerm = null, string? organization = null, bool? isActive = null, List<int>? organizationIds = null)
        => repo.QueryRoleList(searchTerm, organization, isActive, organizationIds);

    /// <summary>
    /// 載入角色
    /// </summary>
    public vw_permission_role? LoadRole(int id)
        => repo.QueryRole(id: id);

    /// <summary>
    /// 載入組織列表
    /// </summary>
    /// <param name="organizationIds">組織 ID 列表（用於組織範圍過濾，應使用 OrganizationScope.ScopeIds）</param>
    public List<vw_permission_organization> LoadOrganizationList(string? searchTerm = null, int? level = null, bool? isActive = null, List<int>? organizationIds = null)
        => repo.QueryOrganizationList(searchTerm, level, isActive, organizationIds);

    /// <summary>
    /// 載入組織
    /// </summary>
    public vw_permission_organization? LoadOrganization(int id)
        => repo.QueryOrganization(id: id);

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
                    var role = repo.QueryRole(id: int.Parse(kvp.Key));
                    var roleName = role?.name ?? kvp.Key;
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
    public bool DeletePermissionManagement(int id)
        => repo.DeletePermissionManagement(id);

    #endregion

    #region ViewBag 設定方法

    /// <summary>
    /// 設定權限管理統一的 ViewBag 靜態資料
    /// </summary>
    /// <param name="viewBag">ViewBag 物件</param>
    /// <param name="organizationIds">組織 ID 列表（用於組織範圍過濾，應使用 OrganizationScope.ScopeIds）</param>
    public void ViewBagModel(dynamic viewBag, List<int>? organizationIds = null)
    {
        viewBag.Organizations = repo.QueryOrganizationList(organizationIds: organizationIds);
        viewBag.Roles = repo.QueryRoleList(organizationIds: organizationIds);
        viewBag.Users = repo.QueryUserList(organizationIds: organizationIds);
        viewBag.Navigations = sidebarService.LoadNavigationList();
    }

    #endregion

}