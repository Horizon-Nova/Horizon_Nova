using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Repositories;

public class PermissionManagementRepository(HnbHnbBackofficeDbContext db)
{
    #region 人員管理
    public async Task<List<vw_permission_user>> LoadUsersAsync()
    {
        return await db.vw_permission_user
            .Where(u => u.is_active == true)
            .OrderBy(u => u.full_name)
            .ToListAsync();
    }

    public async Task<vw_permission_user?> LoadUserByIdAsync(int id)
    {
        return await db.vw_permission_user
            .FirstOrDefaultAsync(u => u.id == id);
    }

    public async Task<List<vw_permission_user>> SearchUsersAsync(string? searchTerm, string? organization, string? role, bool? isActive)
    {
        var query = db.vw_permission_user.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => 
                (u.full_name != null && u.full_name.Contains(searchTerm)) ||
                (u.username != null && u.username.Contains(searchTerm)) ||
                (u.email != null && u.email.Contains(searchTerm)));
        }

        if (!string.IsNullOrEmpty(organization))
        {
            query = query.Where(u => u.organization_name == organization);
        }

        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.role_name == role);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.is_active == isActive.Value);
        }

        return await query.OrderBy(u => u.full_name).ToListAsync();
    }
    #endregion

    #region 角色管理
    public async Task<List<vw_permission_role>> LoadRolesAsync()
    {
        return await db.vw_permission_role
            .Where(r => r.is_active == true)
            .OrderBy(r => r.name)
            .ToListAsync();
    }

    public async Task<vw_permission_role?> LoadRoleByIdAsync(int id)
    {
        return await db.vw_permission_role
            .FirstOrDefaultAsync(r => r.id == id);
    }

    public async Task<List<vw_permission_role>> SearchRolesAsync(string? searchTerm, string? organization, bool? isActive)
    {
        var query = db.vw_permission_role.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(r => 
                (r.name != null && r.name.Contains(searchTerm)) ||
                (r.description != null && r.description.Contains(searchTerm)));
        }

        if (!string.IsNullOrEmpty(organization))
        {
            query = query.Where(r => r.organization_name == organization);
        }

        if (isActive.HasValue)
        {
            query = query.Where(r => r.is_active == isActive.Value);
        }

        return await query.OrderBy(r => r.name).ToListAsync();
    }
    #endregion

    #region 組織管理
    public async Task<List<vw_permission_organization>> LoadOrganizationsAsync()
    {
        return await db.vw_permission_organization
            .Where(o => o.is_active == true)
            .OrderBy(o => o.name)
            .ToListAsync();
    }

    public async Task<vw_permission_organization?> LoadOrganizationByIdAsync(int id)
    {
        return await db.vw_permission_organization
            .FirstOrDefaultAsync(o => o.id == id);
    }

    public async Task<List<vw_permission_organization>> SearchOrganizationsAsync(string? searchTerm, int? level, bool? isActive)
    {
        var query = db.vw_permission_organization.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(o => 
                (o.name != null && o.name.Contains(searchTerm)) ||
                (o.description != null && o.description.Contains(searchTerm)));
        }

        if (level.HasValue)
        {
            query = query.Where(o => o.level == level.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(o => o.is_active == isActive.Value);
        }

        return await query.OrderBy(o => o.name).ToListAsync();
    }
    #endregion

    #region 詳細資料載入
    public async Task<vw_permission_user?> LoadUserDetailsAsync(int id)
    {
        return await db.vw_permission_user
            .FirstOrDefaultAsync(u => u.id == id);
    }

    public async Task<int?> GetUserOrganizationIdAsync(int userId)
    {
        var user = await db.permission_managements
            .FirstOrDefaultAsync(pm => pm.id == userId && pm.type == "user");
        return user?.parent_id;
    }

    public async Task<vw_permission_role?> LoadRoleDetailsAsync(int id)
    {
        return await db.vw_permission_role
            .FirstOrDefaultAsync(r => r.id == id);
    }

    public async Task<vw_permission_organization?> LoadOrganizationDetailsAsync(int id)
    {
        return await db.vw_permission_organization
            .FirstOrDefaultAsync(o => o.id == id);
    }
    #endregion

    #region 基本 CRUD 操作
    public async Task<permission_management?> LoadPermissionManagementByIdAsync(int id)
    {
        return await db.permission_managements
            .FirstOrDefaultAsync(pm => pm.id == id);
    }

    public async Task<permission_management> CreatePermissionManagementAsync(permission_management entity)
    {
        db.permission_managements.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<permission_management> UpdatePermissionManagementAsync(permission_management entity)
    {
        db.permission_managements.Update(entity);
        await db.SaveChangesAsync();
        return entity;
    }

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

    public async Task<List<permission_management>> LoadPermissionManagementsByTypeAsync(string type)
    {
        return await db.permission_managements
            .Where(pm => pm.type == type && pm.is_active == true)
            .OrderBy(pm => pm.name)
            .ToListAsync();
    }
    #endregion
}