using HNB.Areas.Weather.Repositories;
using Models.HnbHnbBackoffice;
using System.Security.Cryptography;
using System.Text;

namespace HNB.Areas.Weather.Services;

/// <summary>
/// 登入服務，負責處理登入相關的業務邏輯
/// </summary>
public class LoginService(LoginRepository loginRepository)
{
    private const string ValidOrganization = "Whatever the wheather";

    /// <summary>
    /// 處理登入邏輯
    /// </summary>
    /// <param name="usernameOrEmail">用戶名或信箱</param>
    /// <param name="password">密碼</param>
    /// <returns>登入結果</returns>
    public (bool success, string? errorMessage, vw_permission_user? user) ProcessLogin(string usernameOrEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            return (false, "請輸入帳號和密碼", null);

        // 檢查組織名稱是否為 "Whatever the wheather"
        var user = loginRepository.QueryUserByOrganization(usernameOrEmail, ValidOrganization);
        if (user == null)
            return (false, "組織名稱不符合或帳號不存在", null);

        // 驗證密碼
        var isValidPassword = VerifyPassword(password, user.password_hash, user.salt);
        if (!isValidPassword)
            return (false, "帳號或密碼錯誤", null);

        return (true, null, user);
    }

    /// <summary>
    /// 驗證密碼
    /// </summary>
    private bool VerifyPassword(string password, string? hash, string? salt)
    {
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt)) return false;

        var computedHash = HashPassword(password, salt);
        return computedHash == hash;
    }

    /// <summary>
    /// 雜湊密碼
    /// </summary>
    private string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(hashedBytes);
    }
}

