using System.Security.Claims;
using Models.HnbBackoffice;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HNB.Areas.Backoffice.Core;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereWhen<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        => condition ? source.Where(predicate) : source;
}

/// <summary>
/// 使用者範圍（包含使用者資料、組織範圍、角色、權限等）
/// </summary>
public class UserScope
{
    /// <summary>
    /// 使用者資料（從 vw_permission_user 視圖取得）
    /// </summary>
    public vw_permission_user? User { get; set; }

    /// <summary>
    /// 使用者所屬的組織 ID
    /// </summary>
    public int? OrganizationId { get; set; }

    /// <summary>
    /// 可存取的所有組織 ID 列表（包含自身組織及所有子孫組織）
    /// </summary>
    public List<int> ScopeIds { get; set; } = new();

    /// <summary>
    /// 使用者擁有的角色 ID 列表
    /// </summary>
    public List<int> RoleIds { get; set; } = new();

    /// <summary>
    /// 使用者可存取的導航權限代碼列表（包含預設權限、角色權限及使用者個人權限）
    /// </summary>
    public List<string> NavigationPermissions { get; set; } = new();
}

public class OrganizationScope(HnbBackofficeDbContext db)
{
    #region 公開方法

    /// <summary>
    /// 解析使用者範圍（核心方法）
    /// </summary>
    /// <param name="user">ClaimsPrincipal（可選，用於測試）</param>
    /// <returns>使用者範圍</returns>
    public UserScope ResolveUserScope(ClaimsPrincipal? user = null)
    {
        var scope = new UserScope();

        if (user?.Identity?.Name == null) return scope;

        var userName = user.Identity.Name;
        scope.User = db.vw_permission_users.FirstOrDefault(u => u.name == userName && u.type == "user");

        if (scope.User == null) return scope;

        scope.OrganizationId = scope.User.organization_id ?? GetOrganizationId(user);

        if (scope.OrganizationId.HasValue)
            scope.ScopeIds = ResolveDescendants(scope.OrganizationId.Value);

        if (scope.User.roles != null)
        {
            foreach (var roleIdStr in scope.User.roles)
            {
                if (int.TryParse(roleIdStr, out var roleId))
                    scope.RoleIds.Add(roleId);
            }
        }

        scope.NavigationPermissions = new HashSet<string> { "dashboard", "profile" }.ToList();

        foreach (var roleId in scope.RoleIds)
        {
            var role = db.permission_managements.FirstOrDefault(r => r.id == roleId && r.type == "role");
            if (role?.navigation_permissions != null)
            {
                foreach (var permission in role.navigation_permissions)
                {
                    if (!string.IsNullOrEmpty(permission) && !scope.NavigationPermissions.Contains(permission))
                        scope.NavigationPermissions.Add(permission);
                }
            }
        }

        var userPm = db.permission_managements.FirstOrDefault(u => u.id == scope.User.id && u.type == "user");
        if (userPm?.navigation_permissions != null)
        {
            foreach (var permission in userPm.navigation_permissions)
            {
                if (!string.IsNullOrEmpty(permission) && !scope.NavigationPermissions.Contains(permission))
                    scope.NavigationPermissions.Add(permission);
            }
        }

        return scope;
    }

    #endregion

    #region 私有輔助方法

    /// <summary>
    /// 解析組織階層（包含 root 與所有子孫組織）
    /// </summary>
    private static List<int> ResolveHierarchy(int rootId, IEnumerable<vw_permission_organization> organizations)
    {
        var result = new List<int> { rootId };
        var byParent = organizations
            .Where(o => o.id.HasValue)
            .GroupBy(o => o.parent_id ?? -1)
            .ToDictionary(g => g.Key, g => g.ToList());

        var stack = new Stack<int>();
        stack.Push(rootId);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!byParent.TryGetValue(current, out var children)) continue;
            foreach (var child in children)
            {
                if (!child.id.HasValue) continue;
                var childId = child.id.Value;
                if (result.Contains(childId)) continue;
                result.Add(childId);
                stack.Push(childId);
            }
        }
        return result;
    }

    /// <summary>
    /// 從 Claims 取得組織 ID
    /// </summary>
    private static int? GetOrganizationId(ClaimsPrincipal? user)
        => user != null && int.TryParse(user.FindFirst("OrganizationId")?.Value, out var orgId) ? orgId : null;

    /// <summary>
    /// 解析子孫組織 ID 列表
    /// </summary>
    private List<int> ResolveDescendants(int rootId)
    {
        var all = db.vw_permission_organizations.AsNoTracking().ToList();
        return ResolveHierarchy(rootId, all);
    }

    #endregion
}