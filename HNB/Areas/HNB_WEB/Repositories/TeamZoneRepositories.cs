using Microsoft.EntityFrameworkCore;
using Models.Hnbdata;

namespace HNB.Areas.HNB_WEB.Repositories;

public class TeamZoneRepositories(HnbdataDbContext hb)
{
    #region 統一的查詢來源
    private IQueryable<project> ValidProject => hb.projects;
    private IQueryable<project_tag> ValidProjecttag => hb.project_tags;
    private IQueryable<team_member> ValidTeamMember => hb.team_members;


    #endregion

    #region Portfolio
    public List<project_tag> ProjecttagMapping()
        => ValidProjecttag?.ToList() ?? new List<project_tag>();

    public List<project> ProjectMapping()
        => ValidProject?.ToList() ?? new List<project>();

    public project? ProjectDetailQuery(int id)
        => ValidProject.FirstOrDefault(p => p.id == id);

    public List<team_member> TeamMemberMapping()
        => ValidTeamMember?.ToList() ?? new List<team_member>();


    #endregion


}
