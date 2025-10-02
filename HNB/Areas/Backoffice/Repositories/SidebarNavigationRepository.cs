using Models.HnbHnbBackoffice;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.Backoffice.Repositories;

public class SidebarNavigationRepository(HnbHnbBackofficeDbContext db)
{
    /// <summary>
    /// 根據用戶名獲取側欄導航項目（包含角色權限）
    /// </summary>
    public async Task<List<vw_sidebar_navigation>> GetNavigationByUserAsync(string userName)
    {
        // 取得使用者的導航權限
        var userPermissions = await GetUserNavigationPermissionsAsync(userName);
        
        // 如果使用者沒有任何權限，返回基本導航（儀表板和個人資料）
        if (!userPermissions.Any())
        {
            return await db.vw_sidebar_navigations
                .Where(n => n.code == "dashboard" || n.code == "profile")
                .OrderBy(n => n.sort_order)
                .ToListAsync();
        }
        
        // 取得所有導航項目
        var allNavigations = await db.vw_sidebar_navigations
            .OrderBy(n => n.sort_order)
            .ToListAsync();
        
        // 找出用戶有權限的導航項目
        var allowedNavigations = new HashSet<string>(userPermissions);
        
        // 如果用戶有子項目權限，也要包含其父項目
        foreach (var nav in allNavigations)
        {
            if (!string.IsNullOrEmpty(nav.parent_code) && userPermissions.Contains(nav.code ?? ""))
            {
                // 遞歸添加所有父項目
                AddParentNavigations(allNavigations, nav.parent_code, allowedNavigations);
            }
        }
        
        // 同時，如果用戶有父項目權限，也要包含其所有子項目（可選，根據需求）
        var originalAllowed = new HashSet<string>(allowedNavigations);
        foreach (var nav in allNavigations)
        {
            if (string.IsNullOrEmpty(nav.parent_code) && originalAllowed.Contains(nav.code ?? ""))
            {
                // 添加所有子項目
                AddChildNavigations(allNavigations, nav.code ?? "", allowedNavigations);
            }
        }
        
        // 返回用戶有權限的導航項目（包含必要的父項目）
        return allNavigations
            .Where(n => allowedNavigations.Contains(n.code ?? ""))
            .ToList();
    }
    
    /// <summary>
    /// 遞歸添加父項目導航權限
    /// </summary>
    private void AddParentNavigations(List<vw_sidebar_navigation> allNavigations, string parentCode, HashSet<string> allowedNavigations)
    {
        if (string.IsNullOrEmpty(parentCode) || allowedNavigations.Contains(parentCode))
            return;
            
        allowedNavigations.Add(parentCode);
        
        // 找到父項目，繼續遞歸
        var parent = allNavigations.FirstOrDefault(n => n.code == parentCode);
        if (parent != null && !string.IsNullOrEmpty(parent.parent_code))
        {
            AddParentNavigations(allNavigations, parent.parent_code, allowedNavigations);
        }
    }
    
    /// <summary>
    /// 遞歸添加子項目導航權限
    /// </summary>
    private void AddChildNavigations(List<vw_sidebar_navigation> allNavigations, string parentCode, HashSet<string> allowedNavigations)
    {
        var children = allNavigations.Where(n => n.parent_code == parentCode).ToList();
        
        foreach (var child in children)
        {
            if (!string.IsNullOrEmpty(child.code) && !allowedNavigations.Contains(child.code))
            {
                allowedNavigations.Add(child.code);
                
                // 遞歸添加子項目的子項目
                AddChildNavigations(allNavigations, child.code, allowedNavigations);
            }
        }
    }

