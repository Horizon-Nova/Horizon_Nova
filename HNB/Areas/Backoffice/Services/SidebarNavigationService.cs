using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

public class SidebarNavigationService(SidebarNavigationRepository repository)
{
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
    {
        return await repository.GetNavigationByIdAsync(id);
    }

    /// <summary>
    /// 根據Code獲取導航項目
    /// </summary>
    public async Task<sidebar_navigation?> GetNavigationByCodeAsync(string code)
    {
        return await repository.GetNavigationByCodeAsync(code);
    }

    /// <summary>
    /// 新增導航項目
    /// </summary>
    public async Task<sidebar_navigation> CreateNavigationAsync(CreateNavigationRequest request)
    {
        var navigation = new sidebar_navigation
        {
            code = GenerateNavigationCode(request.Title),
            title = request.Title,
            url = request.Url,
            icon = request.Icon,
            parent_code = request.ParentCode,
            sort_order = request.SortOrder,
            is_active = true,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        return await repository.CreateNavigationAsync(navigation);
    }

    /// <summary>
    /// 更新導航項目
    /// </summary>
    public async Task<sidebar_navigation> UpdateNavigationAsync(int id, UpdateNavigationRequest request)
    {
        var navigation = await repository.GetNavigationByIdAsync(id);
        if (navigation == null) throw new ArgumentException("導航項目不存在");

        navigation.title = request.Title;
        navigation.url = request.Url;
        navigation.icon = request.Icon;
        navigation.parent_code = request.ParentCode;
        navigation.sort_order = request.SortOrder;
        navigation.is_active = request.IsActive;
        navigation.updated_at = DateTime.UtcNow;

        return await repository.UpdateNavigationAsync(navigation);
    }

    /// <summary>
    /// 刪除導航項目
    /// </summary>
    public async Task<bool> DeleteNavigationAsync(int id)
    {
        return await repository.DeleteNavigationAsync(id);
    }

    /// <summary>
    /// 獲取父級導航項目
    /// </summary>
    public async Task<List<sidebar_navigation>> GetParentNavigationsAsync()
    {
        return await repository.GetParentNavigationsAsync();
    }

    /// <summary>
    /// 驗證URL格式
    /// </summary>
    public bool ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _);
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
                rootItems.Add(item);
            }
            else if (itemDict.TryGetValue(item.ParentCode, out var parent))
            {
                parent.Children.Add(item);
            }
            else
            {
                // 父項目不存在，暫時保存
                orphanItems.Add(item);
            }
        }

        // 處理孤兒項目：如果父項目不在權限範圍內，將子項目提升為根項目
        foreach (var orphan in orphanItems)
        {
            // 檢查是否有更深層的父項目存在
            var hasValidParent = false;
            var currentParentCode = orphan.ParentCode;
            
            while (!string.IsNullOrEmpty(currentParentCode))
            {
                if (itemDict.ContainsKey(currentParentCode))
                {
                    hasValidParent = true;
                    break;
                }
                
                // 尋找更上層的父項目（這需要原始資料，這裡簡化處理）
                break;
            }
            
            if (!hasValidParent)
            {
                // 將孤兒項目提升為根項目
                rootItems.Add(orphan);
            }
        }

        return rootItems.OrderBy(i => i.SortOrder).ToList();
    }

    /// <summary>
    /// 轉換視圖模型
    /// </summary>
    private static NavigationItem ConvertToNavigationItem(vw_sidebar_navigation nav)
    {
        return new NavigationItem
        {
            Id = nav.id ?? 0,
            Code = nav.code ?? "",
            Title = nav.title ?? "",
            Url = nav.url,
            Icon = nav.icon,
            ParentCode = nav.parent_code,
            SortOrder = nav.sort_order ?? 0,
            IsActive = nav.is_active ?? true
        };
    }


    /// <summary>
    /// 產生導航代碼
    /// </summary>
    private static string GenerateNavigationCode(string title)
    {
        return title.ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("管理", "_mgmt")
            .Replace("設定", "_settings")
            .Replace("權限", "permission_");
    }
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

/// <summary>
/// 建立導航項目請求
/// </summary>
public class CreateNavigationRequest
{
    public string? ParentCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// 更新導航項目請求
/// </summary>
public class UpdateNavigationRequest
{
    public string? ParentCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}