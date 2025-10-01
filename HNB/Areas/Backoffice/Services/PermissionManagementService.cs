using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

public class PermissionManagementService(PermissionManagementRepository repo)
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
    public async Task<bool> DeleteUserAsync(int id)
    {
        return await repo.DeletePermissionManagementAsync(id);
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        return await repo.DeletePermissionManagementAsync(id);
    }

    public async Task<bool> DeleteOrganizationAsync(int id)
    {
        return await repo.DeletePermissionManagementAsync(id);
    }
    #endregion

    #region CRUD 操作
    public async Task<permission_management> CreatePermissionManagementAsync(permission_management entity)
    {
        return await repo.CreatePermissionManagementAsync(entity);
    }

    public async Task<permission_management> UpdatePermissionManagementAsync(permission_management entity)
    {
        return await repo.UpdatePermissionManagementAsync(entity);
    }

    public async Task<(bool success, string message)> SaveUserAsync(IFormCollection form, string action)
    {
        try
        {
            // 服務層負責欄位對應和資料轉換
            var user = new permission_management
            {
                id = int.TryParse(form["id"], out var id) ? id : 0,
                type = "user",
                name = form["name"],
                email = form["email"],
                phone = form["phone"],
                gender = form["gender"],
                full_name = form["username"],
                password_hash = !string.IsNullOrEmpty(form["password"].ToString()) ? form["password"].ToString() : null,
                is_active = form["is_active"] == "true",
                nickname = form["nickname"],
                zodiac_sign = form["zodiac_sign"],
                favorite_color = form["favorite_color"],
                location = form["location"],
                bio = form["bio"],
            };

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

            if (action == "add")
            {
                await repo.CreatePermissionManagementAsync(user);
                return (true, "使用者新增成功");
            }
            else
            {
                await repo.UpdatePermissionManagementAsync(user);
                return (true, "使用者更新成功");
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> SaveRoleAsync(IFormCollection form, string action)
    {
        try
        {
            var role = new permission_management
            {
                id = int.TryParse(form["id"], out var id) ? id : 0,
                type = "role",
                name = form["name"],
                description = form["description"],
                is_active = form["is_active"] == "true",
            };

            if (action == "add")
            {
                await repo.CreatePermissionManagementAsync(role);
                return (true, "角色新增成功");
            }
            else
            {
                await repo.UpdatePermissionManagementAsync(role);
                return (true, "角色更新成功");
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> SaveOrganizationAsync(IFormCollection form, string action)
    {
        try
        {
            var organization = new permission_management
            {
                id = int.TryParse(form["id"], out var id) ? id : 0,
                type = "organization",
                name = form["name"],
                description = form["description"],
                level = int.TryParse(form["level"], out var level) ? level : 1,
                is_active = form["is_active"] == "true",
            };

            if (action == "add")
            {
                await repo.CreatePermissionManagementAsync(organization);
                return (true, "組織新增成功");
            }
            else
            {
                await repo.UpdatePermissionManagementAsync(organization);
                return (true, "組織更新成功");
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
    #endregion
}