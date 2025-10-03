using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;
using System.Security.Cryptography;
using System.Text;

namespace HNB.Areas.Backoffice.Services;

public class PermissionManagementService(PermissionManagementRepository repo, SidebarNavigationService sidebarService)
{
    #region 人員管理
    public List<vw_permission_user> LoadUsers()
        => repo.LoadUsers();

    public vw_permission_user? LoadUserById(int id)
        => repo.LoadUserById(id);

    public List<vw_permission_user> SearchUsers(string? searchTerm, string? organization, string? role, bool? isActive)
        => repo.SearchUsers(searchTerm, organization, role, isActive);

    public vw_permission_user? LoadUserDetails(int id)
        => repo.LoadUserDetails(id);

    public int? GetUserOrganizationId(int userId)
        => repo.GetUserOrganizationId(userId);
    #endregion

    #region 角色管理
    public List<vw_permission_role> LoadRoles()
        => repo.LoadRoles();

    public vw_permission_role? LoadRoleById(int id)
        => repo.LoadRoleById(id);

    public List<vw_permission_role> SearchRoles(string? searchTerm, string? organization, bool? isActive)
        => repo.SearchRoles(searchTerm, organization, isActive);

    public vw_permission_role? LoadRoleDetails(int id)
        => repo.LoadRoleDetails(id);

    public int? GetRoleOrganizationId(int roleId)
        => repo.GetRoleOrganizationId(roleId);
    #endregion

    #region 組織管理
    public List<vw_permission_organization> LoadOrganizations()
        => repo.LoadOrganizations();

    public vw_permission_organization? LoadOrganizationById(int id)
        => repo.LoadOrganizationById(id);

    public List<vw_permission_organization> SearchOrganizations(string? searchTerm, int? level, bool? isActive)
        => repo.SearchOrganizations(searchTerm, level, isActive);

    public vw_permission_organization? LoadOrganizationDetails(int id)
        => repo.LoadOrganizationDetails(id);
    #endregion

    #region 刪除操作
    public bool DeleteUser(int id)
    {
        return repo.DeletePermissionManagement(id);
    }

    public bool DeleteRole(int id)
    {
        return repo.DeletePermissionManagement(id);
    }

    public bool DeleteOrganization(int id)
    {
        return repo.DeletePermissionManagement(id);
    }
    #endregion

    #region CRUD 操作
    public permission_management CreatePermissionManagement(permission_management entity)
    {
        return repo.CreatePermissionManagement(entity);
    }

    public permission_management UpdatePermissionManagement(permission_management entity)
    {
        return repo.UpdatePermissionManagement(entity);
    }

    public (bool success, string message) SaveUser(IFormCollection form)
    {
        // 服務層負責欄位對應和資料轉換
        var user = new permission_management
        {
            id = int.TryParse(form["id"], out var id) ? id : 0,
            type = "user",
            name = form["username"],
            email = form["email"],
            phone = form["phone"],
            gender = form["gender"],
            full_name = form["full_name"],
            is_active = form["is_active"] == "true",
            nickname = form["nickname"],
            zodiac_sign = form["zodiac_sign"],
            favorite_color = form["favorite_color"],
            location = form["location"],
            bio = form["bio"],
        };

        // 處理密碼雜湊
        var password = form["password"].ToString();
        if (!string.IsNullOrEmpty(password) && password != "********")
        {
            // 只有當密碼不是預設的遮罩值時才進行雜湊處理
            var (hash, salt) = HashPassword(password);
            user.password_hash = hash;
            user.salt = salt;
        }
        else if (user.id > 0 && string.IsNullOrEmpty(password))
        {
            // 編輯時如果密碼為空，保持原有的密碼雜湊不變
            var existingUser = repo.GetPermissionManagementById(user.id);
            if (existingUser != null)
            {
                user.password_hash = existingUser.password_hash;
                user.salt = existingUser.salt;
            }
        }

        // 處理角色ID
        var roleId = form["role_id"].ToString();
        if (!string.IsNullOrEmpty(roleId))
        {
            user.roles = new List<string> { roleId };
        }

        // 處理組織ID
        var organizationId = form["organization_id"].ToString();
        if (!string.IsNullOrEmpty(organizationId) && int.TryParse(organizationId, out var orgId))
        {
            user.parent_id = orgId;
        }

        // 判斷是新增還是修改：id > 0 就是修改，否則就是新增
        if (user.id > 0)
        {
            // 修改
            repo.UpdatePermissionManagement(user);
            return (true, "使用者更新成功");
        }
        else
        {
            // 新增
            repo.CreatePermissionManagement(user);
            return (true, "使用者新增成功");
        }
    }

