using Models.Hnbdata;

namespace HNB.Areas.HNB_WEB.Repositories;

/// <summary>
/// 團隊區域資料存取層，負責處理與資料庫相關的查詢操作
/// 提供專案、專案標籤和團隊成員的資料存取功能
/// </summary>
public class TeamZoneRepositories (HnbdataDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的專案查詢來源
    /// </summary>
    private IQueryable<project> ValidProject => db.projects;
    
    /// <summary>
    /// 有效的專案標籤查詢來源
    /// </summary>
    private IQueryable<project_tag> ValidProjecttag => db.project_tags;
    
    /// <summary>
    /// 有效的團隊成員查詢來源
    /// </summary>
    private IQueryable<team_member> ValidTeamMember => db.team_members;
    #endregion

    #region Portfolio
    /// <summary>
    /// 取得所有專案標籤清單
    /// </summary>
    /// <returns>專案標籤清單，如果查詢失敗則返回空清單</returns>
    public List<project_tag> ProjecttagMapping()
        => ValidProjecttag?.ToList() ?? new List<project_tag>();

    /// <summary>
    /// 取得所有專案清單
    /// </summary>
    /// <returns>專案清單，如果查詢失敗則返回空清單</returns>
    public List<project> ProjectMapping()
        => ValidProject?.ToList() ?? new List<project>();

    /// <summary>
    /// 根據專案ID查詢特定專案
    /// </summary>
    /// <param name="id">專案ID</param>
    /// <returns>專案物件，如果找不到則返回 null</returns>
    public project? ProjectDetailQuery(int id)
        => ValidProject.FirstOrDefault(p => p.id == id);

    /// <summary>
    /// 取得所有團隊成員清單
    /// </summary>
    /// <returns>團隊成員清單，如果查詢失敗則返回空清單</returns>
    public List<team_member> TeamMemberMapping()
        => ValidTeamMember?.ToList() ?? new List<team_member>();


    #endregion
}
