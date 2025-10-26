using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;
using HNB.Areas.Backoffice.Core;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 權限管理資料存取層，負責處理用戶、角色、組織的資料存取功能
/// </summary>
public class PermissionManagementRepository(HnbHnbBackofficeDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的權限管理查詢來源
    /// </summary>
    public IQueryable<permission_management> ValidPermissionManagements => db.permission_managements;

    #endregion

    #region 專用查詢方法

    /// <summary>
    /// 查詢權限管理列表
    /// </summary>
    /// <param name="type">類型篩選 (user/role/organization)</param>
    /// <param name="isActive">啟用狀態篩選</param>
    public List<permission_management> QueryPermissionManagementList(string? type = null, bool? isActive = null)
        => ValidPermissionManagements
            .Where(pm =>
                (string.IsNullOrEmpty(type) || pm.type == type) &&
                (!isActive.HasValue || pm.is_active == isActive.Value)
            )
            .ToList();

    /// <summary>
    /// 查詢權限管理
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="name">名稱</param>
    /// <param name="type">類型</param>
    public permission_management? QueryPermissionManagement(int? id = null, string? name = null, string? type = null)
    {
        if (id.HasValue)
            return ValidPermissionManagements.FirstOrDefault(pm => pm.id == id.Value);

        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type))
            return ValidPermissionManagements.FirstOrDefault(pm => pm.name == name && pm.type == type);

        return null;
    }

    /// <summary>
    /// 查詢被占用的角色（檢查角色是否已被其他組織使用）
    /// </summary>
    /// <param name="roleIds">要檢查的角色ID列表</param>
    /// <param name="currentOrganizationId">當前組織ID（編輯時排除自己）</param>
    /// <returns>已被占用的角色ID和組織名稱的字典</returns>
    public Dictionary<string, string> QueryOccupiedRoles(List<string> roleIds, int currentOrganizationId = 0)
    {
        var occupiedRoles = new Dictionary<string, string>();

        var otherOrganizations = db.permission_managements
            .Where(pm => pm.type == "organization" && pm.id != currentOrganizationId && pm.roles != null)
            .ToList();

        foreach (var roleId in roleIds)
        {
            var occupyingOrg = otherOrganizations.FirstOrDefault(org => org.roles != null && org.roles.Contains(roleId));
            if (occupyingOrg != null)
            {
                occupiedRoles[roleId] = occupyingOrg.name ?? "未知組織";
            }
        }

        return occupiedRoles;
    }

    /// <summary>
    /// 查詢被占用的角色ID列表
    /// </summary>
    /// <param name="currentOrganizationId">當前組織ID（編輯時排除自己）</param>
    /// <returns>已被占用的角色ID列表</returns>
    public List<string> QueryOccupiedRoleIdList(int currentOrganizationId = 0)
        => db.permission_managements
            .AsEnumerable()
            .Where(pm => pm.type == "organization" && pm.id != currentOrganizationId && pm.roles != null)
            .SelectMany(pm => pm.roles!)
            .Distinct()
            .ToList();

    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 插入權限管理資料（新增或更新）
    /// </summary>
    public permission_management InsertPermissionManagement(permission_management data)
    {
        var existingEntity = db.permission_managements.Find(data.id);

        if (existingEntity == null)
        {
            data.created_at = DateTime.Now;
            data.updated_at = null;
            db.permission_managements.Add(data);
            db.SaveChanges();
            return data;
        }

        existingEntity.type = data.type;
        existingEntity.name = data.name;
        existingEntity.description = data.description;
        existingEntity.email = data.email;
        existingEntity.phone = data.phone;
        existingEntity.gender = data.gender;
        existingEntity.full_name = data.full_name;
        existingEntity.is_active = data.is_active;
        existingEntity.nickname = data.nickname;
        existingEntity.zodiac_sign = data.zodiac_sign;
        existingEntity.favorite_color = data.favorite_color;
        existingEntity.location = data.location;
        existingEntity.bio = data.bio;
        existingEntity.level = data.level;
        existingEntity.parent_id = data.parent_id;
        existingEntity.roles = data.roles;
        existingEntity.navigation_permissions = data.navigation_permissions;

        if (!string.IsNullOrEmpty(data.password_hash) && !string.IsNullOrEmpty(data.salt))
        {
            existingEntity.password_hash = data.password_hash;
            existingEntity.salt = data.salt;
            existingEntity.last_password_change_at = DateTime.Now;
        }

        existingEntity.updated_at = DateTime.Now;
        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 刪除權限管理資料
    /// </summary>
    public bool DeletePermissionManagement(int id)
    {
        var entity = db.permission_managements.Find(id);
        if (entity != null)
        {
            db.permission_managements.Remove(entity);
            db.SaveChanges();
            return true;
        }
        return false;
    }

    #endregion
}