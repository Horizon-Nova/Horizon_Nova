using HNB.Models;
using HNB.Areas.HNB_WEB.Repositories;

namespace HNB.Areas.HNB_WEB.Services;

public class TeamZoneServices
{
    private readonly TeamZoneRepositories _TeamZoneRepositories;

    public TeamZoneServices(TeamZoneRepositories TeamZoneRepositories)
        => _TeamZoneRepositories = TeamZoneRepositories;

    /// <summary> TeamZone ViewBag 區塊 </summary>
    public void PopulateTeamZoneViewBag(dynamic viewBag)
    {
        viewBag.MenuList = _TeamZoneRepositories.SysMenuQuery();
    }

}
