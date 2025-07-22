using HNB.Models;
using HNB.Repositories;

namespace HNB.Services;

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
