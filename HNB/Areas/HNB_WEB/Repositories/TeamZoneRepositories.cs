using Microsoft.EntityFrameworkCore;
using Models.Hnbdata;

namespace HNB.Areas.HNB_WEB.Repositories;

public class TeamZoneRepositories(HnbdataDbContext hb)
{
    #region 統一的查詢來源（僅取上架且發布）
    private IQueryable<project> ValidProject => hb.projects;
    private IQueryable<project_tag> ValidProjecttag => hb.project_tags;


    #endregion

    #region Portfolio
    public List<project_tag> ProjecttagMapping()
        => ValidProjecttag?.ToList() ?? new List<project_tag>();


    #endregion


}
