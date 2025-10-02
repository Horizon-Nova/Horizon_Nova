using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class BackofficeController(SidebarNavigationService sidebarService, PermissionManagementService permissionService) : BaseController(sidebarService)
{
    /// <summary>
    /// 個人資料頁面
    /// </summary>
    public IActionResult Profile()
    {
        SetActiveNavigation("/Backoffice/Backoffice/Profile");
        
        // 取得當前使用者資訊
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
        {
            return RedirectToAction("Login", "Authorize");
        }

        // 從資料庫取得使用者詳細資料
        var users = permissionService.LoadUsers();
        var currentUser = users.FirstOrDefault(u => u.username == userName);
        
        if (currentUser == null)
        {
            // 如果找不到使用者，建立一個基本的使用者物件
            currentUser = new vw_permission_user
            {
                username = userName,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                full_name = User.FindFirst("FullName")?.Value ?? userName
            };
        }

        return View(currentUser);
    }

    /// <summary>
    /// 更新個人資料
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateProfile(IFormCollection form)
    {
        try
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "使用者未登入" });
            }

            var users = permissionService.LoadUsers();
            var currentUser = users.FirstOrDefault(u => u.username == userName);
            
            if (currentUser?.id == null)
            {
                return Json(new { success = false, message = "找不到使用者資料" });
            }

            var result = permissionService.SaveUser(form, "edit");
            
            return Json(new { success = result.success, message = result.message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"更新失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 變更密碼
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        try
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "新密碼與確認密碼不符" });
            }

            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "使用者未登入" });
            }

            // 這裡可以添加密碼驗證邏輯
            // TODO: 驗證當前密碼是否正確

            // 取得使用者ID並更新密碼
            var users = permissionService.LoadUsers();
            var currentUser = users.FirstOrDefault(u => u.username == userName);
            
            if (currentUser?.id == null)
            {
                return Json(new { success = false, message = "找不到使用者資料" });
            }

            // 建立表單資料來更新密碼
            var formData = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["id"] = currentUser.id.ToString(),
                ["password"] = newPassword
            });

            var result = permissionService.SaveUser(formData, "edit");
            
            return Json(new { success = result.success, message = result.success ? "密碼更新成功" : result.message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"密碼更新失敗：{ex.Message}" });
        }
    }
}
