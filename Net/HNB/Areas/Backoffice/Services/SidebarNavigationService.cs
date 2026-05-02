using HNB.Areas.Backoffice.Repositories;
using Models.HnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

public class SidebarNavigationService(
    SidebarNavigationRepository rep,
    PermissionManagementRepository permRep)
{
    #region 統一的查詢方法

    /// <summary>
    /// 載入導航項目列表
    /// </summary>
    /// <param name="form">查詢條件模型，null 代表不套用任何條件</param>
    /// <returns>導航項目列表</returns>
    public List<vw_sidebar_navigation> LoadNavigationList(vw_sidebar_navigation? form)
        => rep.QueryNavigationList(form);

    /// <summary>
    /// 載入單一導航項目
    /// </summary>
    /// <param name="id">導航項目ID</param>
    /// <returns>導航項目或null</returns>
    public vw_sidebar_navigation? LoadNavigation(int? id)
        => rep.QueryNavigation(id);

    #endregion

    #region 特殊查詢

    /// <summary>
    /// 載入上層目錄列表（parent_code 為空代表根目錄）
    /// </summary>
    public List<vw_sidebar_navigation> LoadParentNavigationList()
        => rep.QueryParentNavigationList();
    
    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 創建導航項目
    /// </summary>
    public sidebar_navigation? CreateNavigation(sidebar_navigation form)
        => rep.InsertNavigation(form);

    /// <summary>
    /// 刪除導航項目
    /// </summary>
    public bool DeleteNavigation(int id)
        => rep.DeleteNavigation(id);

    #endregion

    #region 輔助方法

    /// <summary>
    /// 計算下一個可用的排序順序（根據 parent_code）
    /// </summary>
    /// <param name="parentCode">父級導航代碼，null 或空字串代表根目錄</param>
    /// <returns>下一個可用的排序順序</returns>
    public int GetNextSortOrder(string? parentCode = null)
    {
        var normalizedParentCode = string.IsNullOrEmpty(parentCode) ? null : parentCode;
        var navigations = LoadNavigationList(new vw_sidebar_navigation { parent_code = normalizedParentCode });
        
        if (!navigations.Any())
            return 1;
        
        var maxSortOrder = navigations
            .Where(n => n.sort_order.HasValue)
            .Select(n => n.sort_order!.Value)
            .DefaultIfEmpty(0)
            .Max();
        
        return maxSortOrder + 1;
    }

    /// <summary>
    /// 載入用戶的側欄導航列表（根據用戶角色權限）
    /// </summary>
    /// <param name="userName">用戶名稱</param>
    /// <returns>用戶有權限查看的導航項目列表</returns>
    public List<vw_sidebar_navigation> LoadUserNavigationList(string userName)
    {
        // 收集用戶的導航權限代碼
        var permissionCodes = CollectUserPermissionCodes(userName);
        
        // 取得所有啟用的導航項目
        var allNavigations = LoadNavigationList(new vw_sidebar_navigation { is_active = true });
        
        // 建立導航代碼對應表（提升查詢效率）
        var navigationByCode = allNavigations
            .Where(n => !string.IsNullOrEmpty(n.code))
            .ToDictionary(n => n.code!, n => n);
        
        // 找出用戶直接有權限的導航項目
        var allowedCodes = new HashSet<string>(permissionCodes);
        
        // 遞迴添加父項目（如果子項目有權限，父項目也必須顯示）
        foreach (var code in permissionCodes)
        {
            if (navigationByCode.TryGetValue(code, out var nav) && !string.IsNullOrEmpty(nav.parent_code))
            {
                AddParentCodesRecursively(navigationByCode, nav.parent_code, allowedCodes);
            }
        }
        
        // 過濾並排序：只返回允許的導航項目
        return allNavigations
            .Where(n => !string.IsNullOrEmpty(n.code) && allowedCodes.Contains(n.code))
            .OrderBy(n => n.sort_order)
            .ToList();
    }

    /// <summary>
    /// 收集用戶的導航權限代碼（從預設、角色、用戶本身）
    /// </summary>
    private HashSet<string> CollectUserPermissionCodes(string userName)
    {
        var permissionCodes = new HashSet<string> { "dashboard", "profile" };
        
        var user = permRep.QueryPermissionManagement(name: userName, type: "user");
        if (user == null) return permissionCodes;
        
        // 從角色收集權限
        if (user.roles != null)
        {
            foreach (var roleIdStr in user.roles)
            {
                if (int.TryParse(roleIdStr, out var roleId))
                {
                    var role = permRep.QueryPermissionManagement(id: roleId, type: "role");
                    if (role?.navigation_permissions != null)
                    {
                        foreach (var permission in role.navigation_permissions)
                        {
                            if (!string.IsNullOrEmpty(permission))
                                permissionCodes.Add(permission);
                        }
                    }
                }
            }
        }
        
        // 從用戶本身收集權限
        if (user.navigation_permissions != null)
        {
            foreach (var permission in user.navigation_permissions)
            {
                if (!string.IsNullOrEmpty(permission))
                    permissionCodes.Add(permission);
            }
        }
        
        return permissionCodes;
    }

    /// <summary>
    /// 遞迴添加父項目導航代碼（如果子項目有權限，父項目也必須顯示）
    /// </summary>
    private void AddParentCodesRecursively(Dictionary<string, vw_sidebar_navigation> navigationByCode, string parentCode, HashSet<string> allowedCodes)
    {
        if (string.IsNullOrEmpty(parentCode) || allowedCodes.Contains(parentCode))
            return;
        
        allowedCodes.Add(parentCode);
        
        // 繼續向上查找父項目
        if (navigationByCode.TryGetValue(parentCode, out var parent) && !string.IsNullOrEmpty(parent.parent_code))
        {
            AddParentCodesRecursively(navigationByCode, parent.parent_code, allowedCodes);
        }
    }

    #endregion

}
