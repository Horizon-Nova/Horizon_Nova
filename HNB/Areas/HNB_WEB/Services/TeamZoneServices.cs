using HNB.Areas.HNB_WEB.Repositories;
using Models.Hnbdata;

namespace HNB.Areas.HNB_WEB.Services;

/// <summary>
/// 團隊區域服務類別，負責處理團隊相關的業務邏輯
/// 提供作品集資料載入和專案詳情查詢功能
/// </summary>
public class TeamZoneServices(TeamZoneRepositories res)
{
    /// <summary>
    /// 載入作品集相關資料到 ViewBag
    /// 包含專案標籤、專案資料和團隊成員資訊
    /// </summary>
    /// <param name="viewBag">動態 ViewBag 物件，用於傳遞資料到視圖</param>
    public void ViewBagModelPortfolio(dynamic viewBag)
    {
        var TBMap = res.ProjecttagMapping();
        var ProjectMap = res.ProjectMapping();
        var TeamMemberMap = res.TeamMemberMapping();

        viewBag.PortfolioTabsMap = TBMap;
        viewBag.TeamMemberInfo = TeamMemberMap;
        viewBag.PortfolioDataMap = ProjectMap;
    }

    /// <summary>
    /// 根據專案ID查詢專案詳細資料
    /// </summary>
    /// <param name="id">專案ID</param>
    /// <returns>專案物件，如果找不到則返回 null</returns>
    public project? ProjectDetailData(int id)
        => res.ProjectDetailQuery(id);

}
