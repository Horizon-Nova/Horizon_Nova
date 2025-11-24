using Models.HnbHnbBackoffice;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.Weather.Repositories;

/// <summary>
/// 登入資料存取層，負責處理登入相關的資料庫操作
/// </summary>
public class LoginRepository(HnbHnbBackofficeDbContext db)
{
    /// <summary>
    /// 查詢用戶的組織名稱是否為指定值
    /// </summary>
    /// <param name="usernameOrEmail">用戶名或信箱</param>
    /// <param name="organizationName">組織名稱</param>
    /// <returns>如果組織名稱匹配則返回用戶，否則返回 null</returns>
    public vw_permission_user? QueryUserByOrganization(string usernameOrEmail, string organizationName)
    {
        var user = db.vw_permission_users
            .Where(u => u.is_active == true)
            .FirstOrDefault(u => u.name == usernameOrEmail || u.email == usernameOrEmail);
        
        if (user == null) return null;
        
        // 檢查組織名稱是否匹配
        if (string.IsNullOrEmpty(user.organization_name) || 
            !user.organization_name.Equals(organizationName, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        
        return user;
    }

    /// <summary>
    /// 根據用戶 ID 查詢用戶資訊
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <returns>用戶資訊，如果不存在則返回 null</returns>
    public vw_permission_user? QueryUserById(int userId)
    {
        return db.vw_permission_users
            .Where(u => u.is_active == true && u.id == userId)
            .FirstOrDefault();
    }
}

