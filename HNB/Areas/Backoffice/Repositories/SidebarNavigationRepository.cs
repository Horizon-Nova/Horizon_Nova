using Models.HnbHnbBackoffice;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 側欄導航資料存取層，負責處理導航項目的資料存取功能
/// </summary>
public class SidebarNavigationRepository(HnbHnbBackofficeDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的導航項目查詢來源
    /// </summary>
    private IQueryable<vw_sidebar_navigation> ValidNavigations => db.vw_sidebar_navigations.OrderBy(n => n.sort_order);
    
    /// <summary>
    /// 有效的側欄導航查詢來源
    /// </summary>
    public IQueryable<sidebar_navigation> ValidSidebarNavigations => db.sidebar_navigations;
    
    #endregion

    #region 專用查詢方法
    /// <summary>
    /// 查詢導航項目列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="parentCode">父項目篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <returns>導航項目列表</returns>
    public List<vw_sidebar_navigation> QueryNavigationList(string? searchTerm = null, string? parentCode = null, bool? isActive = null)
        => ValidNavigations
            .Where(n =>
                (string.IsNullOrEmpty(searchTerm) || 
                    (n.title != null && n.title.Contains(searchTerm)) ||
                    (n.code != null && n.code.Contains(searchTerm))) &&
                (string.IsNullOrEmpty(parentCode) || n.parent_code == parentCode) &&
                (!isActive.HasValue || n.is_active == isActive.Value)
            )
            .ToList();

    /// <summary>
    /// 查詢單一導航項目
    /// </summary>
    /// <param name="id">導航項目ID</param>
    /// <returns>導航項目或null</returns>
    public vw_sidebar_navigation? QueryNavigation(int id)
        => ValidNavigations.FirstOrDefault(n => n.id == id);

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
        => await ValidNavigations.ToListAsync();

    /// <summary>
    /// 獲取所有側欄導航項目（同步版本）
    /// </summary>
    public List<vw_sidebar_navigation> GetAllNavigations()
        => ValidNavigations.ToList();

    /// <summary>
    /// 根據ID獲取導航項目
    /// </summary>
    public async Task<sidebar_navigation?> GetNavigationByIdAsync(int id)
        => await db.sidebar_navigations.FindAsync(id);

    /// <summary>
    /// 根據Code獲取導航項目
    /// </summary>
    public async Task<sidebar_navigation?> GetNavigationByCodeAsync(string code)
        => await db.sidebar_navigations.FirstOrDefaultAsync(n => n.code == code);

    /// <summary>
    /// 獲取所有父級導航項目
    /// </summary>
    public async Task<List<sidebar_navigation>> GetParentNavigationsAsync()
        => await db.sidebar_navigations
            .Where(n => n.parent_code == null)
            .OrderBy(n => n.sort_order)
            .ToListAsync();

    #endregion

}