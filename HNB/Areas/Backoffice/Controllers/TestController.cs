using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.Backoffice.Core;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;
using static HNB.Areas.Backoffice.Utilities.DirectoryManagerUtilities;

namespace HNB.Areas.Backoffice.Controllers;

/// <summary>
/// 測試控制器
/// </summary>
[Area("Backoffice")]
public class TestController(OrganizationScope organizationScope) : Controller
{
    /// <summary>
    /// 測試解析使用者範圍
    /// </summary>
    public IActionResult ResolveUserScope()
    {
        var scope = organizationScope.ResolveUserScope(User);
        
        return Json(new
        {
            success = true,
            data = new
            {
                user = scope.User != null ? new
                {
                    id = scope.User.id,
                    name = scope.User.name,
                    email = scope.User.email,
                    full_name = scope.User.full_name,
                    organization_id = scope.User.organization_id,
                    organization_name = scope.User.organization_name,
                    roles = scope.User.roles
                } : null,
                organizationId = scope.OrganizationId,
                scopeIds = scope.ScopeIds,
                roleIds = scope.RoleIds,
                navigationPermissions = scope.NavigationPermissions
            }
        });
    }
}

