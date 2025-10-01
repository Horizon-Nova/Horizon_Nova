using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;
using System.Security.Cryptography;
using System.Text;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 認證服務，負責處理用戶登入、密碼驗證等認證相關功能
/// </summary>
public class AuthService(AuthRepository authRepository)
{

    /// <summary>
    /// 驗證用戶憑證
    /// </summary>
    /// <param name="username">用戶名或信箱</param>
    /// <param name="password">密碼</param>
    /// <returns>用戶資訊，如果驗證失敗則返回 null</returns>
    public async Task<vw_permission_user?> ValidateUserAsync(string username, string password)
    {
        var user = await authRepository.GetUserByUsernameOrEmailAsync(username) ?? null;
        if (user == null) return null;

        var isValidPassword = await VerifyPasswordAsync(password, user.password_hash, user.salt);
        return isValidPassword ? user : null;
    }

    /// <summary>
    /// 處理登入邏輯
    /// </summary>
    /// <param name="username">用戶名或信箱</param>
    /// <param name="password">密碼</param>
    /// <returns>登入結果</returns>
    public async Task<(bool success, string? errorMessage, vw_permission_user? user)> ProcessLoginAsync(string username, string password)
    {
        // 檢查輸入
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "請輸入帳號和密碼", null);
        }

        // 驗證用戶
        var user = await ValidateUserAsync(username, password);
        if (user == null)
        {
            return (false, "帳號或密碼錯誤", null);
        }

        return (true, null, user);
    }

    /// <summary>
    /// 更新用戶最後登入時間
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <param name="ipAddress">IP 位址</param>
    public async Task UpdateLastLoginAsync(int userId, string? ipAddress = null)
    {
        await authRepository.UpdateLastLoginAsync(userId, ipAddress);
    }

    /// <summary>
    /// 驗證密碼
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <param name="hash">雜湊值</param>
    /// <param name="salt">鹽值</param>
    /// <returns>是否驗證成功</returns>
    private async Task<bool> VerifyPasswordAsync(string password, string? hash, string? salt)
    {
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt)) return false;

        var computedHash = await HashPasswordAsync(password, salt);
        return computedHash == hash;
    }

    /// <summary>
    /// 雜湊密碼
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <param name="salt">鹽值</param>
    /// <returns>雜湊值</returns>
    private async Task<string> HashPasswordAsync(string password, string salt)
    {
        return await Task.Run(() =>
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        });
    }

    /// <summary>
    /// 生成隨機鹽值
    /// </summary>
    /// <returns>鹽值</returns>
    public static string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    /// <summary>
    /// 雜湊密碼（用於註冊或重設密碼）
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <returns>雜湊值和鹽值</returns>
    public async Task<(string hash, string salt)> HashPasswordAsync(string password)
    {
        var salt = GenerateSalt();
        var hash = await HashPasswordAsync(password, salt);
        return (hash, salt);
    }

    /// <summary>
    /// 檢查用戶是否存在
    /// </summary>
    /// <param name="username">用戶名或信箱</param>
    /// <returns>是否存在</returns>
    public async Task<bool> UserExistsAsync(string username)
    {
        var user = await authRepository.GetUserByUsernameOrEmailAsync(username);
        return user != null;
    }

    /// <summary>
    /// 根據用戶 ID 獲取用戶資訊
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <returns>用戶資訊</returns>
    public async Task<vw_permission_user?> GetUserByIdAsync(int userId)
    {
        return await authRepository.GetUserByIdAsync(userId);
    }
}