    public (bool success, string message) SaveRole(IFormCollection form)
    {
        var role = new permission_management
        {
            id = int.TryParse(form["id"], out var id) ? id : 0,
            type = "role",
            name = form["name"],
            description = form["description"],
            is_active = form["is_active"] == "true",
        };

        // 處理導航權限
        var navigationPermissionsJson = form["navigation_permissions"].ToString();
        if (!string.IsNullOrEmpty(navigationPermissionsJson))
        {
            try
            {
                var permissions = System.Text.Json.JsonSerializer.Deserialize<List<string>>(navigationPermissionsJson);
                role.navigation_permissions = permissions;
            }
            catch (System.Text.Json.JsonException)
            {
                // JSON 解析失敗時設為空陣列，這是預期的處理
                role.navigation_permissions = new List<string>();
            }
        }
        else
        {
            role.navigation_permissions = new List<string>();
        }

        // 判斷是新增還是修改：id > 0 就是修改，否則就是新增
        if (role.id > 0)
        {
            // 修改
            repo.UpdatePermissionManagement(role);
            return (true, "角色更新成功");
        }
        else
        {
            // 新增
            repo.CreatePermissionManagement(role);
            return (true, "角色新增成功");
        }
    }

    public (bool success, string message) SaveOrganization(IFormCollection form)
    {
        var organization = new permission_management
        {
            id = int.TryParse(form["id"], out var id) ? id : 0,
            type = "organization",
            name = form["name"],
            description = form["description"],
            level = int.TryParse(form["level"], out var level) ? level : 1,
            parent_id = int.TryParse(form["parent_id"], out var parentId) ? parentId : null,
            is_active = form["is_active"] == "true",
        };

        // 處理分配角色
        var assignedRolesJson = form["assigned_roles"].ToString();
        if (!string.IsNullOrEmpty(assignedRolesJson))
        {
            try
            {
                var roleIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(assignedRolesJson);
                organization.roles = roleIds;
            }
            catch (System.Text.Json.JsonException)
            {
                // JSON 解析失敗時設為空陣列，這是預期的處理
                organization.roles = new List<string>();
            }
        }
        else
        {
            organization.roles = new List<string>();
        }

        // 判斷是新增還是修改：id > 0 就是修改，否則就是新增
        if (organization.id > 0)
        {
            // 修改：先清除舊的角色關聯
            ClearRoleParentIds(organization.id);
            
            repo.UpdatePermissionManagement(organization);
            
            // 更新新的角色關聯
            UpdateRoleParentIds(organization.roles, organization.id);
            
            return (true, "組織更新成功");
        }
        else
        {
            // 新增
            var result = repo.CreatePermissionManagement(organization);
            
            // 更新角色的parent_id
            UpdateRoleParentIds(organization.roles, result.id);
            
            return (true, "組織新增成功");
        }
    }

    /// <summary>
    /// 更新角色的parent_id
    /// </summary>
    private void UpdateRoleParentIds(List<string>? roleIds, int organizationId)
    {
        if (roleIds == null || !roleIds.Any()) return;

        foreach (var roleIdStr in roleIds)
        {
            if (int.TryParse(roleIdStr, out var roleId))
            {
                repo.UpdateRoleParentId(roleId, organizationId);
            }
        }
    }

    /// <summary>
    /// 清除角色的parent_id
    /// </summary>
    private void ClearRoleParentIds(int organizationId)
    {
        repo.ClearRoleParentIdsByOrganization(organizationId);
    }
    #endregion

