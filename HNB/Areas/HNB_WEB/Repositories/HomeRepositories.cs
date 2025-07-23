using HNB.Models;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.HNB_WEB.Repositories;

public class HomeRepositories
{
    private readonly HnbdataContext _dbContext;

    public HomeRepositories(HnbdataContext dbContext)
        => _dbContext = dbContext;

    #region 統一查詢
    private IQueryable<SysMenu> ValidSysMenuEntity => _dbContext.SysMenus.Where(p => p.MenuStatus == 1).OrderBy(x => x.MenuSort);
    #endregion

    /// <summary>查詢 SysMenu</summary>
    public List<SysMenu> SysMenuQuery()
        => ValidSysMenuEntity.ToList();

}
