using HNB.Models;
using Microsoft.EntityFrameworkCore;
namespace HNB.Areas.HNB_WEB.Repositories;

public class UserRepositories
{
    private readonly HnbdataContext _dbContext;
    public UserRepositories(HnbdataContext dbContext)
        => _dbContext = dbContext;

    #region 統一查詢
    private IQueryable<SysDepartment> ValidSysDepartment => _dbContext.SysDepartments;
    #endregion

    //public async Task<List<SysDepartment>> GetDepartmentListAsync(DepartmentListParam param = null)
    //{
    //    var query = _dbContext.SysDepartments.AsQueryable();

    //    // 過濾刪除
    //    query = query.Where(d => d.BaseIsDelete == 0);

    //    // 參數條件
    //    if (param != null)
    //    {
    //        if (!string.IsNullOrEmpty(param.DepartmentName))
    //        {
    //            query = query.Where(d => d.DepartmentName.Contains(param.DepartmentName));
    //        }

    //        if (!string.IsNullOrEmpty(param.Ids))
    //        {
    //            var idList = TextHelper.SplitToLongList(param.Ids);
    //            query = query.Where(d => idList.Contains(d.Id));
    //        }
    //    }

    //    return await query
    //        .OrderBy(d => d.DepartmentSort)
    //        .AsNoTracking()
    //        .ToListAsync();
    //}


}
