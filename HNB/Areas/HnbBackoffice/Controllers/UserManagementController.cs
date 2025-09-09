using HNB.Areas.HnbBackoffice.Filters;
using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
[OperationPermission(requireIpMatch: true, verifyDb: true)]
public class UserManagementController(UserManagementService svc) : Controller
{
    #region 首頁
    public IActionResult UserManagement() => View();
    #endregion

    #region 送出（Modal 儲存）
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SubmitUserManagement(user_profile model, IFormFile? avatar)
    {
        svc.SaveUserProfile(model, avatar);
        return Json(new { ok = true, message = "使用者資料已儲存。" });
    }
    #endregion

    #region 分頁載入
    public IActionResult UserManagementTab(string t = "users")
    {
        var tab = NormalizeTab(t);
        var partial = tab switch
        {
            "users" => "_UM.Users",
            "organizations" => "_UM.Organizations",
            "org-roles" => "_UM.OrgRoles",
            "org-chart" => "_UM.OrgChart",
            _ => "_UM.Users",
        };
        return PartialView(partial);
    }
    #endregion

    #region 工具
    private static string NormalizeTab(string? t)
    {
        t = (t ?? "users").ToLowerInvariant();
        return t is "users" or "organizations" or "org-roles" or "org-chart" ? t : "users";
    }
    #endregion
}
