using Models.Hnbdata;

namespace HNB.Areas.HNB_WEB.Repositories;

/// <summary>
/// 團隊作品集資料存取層，僅負責資料查詢與轉換。
/// </summary>
public class TeamZoneRepository(HnbdataDbContext db)
{
    #region 統一的查詢來源

    private IQueryable<project> ValidProjects => db.projects;

    private IQueryable<project_tag> ValidProjectTags => db.project_tags;

    private IQueryable<team_member> ValidTeamMembers => db.team_members;

    #endregion

    #region 專用查詢方法

    /// <summary>
    /// 取得專案標籤列表。
    /// </summary>
    public List<project_tag> QueryProjectTagList()
        => ValidProjectTags?.ToList() ?? new List<project_tag>();

    /// <summary>
    /// 取得專案列表。
    /// </summary>
    public List<project> QueryProjectList()
        => ValidProjects?.ToList() ?? new List<project>();

    /// <summary>
    /// 取得團隊成員列表。
    /// </summary>
    public List<team_member> QueryTeamMemberList()
        => ValidTeamMembers?.ToList() ?? new List<team_member>();

    /// <summary>
    /// 查詢指定專案。
    /// </summary>
    public project? QueryProject(int id)
        => ValidProjects.FirstOrDefault(project => project.id == id);

    #endregion
}

