using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

public class SidebarNavigationController(SidebarNavigationService sidebarService, SidebarNavigationService navigationService) : BaseController(sidebarService)
{
    private readonly SidebarNavigationService _navigationService = navigationService;

    /// <summary>
    /// 側欄導航管理首頁
    /// </summary>
    public async Task<IActionResult> SidebarNavigation()
    {
        SetActiveNavigation("/Backoffice/SidebarNavigation/SidebarNavigation");
        
        var navigations = await _navigationService.GetAllNavigationsAsync();
        return View(navigations);
    }

    /// <summary>
    /// 新增導航項目頁面
    /// </summary>
    public async Task<IActionResult> Create(int? parentId = null)
    {
        SetActiveNavigation("/Backoffice/SidebarNavigation");
        
        ViewBag.ParentId = parentId;
        ViewBag.ParentNavigations = await _navigationService.GetAllNavigationsAsync();
        
        return View();
    }

    /// <summary>
    /// 新增導航項目
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateNavigationRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ParentNavigations = await _navigationService.GetAllNavigationsAsync();
            return View(request);
        }

        // 驗證 URL 格式
        if (!string.IsNullOrEmpty(request.Url) && !_navigationService.ValidateUrl(request.Url))
        {
            ModelState.AddModelError("Url", "請輸入有效的 URL 格式");
            ViewBag.ParentNavigations = await _navigationService.GetAllNavigationsAsync();
            return View(request);
        }

        try
        {
            await _navigationService.CreateNavigationAsync(request);
            TempData["SuccessMessage"] = "導航項目新增成功！";
            return RedirectToAction(nameof(SidebarNavigation));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"新增失敗：{ex.Message}");
            ViewBag.ParentNavigations = await _navigationService.GetAllNavigationsAsync();
            return View(request);
        }
    }

    /// <summary>
    /// 編輯導航項目頁面
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        SetActiveNavigation("/Backoffice/SidebarNavigation");
        
        var navigation = await _navigationService.GetAllNavigationsAsync();
        var item = navigation.SelectMany(GetAllItems).FirstOrDefault(n => n.Id == id);
        
        if (item == null)
        {
            TempData["ErrorMessage"] = "找不到指定的導航項目";
            return RedirectToAction(nameof(SidebarNavigation));
        }

        var request = new UpdateNavigationRequest
        {
            ParentCode = item.ParentCode,
            Title = item.Title,
            Url = item.Url,
            Icon = item.Icon,
            SortOrder = item.SortOrder,
            IsActive = item.IsActive
        };

        ViewBag.NavigationId = id;
        ViewBag.ParentNavigations = await _navigationService.GetAllNavigationsAsync();
        
        return View(request);
    }

    /// <summary>
    /// 更新導航項目
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, UpdateNavigationRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.NavigationId = id;
            ViewBag.ParentNavigations = await _navigationService.GetAllNavigationsAsync();
            return View(request);
        }

        // 驗證 URL 格式
        if (!string.IsNullOrEmpty(request.Url) && !_navigationService.ValidateUrl(request.Url))
        {
            ModelState.AddModelError("Url", "請輸入有效的 URL 格式");
            ViewBag.NavigationId = id;
            ViewBag.ParentNavigations = await _navigationService.GetAllNavigationsAsync();
            return View(request);
        }

        try
        {
            var result = await _navigationService.UpdateNavigationAsync(id, request);
            if (result == null)
            {
                TempData["ErrorMessage"] = "找不到指定的導航項目";
                return RedirectToAction(nameof(SidebarNavigation));
            }

            TempData["SuccessMessage"] = "導航項目更新成功！";
            return RedirectToAction(nameof(SidebarNavigation));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"更新失敗：{ex.Message}");
            ViewBag.NavigationId = id;
            ViewBag.ParentNavigations = await _navigationService.GetAllNavigationsAsync();
            return View(request);
        }
    }

    /// <summary>
    /// 刪除導航項目
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _navigationService.DeleteNavigationAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "導航項目刪除成功！";
            }
            else
            {
                TempData["ErrorMessage"] = "找不到指定的導航項目";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"刪除失敗：{ex.Message}";
        }

        return RedirectToAction(nameof(SidebarNavigation));
    }


    /// <summary>
    /// 遞歸獲取所有導航項目（扁平化）
    /// </summary>
    private static IEnumerable<NavigationItem> GetAllItems(NavigationItem item)
    {
        yield return item;
        foreach (var child in item.Children)
        {
            foreach (var descendant in GetAllItems(child))
            {
                yield return descendant;
            }
        }
    }
}
