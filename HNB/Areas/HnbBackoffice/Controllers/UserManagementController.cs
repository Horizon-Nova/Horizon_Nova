using HNB.Areas.HnbBackoffice.Filters;
using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
[OperationPermission(requireIpMatch: true, verifyDb: true)]
public class UserManagementController(UserManagementService svc) : Controller
{
    #region 主畫面
    public IActionResult UserManagement()
        => View();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SubmitUserManagement(user_profile model, IFormFile? avatar)
    {
        svc.SaveUserProfile(model, avatar);
        return Json(new { ok = true, message = "使用者資料已儲存。" });
    }

    public IActionResult UserManagementTab(string t = "users")
    {
        var viewName = NormalizeTab(t);
        return View($"{viewName}");
    }

    private static string NormalizeTab(string? t)
    {
        t = (t ?? "users").ToLowerInvariant();
        return t switch
        {
            "users" => "Users",
            "usersdialog" => "UsersDialog",
            "organizations" => "Organizations",
            "org-roles" => "OrgRoles",
            "org-chart" => "OrgChart",
            _ => "Users"
        };
    }

    #endregion

    #region users (用戶管理)

    public IActionResult UsersResult(person_relation_v model)
    {
        var data = svc.QueryUsersResult(model);
        return View("UsersResult", data);
    }

    #endregion

}
