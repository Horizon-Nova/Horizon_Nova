using Models.HnbHnbBackoffice;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 認證資料存取層，負責處理用戶認證相關的資料庫操作
/// </summary>
public class AuthRepository(HnbHnbBackofficeDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的用戶查詢來源
    /// </summary>
    private IQueryable<vw_permission_user> ValidUsers => db.vw_permission_users.Where(u => u.is_active == true);
    
    /// <summary>
    /// 有效的權限管理查詢來源
    /// </summary>
    private IQueryable<permission_management> ValidPermissionManagements => db.permission_managements.Where(u => u.type == "user");
    #endregion

    #region 專用查詢方法

    /// <summary>
    /// 查詢用戶列表
    /// </summary>
    /// <param name="isActive">啟用狀態篩選</param>
    public List<vw_permission_user> QueryUserList(bool? isActive = null)
        => ValidUsers
            .Where(u => !isActive.HasValue || u.is_active == isActive.Value)
            .ToList();

    /// <summary>
    /// 查詢用戶
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <param name="usernameOrEmail">用戶名或信箱</param>
    public vw_permission_user? QueryUser(int? userId = null, string? usernameOrEmail = null)
    {
        if (userId.HasValue)
            return ValidUsers.FirstOrDefault(u => u.id == userId.Value);
        
        if (!string.IsNullOrEmpty(usernameOrEmail))
            return ValidUsers.FirstOrDefault(u => u.name == usernameOrEmail || u.email == usernameOrEmail);
        
        return null;
    }

    /// <summary>
    /// 查詢權限用戶列表
    /// </summary>
    /// <param name="isActive">啟用狀態篩選</param>
    public List<permission_management> QueryPermissionUserList(bool? isActive = null)
        => ValidPermissionManagements
            .Where(u => !isActive.HasValue || u.is_active == isActive.Value)
            .ToList();

    /// <summary>
    /// 查詢權限用戶
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <param name="username">用戶名</param>
    public permission_management? QueryPermissionUser(int? userId = null, string? username = null)
    {
        if (userId.HasValue)
            return ValidPermissionManagements.FirstOrDefault(u => u.id == userId.Value);
        
        if (!string.IsNullOrEmpty(username))
            return ValidPermissionManagements.FirstOrDefault(u => u.name == username);
        
        return null;
    }

    /// <summary>
    /// 查詢用戶名是否存在
    /// </summary>
    /// <param name="username">用戶名</param>
    /// <param name="excludeUserId">排除的用戶 ID（用於更新時檢查）</param>
    public bool QueryUsernameExists(string username, int? excludeUserId = null)
    {
        var query = ValidPermissionManagements.Where(u => u.name == username);
        if (excludeUserId.HasValue)
            query = query.Where(u => u.id != excludeUserId.Value);
        return query.Any();
    }

    /// <summary>
    /// 查詢信箱是否存在
    /// </summary>
    /// <param name="email">信箱</param>
    /// <param name="excludeUserId">排除的用戶 ID（用於更新時檢查）</param>
    public bool QueryEmailExists(string email, int? excludeUserId = null)
    {
        var query = ValidPermissionManagements.Where(u => u.email == email);
        if (excludeUserId.HasValue)
            query = query.Where(u => u.id != excludeUserId.Value);
        return query.Any();
    }

    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 插入權限用戶（新增或更新）
    /// </summary>
    public permission_management InsertPermissionUser(permission_management user)
    {
        var existingEntity = db.permission_managements.Find(user.id);
        
        if (existingEntity == null)
        {
            // 新增
            user.type = "user";
            user.created_at = DateTime.Now;
            user.updated_at = null;
            db.permission_managements.Add(user);
            db.SaveChanges();
            return user;
        }
        
        // 更新
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
        existingEntity.navigation_permissions = user.navigation_permissions;
        
        // 判斷密碼是否為空，有就指定
        if (!string.IsNullOrEmpty(user.password_hash) && !string.IsNullOrEmpty(user.salt))
        {
            existingEntity.password_hash = user.password_hash;
            existingEntity.salt = user.salt;
            existingEntity.last_password_change_at = DateTime.Now;
        }
        
        // 判斷是否更新登入資訊
        if (user.last_login_at.HasValue)
        {
            existingEntity.last_login_at = user.last_login_at;
            existingEntity.last_login_ip = user.last_login_ip;
            existingEntity.login_count = user.login_count;
        }
        
        existingEntity.updated_at = DateTime.Now;
        db.SaveChanges();
        return existingEntity;
    }

    #endregion
}
