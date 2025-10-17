using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class PermissionManagementController(PermissionManagementService sev, AuthService authService) : BaseController
{
    public IActionResult Users()
    {
        sev.ViewBagModel(ViewBag);
        return View();
    }

    public IActionResult Roles()
    {
        sev.ViewBagModel(ViewBag);
        return View();
    }

    public IActionResult Organizations()
    {
        sev.ViewBagModel(ViewBag);
        return View();
    }

    public IActionResult OrganizationChart()
    {
        sev.ViewBagModel(ViewBag);
        return View();
    }

    public IActionResult LoadDetail(int? id = null)
    {
        sev.ViewBagModel(ViewBag, id);
        return PartialView("_Detail");
    }

    [HttpPost]
    public IActionResult Delete(int id) 
        => Json(new { success = sev.Delete(id), message = sev.Delete(id) ? "刪除成功" : "刪除失敗" });

    [HttpPost]
    public IActionResult SubmitUser(permission_management form, int[] role_ids) 
    {
        form.type = "user";
        
        if (role_ids != null && role_ids.Length > 0)
        {
            form.roles = role_ids.Select(id => id.ToString()).ToList();
        }
        else
        {
            form.roles = new List<string>();
        }
        
        if (!string.IsNullOrEmpty(form.password_hash))
        {
            var (hash, salt) = authService.HashNewPassword(form.password_hash);
            form.password_hash = hash;
            form.salt = salt;
        }
        
        return Json(new { success = sev.CreateUser(form).success, message = sev.CreateUser(form).message });
    }

    [HttpPost]
    public IActionResult SubmitRole(permission_management form, string[] navigation_permissions) 
    {
        form.type = "role";
        
        if (navigation_permissions != null)
        {
            form.navigation_permissions = navigation_permissions.ToList();
        }
        else
        {
            form.navigation_permissions = new List<string>();
        }
        
        return Json(new { success = sev.CreateRole(form).success, message = sev.CreateRole(form).message });
    }

    [HttpPost]
    public IActionResult SubmitOrganization(permission_management form, int[] assigned_roles) 
    {
        form.type = "organization";
        
        if (assigned_roles != null && assigned_roles.Length > 0)
        {
            form.roles = assigned_roles.Select(id => id.ToString()).ToList();
        }
        else
        {
            form.roles = new List<string>();
        }
        
        return Json(new { success = sev.CreateOrganization(form).success, message = sev.CreateOrganization(form).message });
    }


}
