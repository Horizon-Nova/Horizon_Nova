using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 權限管理資料存取層，負責處理用戶、角色、組織的資料存取功能
/// </summary>
public class PermissionManagementRepository(HnbHnbBackofficeDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的用戶查詢來源
    /// </summary>
    private IQueryable<vw_permission_user> ValidUsers => db.vw_permission_users.Where(u => u.type == "user").OrderBy(u => u.full_name);
    
    /// <summary>
    /// 有效的角色查詢來源
    /// </summary>
    private IQueryable<vw_permission_role> ValidRoles => db.vw_permission_roles.OrderBy(r => r.name);
    
    /// <summary>
    /// 有效的組織查詢來源
    /// </summary>
    private IQueryable<vw_permission_organization> ValidOrganizations => db.vw_permission_organizations.OrderBy(o => o.name);
    
    /// <summary>
    /// 有效的權限管理查詢來源
    /// </summary>
    public IQueryable<permission_management> ValidPermissionManagements => db.permission_managements;
    
    #endregion

    #region 專用查詢方法
    /// <summary>
    /// 查詢用戶列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="organization">組織篩選</param>
    /// <param name="role">角色篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <returns>用戶列表</returns>
    public List<vw_permission_user> QueryUserList(int? id, string? searchTerm = null,string? organization = null,string? role = null,bool? isActive = null)
        => ValidUsers
            .Where(u =>
                (string.IsNullOrEmpty(searchTerm) || 
                    (u.id == id) ||
                    (u.full_name != null && u.full_name.Contains(searchTerm)) ||
                    (u.username != null && u.username.Contains(searchTerm)) ||
                    (u.email != null && u.email.Contains(searchTerm))) &&
                (string.IsNullOrEmpty(organization) || u.organization_name == organization) &&
                (string.IsNullOrEmpty(role) || u.role_name == role) &&
                (!isActive.HasValue || u.is_active == isActive.Value)
            )
            .ToList();

    /// <summary>
    /// 查詢單一用戶
    /// </summary>
    /// <param name="id">用戶ID</param>
    /// <returns>用戶或null</returns>
    public vw_permission_user? QueryUser(int id)
        => ValidUsers.FirstOrDefault(u => u.id == id);

    /// <summary>
    /// 查詢角色列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="organization">組織篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <returns>角色列表</returns>
    public List<vw_permission_role> QueryRoleList(string? searchTerm = null,string? organization = null,bool? isActive = null)
        => ValidRoles
            .Where(r =>
                (string.IsNullOrEmpty(searchTerm) || 
                    (r.name != null && r.name.Contains(searchTerm)) ||
                    (r.description != null && r.description.Contains(searchTerm))) &&
                (string.IsNullOrEmpty(organization) || r.organization_name == organization) &&
                (!isActive.HasValue || r.is_active == isActive.Value)
            )
            .ToList();

    /// <summary>
    /// 查詢單一角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>角色或null</returns>
    public vw_permission_role? QueryRole(int id)
        => ValidRoles.FirstOrDefault(r => r.id == id);

    /// <summary>
    /// 查詢組織列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="level">層級篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    /// <returns>組織列表</returns>
    public List<vw_permission_organization> QueryOrganizationList(string? searchTerm = null,int? level = null,bool? isActive = null)
        => ValidOrganizations
            .Where(o =>
                (string.IsNullOrEmpty(searchTerm) || 
                    (o.name != null && o.name.Contains(searchTerm)) ||
                    (o.description != null && o.description.Contains(searchTerm))) &&
                (!level.HasValue || o.level == level.Value) &&
                (!isActive.HasValue || o.is_active == isActive.Value)
            )
            .ToList();

    /// <summary>
    /// 查詢單一組織
    /// </summary>
    /// <param name="id">組織ID</param>
    /// <returns>組織或null</returns>
    public vw_permission_organization? QueryOrganization(int id)
        => ValidOrganizations.FirstOrDefault(o => o.id == id);

    #endregion

    #region 基本 CRUD 操作
    /// <summary>
    /// 插入用戶資料（新增或更新）
    /// </summary>
    public permission_management InsertUser(permission_management user)
    {
        var existingEntity = db.permission_managements.Find(user.id);
        if (existingEntity == null)
        {
            db.Add(user);
            existingEntity = user;
        }

        existingEntity.type = user.type;
        existingEntity.name = user.name;
        existingEntity.email = user.email;
        existingEntity.phone = user.phone;
        existingEntity.gender = user.gender;
        existingEntity.full_name = user.full_name;
        existingEntity.is_active = user.is_active;
        existingEntity.nickname = user.nickname;
        existingEntity.zodiac_sign = user.zodiac_sign;
        existingEntity.favorite_color = user.favorite_color;
        existingEntity.location = user.location;
        existingEntity.bio = user.bio;
        existingEntity.parent_id = user.parent_id;
        existingEntity.roles = user.roles;
        
        if (!string.IsNullOrEmpty(user.password_hash))
        {
            existingEntity.password_hash = user.password_hash;
            existingEntity.salt = user.salt;
        }

        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 插入角色資料（新增或更新）
    /// </summary>
    public permission_management InsertRole(permission_management role)
    {
        var existingEntity = db.permission_managements.Find(role.id);
        if (existingEntity == null)
        {
            db.Add(role);
            existingEntity = role;
        }

        existingEntity.type = role.type;
        existingEntity.name = role.name;
        existingEntity.description = role.description;
        existingEntity.is_active = role.is_active;
        existingEntity.parent_id = role.parent_id;
        existingEntity.navigation_permissions = role.navigation_permissions;

        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 插入組織資料（新增或更新）
    /// </summary>
    public permission_management InsertOrganization(permission_management organization)
    {
        var existingEntity = db.permission_managements.Find(organization.id);
        if (existingEntity == null)
        {
            db.Add(organization);
            existingEntity = organization;
        }

        existingEntity.type = organization.type;
        existingEntity.name = organization.name;
        existingEntity.description = organization.description;
        existingEntity.level = organization.level;
        existingEntity.parent_id = organization.parent_id;
        existingEntity.is_active = organization.is_active;
        existingEntity.roles = organization.roles;

        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 刪除權限管理資料
    /// </summary>
    public bool Delete(int id)
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