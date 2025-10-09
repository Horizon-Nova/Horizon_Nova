using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Repositories;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class BackofficeController(PermissionManagementService permissionService, AuthService authService, AuthRepository authRepository) : BaseController
{
    /// <summary>
    /// 個人資料頁面
    /// </summary>
    public IActionResult Profile()
    {
        
        var currentUserName = User.Identity?.Name;
        if (string.IsNullOrEmpty(currentUserName))
            return RedirectToAction("Login", "Authorize");

        var users = permissionService.LoadUsers();
        var currentUser = users.FirstOrDefault(u => u.name == currentUserName);
        
        if (currentUser == null)
        {
            currentUser = new vw_permission_user
            {
                name = currentUserName,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                full_name = User.FindFirst("FullName")?.Value ?? currentUserName
            };
        }

        return View(currentUser);
    }

    /// <summary>
    /// 更新個人資料
    /// </summary>
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult UpdateProfile(permission_management form)
    {
        try
        {
            var currentUserName = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserName))
                return Json(new { success = false, message = "使用者未登入" });

            var users = permissionService.LoadUsers();
            var currentUser = users.FirstOrDefault(u => u.name == currentUserName);
            
            if (currentUser?.id == null)
                return Json(new { success = false, message = "找不到使用者資料" });

            form.id = currentUser.id.Value;
            var result = permissionService.CreateUser(form);
            
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
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        try
        {
            // 驗證輸入
            if (string.IsNullOrEmpty(currentPassword))
                return Json(new { success = false, message = "請輸入目前密碼" });

            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
                return Json(new { success = false, message = "新密碼與確認密碼不符" });

            if (newPassword.Length < 6)
                return Json(new { success = false, message = "新密碼長度至少需要6個字元" });

            var currentUserName = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserName))
                return Json(new { success = false, message = "使用者未登入" });

            var users = permissionService.LoadUsers();
            var currentUser = users.FirstOrDefault(u => u.name == currentUserName);
            
            if (currentUser?.id == null)
                return Json(new { success = false, message = "找不到使用者資料" });

            // 驗證當前密碼是否正確
            var validUser = await authService.ValidateUserAsync(currentUserName, currentPassword);
            if (validUser == null)
                return Json(new { success = false, message = "目前密碼不正確" });

            // 生成新密碼的雜湊值和鹽值
            var (newHash, newSalt) = await authService.HashPasswordAsync(newPassword);

            // 更新密碼
            await authRepository.UpdatePasswordAsync(currentUser.id.Value, newHash, newSalt);
            
            return Json(new { success = true, message = "密碼更新成功" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"密碼更新失敗：{ex.Message}" });
        }
    }
}