    /// <summary>
    /// 取得使用者的導航權限
    /// </summary>
    private async Task<List<string>> GetUserNavigationPermissionsAsync(string userName)
    {
        var permissions = new List<string>();
        
        // 基本權限：所有使用者都能看到儀表板和個人資料
        permissions.AddRange(new[] { "dashboard", "profile" });
        
        // 從使用者直接權限取得
        var userDirectPermissions = await db.permission_managements
            .Where(pm => pm.type == "user" && pm.name == userName && pm.navigation_permissions != null)
            .Select(pm => pm.navigation_permissions)
            .FirstOrDefaultAsync();
            
        if (userDirectPermissions != null && userDirectPermissions.Any())
        {
            permissions.AddRange(userDirectPermissions);
        }
        
        // 從使用者角色權限取得
        var user = await db.permission_managements
            .Where(pm => pm.type == "user" && pm.name == userName && pm.roles != null)
            .FirstOrDefaultAsync();
            
        var userRolePermissions = new List<List<string>>();
        if (user?.roles != null)
        {
            foreach (var roleIdStr in user.roles)
            {
                if (int.TryParse(roleIdStr, out var roleId))
                {
                    var role = await db.permission_managements
                        .Where(pm => pm.id == roleId && pm.type == "role" && pm.navigation_permissions != null)
                        .FirstOrDefaultAsync();
                        
                    if (role?.navigation_permissions != null)
                    {
                        userRolePermissions.Add(role.navigation_permissions);
                    }
                }
            }
        }
        
        foreach (var rolePermissions in userRolePermissions)
        {
            if (rolePermissions != null && rolePermissions.Any())
            {
                permissions.AddRange(rolePermissions);
            }
        }
        
        // 去除重複並返回
        return permissions.Distinct().ToList();
    }

    /// <summary>
    /// 獲取所有側欄導航項目（管理用）
    /// </summary>
    public async Task<List<vw_sidebar_navigation>> GetAllNavigationsAsync()
    {
        return await db.vw_sidebar_navigations
            .OrderBy(n => n.parent_code)
            .ThenBy(n => n.sort_order)
            .ToListAsync();
    }

    /// <summary>
    /// 獲取所有側欄導航項目（同步版本）
    /// </summary>
    public List<vw_sidebar_navigation> GetAllNavigations()
    {
        return db.vw_sidebar_navigations
            .OrderBy(n => n.parent_code)
            .ThenBy(n => n.sort_order)
            .ToList();
    }

    /// <summary>
    /// 根據ID獲取導航項目
    /// </summary>
    public async Task<sidebar_navigation?> GetNavigationByIdAsync(int id)
    {
        return await db.sidebar_navigations.FindAsync(id);
    }

    /// <summary>
    /// 根據Code獲取導航項目
    /// </summary>
    public async Task<sidebar_navigation?> GetNavigationByCodeAsync(string code)
    {
        return await db.sidebar_navigations
            .FirstOrDefaultAsync(n => n.code == code);
    }

    /// <summary>
    /// 新增導航項目
    /// </summary>
    public async Task<sidebar_navigation> CreateNavigationAsync(sidebar_navigation navigation)
    {
        db.sidebar_navigations.Add(navigation);
        await db.SaveChangesAsync();
        return navigation;
    }

    /// <summary>
    /// 更新導航項目
    /// </summary>
    public async Task<sidebar_navigation> UpdateNavigationAsync(sidebar_navigation navigation)
    {
        db.sidebar_navigations.Update(navigation);
        await db.SaveChangesAsync();
        return navigation;
    }

    /// <summary>
    /// 刪除導航項目
    /// </summary>
    public async Task<bool> DeleteNavigationAsync(int id)
    {
        var navigation = await db.sidebar_navigations.FindAsync(id);
        if (navigation == null) return false;

        db.sidebar_navigations.Remove(navigation);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 檢查導航項目是否存在
    /// </summary>
    public async Task<bool> NavigationExistsAsync(string code)
    {
        return await db.sidebar_navigations.AnyAsync(n => n.code == code);
    }

    /// <summary>
    /// 獲取所有父級導航項目
    /// </summary>
    public async Task<List<sidebar_navigation>> GetParentNavigationsAsync()
    {
        return await db.sidebar_navigations
            .Where(n => n.parent_code == null)
            .OrderBy(n => n.sort_order)
            .ToListAsync();
    }
}