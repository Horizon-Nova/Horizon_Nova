using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

public class SidebarNavigationService(SidebarNavigationRepository repository)
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
        => repository.QueryNavigationList(searchTerm, parentCode, isActive);

    /// <summary>
    /// 載入單一導航項目
    /// </summary>
    /// <param name="id">導航項目ID</param>
    /// <returns>導航項目或null</returns>
    public vw_sidebar_navigation? LoadNavigationById(int id)
        => repository.QueryNavigation(id);

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
        viewBag.Navigations = LoadNavigations();
        viewBag.Navigation = id.HasValue ? LoadNavigationById(id.Value) : null;
        viewBag.ParentNavigations = LoadNavigations();
    }

    #endregion

    #region 輔助方法

    /// <summary>
    /// 獲取用戶的側欄導航結構（包含角色權限）
    /// </summary>
    public async Task<List<NavigationItem>> GetUserNavigationAsync(string userName)
    {
        var navigationData = await repository.GetNavigationByUserAsync(userName);
        return BuildNavigationTree(navigationData.Select(ConvertToNavigationItem).ToList());
    }

    /// <summary>
    /// 獲取所有導航項目（管理用）
    /// </summary>
    public async Task<List<NavigationItem>> GetAllNavigationsAsync()
    {
        var navigationData = await repository.GetAllNavigationsAsync();
        return BuildNavigationTree(navigationData.Select(ConvertToNavigationItem).ToList());
    }

    /// <summary>
    /// 獲取所有導航項目（同步版本）
    /// </summary>
    public List<NavigationItem> GetAllNavigations()
    {
        var navigationData = repository.GetAllNavigations();
        return BuildNavigationTree(navigationData.Select(ConvertToNavigationItem).ToList());
    }

    /// <summary>
    /// 根據ID獲取導航項目
    /// </summary>
    public async Task<sidebar_navigation?> GetNavigationByIdAsync(int id)
        => await repository.GetNavigationByIdAsync(id);

    /// <summary>
    /// 根據Code獲取導航項目
    /// </summary>
    public async Task<sidebar_navigation?> GetNavigationByCodeAsync(string code)
        => await repository.GetNavigationByCodeAsync(code);


    /// <summary>
    /// 獲取父級導航項目
    /// </summary>
    public async Task<List<sidebar_navigation>> GetParentNavigationsAsync()
        => await repository.GetParentNavigationsAsync();


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

