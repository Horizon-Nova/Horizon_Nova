using HNB.Areas.HNB_WEB.Repositories;
using Models.HnbWeb;

namespace HNB.Areas.HNB_WEB.Services;

/// <summary>
/// 團隊作品集服務層，負責處理頁面所需的資料組裝。
/// </summary>
public class TeamZoneService(TeamZoneRepository repository)
{
    #region 統一的查詢方法

    /// <summary>
    /// 載入專案標籤列表
    /// </summary>
    public List<project_tag> LoadProjectTagList()
        => repository.QueryProjectTagList();

    /// <summary>
    /// 載入專案列表
    /// </summary>
    public List<project> LoadProjectList()
        => repository.QueryProjectList();

    /// <summary>
    /// 載入團隊成員列表
    /// </summary>
    public List<team_member> LoadTeamMemberList()
        => repository.QueryTeamMemberList();

    /// <summary>
    /// 載入指定專案
    /// </summary>
    /// <param name="id">專案 ID</param>
    public project? LoadProject(int? id = null)
        => repository.QueryProject(id);

    #endregion

    #region ViewBag 設定方法

    /// <summary>
    /// 設定作品集頁面的 ViewBag 資料
    /// </summary>
    public void ViewBagPortfolioModel(dynamic viewBag)
    {
        viewBag.PortfolioTabsMap = LoadProjectTagList();
        viewBag.PortfolioDataMap = LoadProjectList();
        viewBag.TeamMemberInfo = LoadTeamMemberList();
    }

    #endregion
}

