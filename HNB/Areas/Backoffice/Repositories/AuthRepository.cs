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

    /// <summary>
    /// 根據用戶名或信箱獲取用戶資訊
    /// </summary>
    /// <param name="usernameOrEmail">用戶名或信箱</param>
    /// <returns>用戶資訊</returns>
    public async Task<vw_permission_user?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        try
        {
            return await ValidUsers
                .Where(u => u.username == usernameOrEmail || u.email == usernameOrEmail)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            // 記錄錯誤但不拋出異常
            Console.WriteLine($"獲取用戶資訊時發生錯誤: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 根據用戶 ID 獲取用戶資訊
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <returns>用戶資訊</returns>
    public async Task<vw_permission_user?> GetUserByIdAsync(int userId)
    {
        try
        {
            return await ValidUsers
                .Where(u => u.id == userId)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"根據用戶 ID 獲取用戶資訊時發生錯誤: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 更新用戶最後登入時間
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <param name="ipAddress">IP 位址</param>
    public async Task UpdateLastLoginAsync(int userId, string? ipAddress = null)
    {
        try
        {
            var user = await ValidPermissionManagements
                .Where(u => u.id == userId)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                user.last_login_at = DateTime.UtcNow;
                user.last_login_ip = ipAddress != null ? System.Net.IPAddress.Parse(ipAddress) : null;
                user.login_count = (user.login_count ?? 0) + 1;
                user.updated_at = DateTime.UtcNow;

                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新用戶最後登入時間時發生錯誤: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 更新用戶密碼
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <param name="passwordHash">密碼雜湊值</param>
    /// <param name="salt">鹽值</param>
    public async Task UpdatePasswordAsync(int userId, string passwordHash, string salt)
    {
        try
        {
            var user = await ValidPermissionManagements
                .Where(u => u.id == userId)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                user.password_hash = passwordHash;
                user.salt = salt;
                user.last_password_change_at = DateTime.UtcNow;
                user.updated_at = DateTime.UtcNow;

                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新用戶密碼時發生錯誤: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 檢查用戶名是否已存在
    /// </summary>
    /// <param name="username">用戶名</param>
    /// <param name="excludeUserId">排除的用戶 ID（用於更新時檢查）</param>
    /// <returns>是否存在</returns>
    public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
    {
        try
        {
            var query = ValidPermissionManagements
                .Where(u => u.name == username);

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.id != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"檢查用戶名是否存在時發生錯誤: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 檢查信箱是否已存在
    /// </summary>
    /// <param name="email">信箱</param>
    /// <param name="excludeUserId">排除的用戶 ID（用於更新時檢查）</param>
    /// <returns>是否存在</returns>
    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        try
        {
            var query = ValidPermissionManagements
                .Where(u => u.email == email);

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.id != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"檢查信箱是否存在時發生錯誤: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 創建新用戶
    /// </summary>
    /// <param name="user">用戶資訊</param>
    /// <returns>創建的用戶 ID</returns>
    public async Task<int> CreateUserAsync(permission_management user)
    {
        try
        {
            user.type = "user";
            user.created_at = DateTime.UtcNow;
            user.updated_at = DateTime.UtcNow;

            db.permission_managements.Add(user);
            await db.SaveChangesAsync();

            return user.id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"創建用戶時發生錯誤: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 更新用戶資訊
    /// </summary>
    /// <param name="user">用戶資訊</param>
    public async Task UpdateUserAsync(permission_management user)
    {
        try
        {
            user.updated_at = DateTime.UtcNow;
            db.permission_managements.Update(user);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新用戶資訊時發生錯誤: {ex.Message}");
            throw;
        }
    }
}
