using HNB.Areas.HNB_WEB.Repositories;
using Models.Hnbdata;

namespace HNB.Areas.HNB_WEB.Services;

/// <summary>
/// 團隊作品集服務層，負責處理頁面所需的資料組裝。
/// </summary>
public class TeamZoneService(TeamZoneRepository repository)
{
    #region 載入資料

    /// <summary>
    /// 取得專案標籤列表。
    /// </summary>
    public List<project_tag> LoadProjectTagList()
        => repository.QueryProjectTagList();

    /// <summary>
    /// 取得專案資料列表。
    /// </summary>
    public List<project> LoadProjectList()
        => repository.QueryProjectList();

    /// <summary>
    /// 取得團隊成員列表。
    /// </summary>
    public List<team_member> LoadTeamMemberList()
        => repository.QueryTeamMemberList();

    /// <summary>
    /// 取得指定專案資料。
    /// </summary>
    public project? LoadProject(int id)
        => repository.QueryProject(id);

    #endregion

    #region ViewBag 設定

    /// <summary>
    /// 組裝作品集頁面所需的 ViewBag 資料。
    /// </summary>
    public void ViewBagPortfolioModel(dynamic viewBag)
    {
        viewBag.PortfolioTabsMap = LoadProjectTagList();
        viewBag.PortfolioDataMap = LoadProjectList();
        viewBag.TeamMemberInfo = LoadTeamMemberList();
    }

    #endregion
}

