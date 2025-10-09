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
        viewBag.NavigationTree = GetAllNavigations(); // 樹狀結構（給目錄管理頁面用）
    }

    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 創建導航項目（新增或變更）
    /// </summary>
    public sidebar_navigation CreateNavigation(sidebar_navigation form)
        => rep.InsertNavigation(form);

    /// <summary>
    /// 刪除導航項目
    /// </summary>
    public bool DeleteNavigation(int id)
        => rep.DeleteNavigation(id);

    #endregion

    #region 輔助方法

    /// <summary>
    /// 獲取用戶的側欄導航結構（包含角色權限）
    /// </summary>
    public Task<List<NavigationItem>> GetUserNavigationAsync(string userName)
    {
        var permissions = new List<string>();
        
        // 基本權限：所有使用者都能看到儀表板和個人資料
        permissions.AddRange(new[] { "dashboard", "profile" });
        
        // 從 PermissionManagementRepository 獲取用戶資料
        var user = permRep.QueryUserByName(userName);
        
        // 獲取用戶直接權限
        if (user?.navigation_permissions != null && user.navigation_permissions.Any())
        {
            permissions.AddRange(user.navigation_permissions);
        }
        
        // 獲取用戶角色權限
        if (user?.roles != null)
        {
            foreach (var roleIdStr in user.roles)
            {
                if (int.TryParse(roleIdStr, out var roleId))
                {
                    var role = permRep.QueryRoleById(roleId);
                    if (role?.navigation_permissions != null)
                    {
                        permissions.AddRange(role.navigation_permissions);
                    }
                }
            }
        }
        
        // 去除重複
        var userPermissionCodes = permissions.Distinct().ToList();
        
        // 從 SidebarNavigationRepository 獲取所有啟用的導航項目
        var allNavigations = rep.QueryNavigationList(isActive: true);
        
        // 過濾出用戶有權限的導航項目
        var allowedNavigations = allNavigations
            .Where(n => !string.IsNullOrEmpty(n.code) && userPermissionCodes.Contains(n.code))
            .ToList();
        
        // 添加父項目（確保有權限的子項目可以顯示）
        var allowedCodes = new HashSet<string>(userPermissionCodes);
        foreach (var nav in allowedNavigations.ToList())
        {
            AddParentNavigationCodes(allNavigations, nav.parent_code, allowedCodes);
        }
        
        // 重新過濾包含父項目的導航列表
        var finalNavigations = allNavigations
            .Where(n => !string.IsNullOrEmpty(n.code) && allowedCodes.Contains(n.code))
            .ToList();
        
        return Task.FromResult(BuildNavigationTree(finalNavigations.Select(ConvertToNavigationItem).ToList()));
    }

    /// <summary>
    /// 獲取所有導航項目（管理用）
    /// </summary>
    public List<NavigationItem> GetAllNavigations()
    {
        var navigationData = rep.QueryNavigationList();
        return BuildNavigationTree(navigationData.Select(ConvertToNavigationItem).ToList());
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
    /// 建立導航樹狀結構
    /// </summary>
    private static List<NavigationItem> BuildNavigationTree(List<NavigationItem> items)
    {
        var itemDict = items.ToDictionary(i => i.Code, i => i);
        var rootItems = new List<NavigationItem>();
        var orphanItems = new List<NavigationItem>(); // 暫時無法找到父項目的項目

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.ParentCode))
            {
                // 根項目
                rootItems.Add(item);
            }
            else if (itemDict.ContainsKey(item.ParentCode))
            {
                // 有父項目，添加到父項目的子項目中
                itemDict[item.ParentCode].Children.Add(item);
            }
            else
            {
                // 暫時找不到父項目，先記錄下來
                orphanItems.Add(item);
            }
        }

        // 處理孤兒項目（可能是因為父項目在後面才出現）
        foreach (var orphan in orphanItems)
        {
            if (itemDict.ContainsKey(orphan.ParentCode!))
            {
                itemDict[orphan.ParentCode!].Children.Add(orphan);
            }
            else
            {
                // 如果還是找不到父項目，就當作根項目處理
                rootItems.Add(orphan);
            }
        }

        return rootItems.OrderBy(i => i.SortOrder).ToList();
    }

    /// <summary>
    /// 轉換資料庫模型為視圖模型
    /// </summary>
    private static NavigationItem ConvertToNavigationItem(vw_sidebar_navigation nav)
    {
        return new NavigationItem
        {
            Id = nav.id ?? 0,
            Code = nav.code ?? "",
            ParentCode = nav.parent_code,
            Title = nav.title ?? "",
            Url = nav.url,
            Icon = nav.icon,
            SortOrder = nav.sort_order ?? 0,
            IsActive = nav.is_active ?? false
        };
    }


    #endregion
}

/// <summary>
/// 導航項目視圖模型
/// </summary>
public class NavigationItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? ParentCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<NavigationItem> Children { get; set; } = new();
    public bool HasChildren => Children.Any();
}

