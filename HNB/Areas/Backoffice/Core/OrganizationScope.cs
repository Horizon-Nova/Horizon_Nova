using System.Security.Claims;
using Models.HnbHnbBackoffice;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HNB.Areas.Backoffice.Core;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereWhen<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        => condition ? source.Where(predicate) : source;
}

public static class OrganizationScope
{
    /// <summary>
    /// 計算並回傳包含 root 與所有子孫組織的 ID 集合。
    /// </summary>
    public static List<int> ResolveAccessibleOrganizationIds(int rootOrganizationId, IEnumerable<vw_permission_organization> allOrganizations)
    {
        var result = new List<int> { rootOrganizationId };
        var byParent = allOrganizations
            .Where(o => o.id.HasValue)
            .GroupBy(o => o.parent_id ?? -1)
            .ToDictionary(g => g.Key, g => g.ToList());

        var stack = new Stack<int>();
        stack.Push(rootOrganizationId);
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
    /// 從 Claims 解析目前使用者的組織 ID，無則回傳 null。
    /// </summary>
    public static int? ResolveUserOrganizationId(ClaimsPrincipal user)
        => int.TryParse(user.FindFirst("OrganizationId")?.Value, out var orgId) ? orgId : null;

    /// <summary>
    /// 自資料庫載入所有組織後，計算並回傳 root 的所有子孫組織 ID。
    /// </summary>
    public static List<int> ResolveDescendantOrganizationIds(HnbHnbBackofficeDbContext db, int rootOrganizationId)
    {
        var all = db.vw_permission_organizations.AsNoTracking().ToList();
        return ResolveAccessibleOrganizationIds(rootOrganizationId, all);
    }

    /// <summary>
    /// 回傳清單中第一個可解析為 int 的元素，否則回 null。
    /// </summary>
    private static int? ParseFirstInt(IEnumerable<string>? values)
    {
        if (values == null) return null;
        foreach (var v in values)
        {
            if (int.TryParse(v, out var n)) return n;
        }
        return null;
    }

    /// <summary>
    /// 將字串清單轉換為 int 清單（忽略無法解析的元素）。
    /// </summary>
    private static List<int> ParseIntList(IEnumerable<string>? values)
    {
        var result = new List<int>();
        if (values == null) return result;
        foreach (var v in values)
        {
            if (int.TryParse(v, out var n)) result.Add(n);
        }
        return result;
    }

    /// <summary>
    /// 由 permission_management 或 Claims 解析組織 ID；優先順序：pm.id(organization) → pm.parent_id → Claims。
    /// </summary>
    public static int? ResolveOrganizationId(permission_management? pm, ClaimsPrincipal? user = null)
    {
        if (pm == null) return user != null ? ResolveUserOrganizationId(user) : null;
        if (pm.type == "organization" && pm.id > 0) return pm.id;
        var fromParent = ParseFirstInt(pm.parent_id);
        if (fromParent.HasValue) return fromParent.Value;
        return user != null ? ResolveUserOrganizationId(user) : null;
    }

    /// <summary>
    /// 依 pm 或 Claims 決定查詢範圍並回傳角色集合；支援：單一角色、角色 ID 清單、組織範圍與條件過濾。
    /// </summary>
    public static List<vw_permission_role> ResolveRoles(
        HnbHnbBackofficeDbContext db,
        permission_management? pm = null,
        ClaimsPrincipal? user = null,
        string? searchTerm = null,
        string? organization = null,
        bool? isActive = null,
        bool includeAllWhenNoScope = false)
    {
        var roleIds = ParseIntList(pm?.roles);
        var hasDirectKey = (pm?.type == "role" && pm.id > 0) || roleIds.Count > 0;
        var orgId = ResolveOrganizationId(pm, user);
        var scopeIds = orgId.HasValue ? ResolveDescendantOrganizationIds(db, orgId.Value) : null;

        if (!includeAllWhenNoScope && !orgId.HasValue && !hasDirectKey)
            return new List<vw_permission_role>();

        return db.vw_permission_roles
            .WhereWhen(pm?.type == "role" && pm.id > 0, r => r.id == pm!.id)
            .WhereWhen(roleIds.Count > 0, r => r.id.HasValue && roleIds.Contains(r.id.Value))
            .WhereWhen(orgId.HasValue, r => r.organization_id.HasValue && scopeIds!.Contains(r.organization_id.Value))
            .WhereWhen(!string.IsNullOrWhiteSpace(searchTerm), r => (r.name != null && r.name.Contains(searchTerm!)) || (r.description != null && r.description.Contains(searchTerm!)))
            .WhereWhen(!string.IsNullOrWhiteSpace(organization), r => r.organization_name == organization)
            .WhereWhen(isActive.HasValue, r => r.is_active == isActive!.Value)
            .ToList();
    }

    /// <summary>
    /// 依 pm 或 Claims 決定查詢範圍並回傳使用者集合；支援：單一使用者、name/email、組織範圍與條件過濾。
    /// </summary>
    public static List<vw_permission_user> ResolveUsers(
        HnbHnbBackofficeDbContext db,
        permission_management? pm = null,
        ClaimsPrincipal? user = null,
        string? searchTerm = null,
        string? organization = null,
        string? role = null,
        bool? isActive = null,
        bool includeAllWhenNoScope = false)
    {
        var hasDirectKey = (pm?.type == "user" && pm.id > 0) || !string.IsNullOrWhiteSpace(pm?.name) || !string.IsNullOrWhiteSpace(pm?.email);
        var orgId = ResolveOrganizationId(pm, user);
        var scopeIds = orgId.HasValue ? ResolveDescendantOrganizationIds(db, orgId.Value) : null;

        if (!includeAllWhenNoScope && !orgId.HasValue && !hasDirectKey)
            return new List<vw_permission_user>();

        return db.vw_permission_users.Where(x => x.type == "user")
            .WhereWhen(pm?.type == "user" && pm.id > 0, x => x.id == pm!.id)
            .WhereWhen(!string.IsNullOrWhiteSpace(pm?.name), x => x.name == pm!.name)
            .WhereWhen(!string.IsNullOrWhiteSpace(pm?.email), x => x.email == pm!.email)
            .WhereWhen(orgId.HasValue, x => x.organization_id.HasValue && scopeIds!.Contains(x.organization_id.Value))
            .WhereWhen(!string.IsNullOrWhiteSpace(searchTerm), u => (u.full_name != null && u.full_name.Contains(searchTerm!)) || (u.name != null && u.name.Contains(searchTerm!)) || (u.email != null && u.email.Contains(searchTerm!)))
            .WhereWhen(!string.IsNullOrWhiteSpace(organization), u => u.organization_name == organization)
            .WhereWhen(!string.IsNullOrWhiteSpace(role), u => u.role_name == role)
            .WhereWhen(isActive.HasValue, u => u.is_active == isActive!.Value)
            .ToList();
    }

    /// <summary>
    /// 依 pm 或 Claims 決定查詢範圍並回傳組織集合；支援：單一組織含子孫、名稱清單、組織範圍與條件過濾。
    /// </summary>
    public static List<vw_permission_organization> ResolveOrganizations(
        HnbHnbBackofficeDbContext db,
        permission_management? pm = null,
        ClaimsPrincipal? user = null,
        string? searchTerm = null,
        int? level = null,
        bool? isActive = null,
        bool includeAllWhenNoScope = false)
    {
        var orgId = ResolveOrganizationId(pm, user);
        var scopeIds = orgId.HasValue ? ResolveDescendantOrganizationIds(db, orgId.Value) : null;
        var hasDirectKey = (pm?.type == "organization" && pm.id > 0) || (pm?.organization_names != null && pm.organization_names.Count > 0);
        if (!includeAllWhenNoScope && !orgId.HasValue && !hasDirectKey)
            return new List<vw_permission_organization>();

        var directIds = (pm?.type == "organization" && pm.id > 0) ? ResolveDescendantOrganizationIds(db, pm.id) : null;

        return db.vw_permission_organizations
            .WhereWhen(pm?.type == "organization" && pm.id > 0, o => o.id.HasValue && directIds!.Contains(o.id.Value))
            .WhereWhen(pm?.organization_names != null && pm.organization_names.Count > 0, o => o.organization_name != null && pm!.organization_names.Contains(o.organization_name))
            .WhereWhen(orgId.HasValue, o => o.id.HasValue && scopeIds!.Contains(o.id.Value))
            .WhereWhen(!string.IsNullOrWhiteSpace(searchTerm), o => (o.organization_name != null && o.organization_name.Contains(searchTerm!)) || (o.organization_description != null && o.organization_description.Contains(searchTerm!)))
            .WhereWhen(level.HasValue, o => o.organization_level == level!.Value)
            .WhereWhen(isActive.HasValue, o => o.is_active == isActive!.Value)
            .OrderBy(o => o.organization_name)
            .ToList();
    }

    /// <summary>
    /// 查詢單一角色。
    /// </summary>
    public static vw_permission_role? ResolveRole(HnbHnbBackofficeDbContext db, int id)
        => db.vw_permission_roles.FirstOrDefault(r => r.id == id);

    /// <summary>
    /// 查詢單一組織。
    /// </summary>
    public static vw_permission_organization? ResolveOrganization(HnbHnbBackofficeDbContext db, int id)
        => db.vw_permission_organizations.FirstOrDefault(o => o.id == id);
}