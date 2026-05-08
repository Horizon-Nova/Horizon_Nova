using HNB.Areas.Backoffice.Repositories;
using Models.HnbBackoffice;
using System.Security.Cryptography;
using System.Text;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 認證服務，負責處理用戶登入、密碼驗證等認證相關功能
/// </summary>
public class AuthService(AuthRepository authRepository)
{

    #region 統一的查詢方法

    /// <summary>
    /// 載入用戶列表
    /// </summary>
    /// <param name="isActive">啟用狀態篩選</param>
    public List<vw_permission_user> LoadUserList(bool? isActive = null)
        => authRepository.QueryUserList(isActive);

    /// <summary>
    /// 載入用戶
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <param name="usernameOrEmail">用戶名或信箱</param>
    public vw_permission_user? LoadUser(int? userId = null, string? usernameOrEmail = null)
        => authRepository.QueryUser(userId, usernameOrEmail);

    /// <summary>
    /// 載入權限用戶列表
    /// </summary>
    /// <param name="isActive">啟用狀態篩選</param>
    public List<permission_management> LoadPermissionUserList(bool? isActive = null)
        => authRepository.QueryPermissionUserList(isActive);

    /// <summary>
    /// 載入權限用戶
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <param name="username">用戶名</param>
    public permission_management? LoadPermissionUser(int? userId = null, string? username = null)
        => authRepository.QueryPermissionUser(userId, username);

    /// <summary>
    /// 載入用戶名是否存在
    /// </summary>
    public bool LoadUsernameExists(string username, int? excludeUserId = null)
        => authRepository.QueryUsernameExists(username, excludeUserId);

    /// <summary>
    /// 載入信箱是否存在
    /// </summary>
    public bool LoadEmailExists(string email, int? excludeUserId = null)
        => authRepository.QueryEmailExists(email, excludeUserId);

    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 創建權限用戶（新增或更新）
    /// </summary>
    public permission_management CreatePermissionUser(permission_management user)
        => authRepository.InsertPermissionUser(user);

    #endregion

    #region 輔助方法

    /// <summary>
    /// 驗證用戶憑證
    /// </summary>
    public vw_permission_user? ValidateUserCredentials(string username, string password)
    {
        var user = LoadUser(usernameOrEmail: username);
        if (user == null) return null;

        var isValidPassword = VerifyPassword(password, user.password_hash, user.salt);
        return isValidPassword ? user : null;
    }

    /// <summary>
    /// 處理登入邏輯
    /// </summary>
    public (bool success, string? errorMessage, vw_permission_user? user) ProcessLogin(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return (false, "請輸入帳號和密碼", null);

        var user = ValidateUserCredentials(username, password);
        if (user == null)
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

    /// <summary>
    /// 生成隨機鹽值
    /// </summary>
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
    public (string hash, string salt) HashNewPassword(string password)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);
        return (hash, salt);
    }

    #endregion
}
