using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

public class PermissionManagementService(PermissionManagementRepository repo, SidebarNavigationService sidebarService)
{
    #region 統一的查詢方法

    /// <summary>
    /// 載入用戶列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="organization">組織篩選</param>
    /// <param name="role">角色篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <returns>用戶列表</returns>
    public List<vw_permission_user> LoadUsers(string? searchTerm = null, string? organization = null, string? role = null, bool? isActive = null)
        => repo.QueryUserList(null, searchTerm, organization, role, isActive);

    /// <summary>
    /// 載入角色列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="organization">組織篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <returns>角色列表</returns>
    public List<vw_permission_role> LoadRoles(string? searchTerm = null, string? organization = null, bool? isActive = null)
        => repo.QueryRoleList(searchTerm, organization, isActive);

    /// <summary>
    /// 載入組織列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="level">層級篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <returns>組織列表</returns>
    public List<vw_permission_organization> LoadOrganizations(string? searchTerm = null, int? level = null, bool? isActive = null)
        => repo.QueryOrganizationList(searchTerm, level, isActive);

    /// <summary>
    /// 載入單一用戶
    /// </summary>
    /// <param name="id">用戶ID</param>
    /// <returns>用戶或null</returns>
    public vw_permission_user? LoadUserById(int id)
        => repo.QueryUser(id);

    /// <summary>
    /// 載入單一角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>角色或null</returns>
    public vw_permission_role? LoadRoleById(int id)
        => repo.QueryRole(id);

    /// <summary>
    /// 載入單一組織
    /// </summary>
    /// <param name="id">組織ID</param>
    /// <returns>組織或null</returns>
    public vw_permission_organization? LoadOrganizationById(int id)
        => repo.QueryOrganization(id);

    #endregion

    #region 統一的儲存方法

    /// <summary>
    /// 儲存用戶資料
    /// </summary>
    /// <param name="user">用戶資料</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) CreateUser(permission_management user)
    {
        repo.InsertUser(user);
        return (true, "用戶儲存成功");
    }

    /// <summary>
    /// 儲存角色資料
    /// </summary>
    /// <param name="role">角色資料</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) CreateRole(permission_management role)
    {
        repo.InsertRole(role);
        return (true, "角色儲存成功");
    }

    /// <summary>
    /// 儲存組織資料
    /// </summary>
    /// <param name="organization">組織資料</param>
    /// <returns>操作結果</returns>
    public (bool success, string message) CreateOrganization(permission_management organization)
    {
        repo.InsertOrganization(organization);
        return (true, "組織儲存成功");
    }


    #endregion

    #region ViewBag 設定方法

    /// <summary>
    /// 設定權限管理統一的 ViewBag 資料
    /// </summary>
    /// <param name="viewBag">ViewBag 物件</param>
    /// <param name="id">資料ID</param>
    public void ViewBagModel(dynamic viewBag, int? id = null)
    {
        viewBag.Id = id;

        viewBag.Organizations = LoadOrganizations();
        viewBag.Organization = id.HasValue ? LoadOrganizationById(id.Value) : null;

        viewBag.Roles = LoadRoles();
        viewBag.Role = id.HasValue ? LoadRoleById(id.Value) : null;

        viewBag.Users = LoadUsers();
        viewBag.User = id.HasValue ? LoadUserById(id.Value) : null;

        viewBag.Navigations = sidebarService.GetAllNavigations();
        
        // 載入角色的導航權限資料
        if (id.HasValue)
        {
            var roleData = repo.ValidPermissionManagements.FirstOrDefault(p => p.id == id.Value && p.type == "role");
            viewBag.RoleNavigationPermissions = roleData?.navigation_permissions ?? new List<string>();
        }
        else
        {
            viewBag.RoleNavigationPermissions = new List<string>();
        }
    }

    #endregion

    #region 輔助方法

    /// <summary>
    /// 刪除權限管理資料
    /// </summary>
    /// <param name="id">資料ID</param>
    /// <returns>是否刪除成功</returns>
    public bool Delete(int id)
        => repo.Delete(id);

    #endregion

}