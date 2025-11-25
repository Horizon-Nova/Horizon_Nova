using HNB.Areas.Weather.Repositories;
using Models.HnbBackoffice;

namespace HNB.Areas.Weather.Services;

/// <summary>
/// 個人資料服務，負責處理個人資料相關的業務邏輯
/// </summary>
public class ProfileService(LoginRepository loginRepository)
{
    /// <summary>
    /// 根據用戶 ID 獲取用戶資訊
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <returns>用戶資訊，如果不存在則返回 null</returns>
    public vw_permission_user? GetUserProfile(int userId)
    {
        return loginRepository.QueryUserById(userId);
    }
}

