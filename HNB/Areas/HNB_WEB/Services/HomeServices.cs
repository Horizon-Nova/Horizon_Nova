using HNB.Models;
using HNB.Areas.HNB_WEB.Repositories;

namespace HNB.Areas.HNB_WEB.Services;

public class HomeServices
{
    private readonly HomeRepositories _homeRepositories;

    public HomeServices(HomeRepositories homeRepositories)
        => _homeRepositories = homeRepositories;

    /// <summary> Home ViewBag 區塊 </summary>
    public void PopulateHomeViewBag(dynamic viewBag)
    {
        viewBag.MenuList = _homeRepositories.SysMenuQuery();
    }

}