    #region 密碼處理
    /// <summary>
    /// 生成隨機鹽值
    /// </summary>
    /// <returns>鹽值</returns>
    private static string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    /// <summary>
    /// 雜湊密碼
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <returns>雜湊值和鹽值</returns>
    private (string hash, string salt) HashPassword(string password)
    {
        var salt = GenerateSalt();
        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        var hash = Convert.ToBase64String(hashedBytes);
        return (hash, salt);
    }
    #endregion

    /// <summary>
    /// 取得可用角色（未分配給其他組織的角色）
    /// </summary>
    /// <param name="organizationId">組織ID（編輯時使用，0表示新增）</param>
    /// <returns>可用角色列表</returns>
    public List<vw_permission_role> GetAvailableRoles(int organizationId = 0)
        => repo.GetAvailableRoles(organizationId);

    /// <summary>
    /// 取得角色的導航權限
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns>導航權限列表</returns>
    public List<string> GetRoleNavigationPermissions(int roleId)
        => repo.GetRoleNavigationPermissions(roleId);

    #region ViewBag 資料準備
    /// <summary>
    /// 準備權限管理的 ViewBag 資料
    /// </summary>
    /// <param name="viewBag">ViewBag 物件</param>
    /// <param name="type">類型 (user, role, organization)</param>
    /// <param name="id">ID (可選)</param>
    public void ViewBagModelPM(dynamic viewBag, string type = "", int? id = null)
    {
        // 設定表單標題和副標題
        switch (type.ToLower())
        {
            case "user":
                viewBag.UserFormTitle = id.HasValue ? "編輯帳號" : "新增帳號";
                viewBag.UserFormSubtitle = id.HasValue ? "修改帳號基本資訊" : "建立新的系統使用者";
                if (id.HasValue)
                {
                    viewBag.User = LoadUserDetails(id.Value);
                }
                break;
                
            case "role":
                viewBag.RoleFormTitle = id.HasValue ? "編輯角色" : "新增角色";
                viewBag.RoleFormSubtitle = id.HasValue ? "修改角色基本資訊" : "建立新的系統角色";
                if (id.HasValue)
                {
                    var role = LoadRoleDetails(id.Value);
                    System.Diagnostics.Debug.WriteLine($"LoadRoleDetails({id.Value}) 返回: {role?.name ?? "null"}");
                    viewBag.Role = role;
                }
                else
                {
                    // 為了讓 roleDetailModal 能正常顯示，設置一個空的 Role 物件
                    viewBag.Role = null;
                }
                break;
                
            case "organization":
                viewBag.OrganizationFormTitle = id.HasValue ? "編輯組織" : "新增組織";
                viewBag.OrganizationFormSubtitle = id.HasValue ? "修改組織基本資訊" : "建立新的組織單位";
                if (id.HasValue)
                {
                    viewBag.Organization = LoadOrganizationDetails(id.Value);
                }
                break;
        }
        
        // 載入選項數據
        viewBag.Roles = LoadRoles();
        viewBag.Organizations = LoadOrganizations();
        viewBag.Navigations = GetAllNavigations();
    }

    /// <summary>
    /// 準備帳號管理頁面的 ViewBag 資料
    /// </summary>
    /// <param name="viewBag">ViewBag 物件</param>
    public void ViewBagModelUsers(dynamic viewBag)
    {
        viewBag.Organizations = LoadOrganizations();
        viewBag.Roles = LoadRoles();
    }

    /// <summary>
    /// 準備角色管理頁面的 ViewBag 資料
    /// </summary>
    /// <param name="viewBag">ViewBag 物件</param>
    public void ViewBagModelRoles(dynamic viewBag)
    {
        viewBag.Organizations = LoadOrganizations();
    }

    /// <summary>
    /// 準備組織管理頁面的 ViewBag 資料
    /// </summary>
    /// <param name="viewBag">ViewBag 物件</param>
    public void ViewBagModelOrganizations(dynamic viewBag)
    {
        viewBag.Roles = LoadRoles();
    }
    #endregion

    /// <summary>
    /// 取得所有導航項目
    /// </summary>
    /// <returns>導航項目列表</returns>
    private List<NavigationItem> GetAllNavigations()
    {
        return sidebarService.GetAllNavigations();
    }
}