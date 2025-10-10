using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

public class SidebarNavigationService(
    SidebarNavigationRepository rep,
    PermissionManagementRepository permRep)
{
    #region 統一的查詢方法

    /// <summary>
    /// 載入導航項目列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="parentCode">父項目篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <returns>導航項目列表</returns>
    public List<vw_sidebar_navigation> LoadNavigations(string? searchTerm = null, string? parentCode = null, bool? isActive = null)
        => rep.QueryNavigationList(searchTerm, parentCode, isActive);

    /// <summary>
    /// 載入單一導航項目
    /// </summary>
    /// <param name="id">導航項目ID</param>
    /// <returns>導航項目或null</returns>
    public vw_sidebar_navigation? LoadNavigationById(int id)
        => rep.QueryNavigation(id);

    /// <summary>
    /// 載入所有導航項目（用於下拉選單等）
    /// </summary>
    /// <returns>所有導航項目列表</returns>
    public List<vw_sidebar_navigation> LoadAllNavigations()
        => rep.QueryNavigationList(searchTerm: null, parentCode: null, isActive: true);

    #endregion

    #region ViewBag 設定方法

    /// <summary>
    /// 設定側欄導航統一的 ViewBag 資料
    /// </summary>
    /// <param name="viewBag">ViewBag 物件</param>
    /// <param name="id">導航項目ID</param>
    public void ViewBagModel(dynamic viewBag, int? id = null)
    {
        viewBag.Id = id;
        viewBag.Navigations = LoadNavigations(); // 扁平列表
        viewBag.Navigation = id.HasValue ? LoadNavigationById(id.Value) : null;
    }

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
    /// 載入用戶的側欄導航列表（根據用戶角色權限）
    /// </summary>
    public List<vw_sidebar_navigation> LoadUserNavigationList(string userName)
    {
        var permissions = new List<string>();
        
        // 基本權限：所有使用者都能看到儀表板和個人資料
        permissions.AddRange(new[] { "dashboard", "profile" });
        
        // 1. 先查詢用戶
        var user = permRep.QueryPermissionManagement(name: userName, type: "user");
        
        // 2. 遍歷用戶的所有角色，獲取角色權限
        if (user?.roles != null && user.roles.Any())
        {
            foreach (var roleIdStr in user.roles)
            {
                if (int.TryParse(roleIdStr, out var roleId))
                {
                    var role = permRep.QueryPermissionManagement(id: roleId, type: "role");
                    if (role?.navigation_permissions != null && role.navigation_permissions.Any())
                    {
                        permissions.AddRange(role.navigation_permissions);
                    }
                }
            }
        }
        
        // 3. 獲取用戶的直接權限（如果有特殊權限）
        if (user?.navigation_permissions != null && user.navigation_permissions.Any())
        {
            permissions.AddRange(user.navigation_permissions);
        }
        
        // 去除重複
        var userPermissionCodes = permissions.Distinct().ToList();
        
        // 從 SidebarNavigationRepository 獲取所有啟用的導航項目（包含所有層級）
        var allNavigations = rep.QueryNavigationList(searchTerm: null, parentCode: null, isActive: true);
        
        // 過濾出用戶有權限的導航項目
        var allowedNavigations = allNavigations
            .Where(n => !string.IsNullOrEmpty(n.code) && userPermissionCodes.Contains(n.code))
            .ToList();
        
        // 添加父項目和所有子項目
        var allowedCodes = new HashSet<string>(userPermissionCodes);
        
        // 1. 添加父項目（確保有權限的子項目可以顯示）
        foreach (var nav in allowedNavigations.ToList())
        {
            AddParentNavigationCodes(allNavigations, nav.parent_code, allowedCodes);
        }
        
        // 2. 添加所有子項目（遞歸）
        foreach (var nav in allowedNavigations.ToList())
        {
            AddChildNavigationCodes(allNavigations, nav.code, allowedCodes);
        }
        
        // 返回扁平列表（包含父項目和子項目）
        return allNavigations
            .Where(n => !string.IsNullOrEmpty(n.code) && allowedCodes.Contains(n.code))
            .OrderBy(n => n.sort_order)
            .ToList();
    }

    /// <summary>
    /// 遞歸添加父項目導航代碼
    /// </summary>
    private void AddParentNavigationCodes(List<vw_sidebar_navigation> allNavigations, string? parentCode, HashSet<string> allowedCodes)
    {
        if (string.IsNullOrEmpty(parentCode) || allowedCodes.Contains(parentCode))
            return;
            
        allowedCodes.Add(parentCode);
        
        // 找到父項目，繼續遞歸
        var parent = allNavigations.FirstOrDefault(n => n.code == parentCode);
        if (parent != null && !string.IsNullOrEmpty(parent.parent_code))
        {
            AddParentNavigationCodes(allNavigations, parent.parent_code, allowedCodes);
        }
    }

    /// <summary>
    /// 遞歸添加子項目導航代碼
    /// </summary>
    private void AddChildNavigationCodes(List<vw_sidebar_navigation> allNavigations, string? parentCode, HashSet<string> allowedCodes)
    {
        if (string.IsNullOrEmpty(parentCode))
            return;
        
        // 找到所有子項目
        var children = allNavigations.Where(n => n.parent_code == parentCode).ToList();
        
        foreach (var child in children)
        {
            if (!string.IsNullOrEmpty(child.code) && !allowedCodes.Contains(child.code))
            {
                allowedCodes.Add(child.code);
                
                // 繼續遞歸添加子項目的子項目
                AddChildNavigationCodes(allNavigations, child.code, allowedCodes);
            }
        }
    }

    #endregion
}
