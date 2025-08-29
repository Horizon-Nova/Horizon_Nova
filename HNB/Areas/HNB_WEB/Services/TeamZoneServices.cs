using HNB.Areas.HNB_WEB.Repositories;
using System.Text.Json;
using Models.Hnbdata;

namespace HNB.Areas.HNB_WEB.Services;

public class TeamZoneServices(TeamZoneRepositories res)
{

    ///// <summary>作品清單頁</summary>
    public void ViewBagModelPortfolio(dynamic viewBag)
    {
        var TBMap = res.ProjecttagMapping();
        var ProjectMap = res.ProjectMapping();
        var TeamMemberMap = res.TeamMemberMapping();

        viewBag.PortfolioTabsMap = TBMap;
        viewBag.TeamMemberInfo = TeamMemberMap;
        viewBag.PortfolioDataMap = ProjectMap;
    }

    ///// <summary>詳細資料</summary>
    public project? ProjectDetailData(int id)
        => res.ProjectDetailQuery(id);

}
