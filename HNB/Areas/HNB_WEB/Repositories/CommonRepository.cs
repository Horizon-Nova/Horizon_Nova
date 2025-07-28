using HNB.Models;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.HNB_WEB.Repositories;

/// <summary>
/// 通用資料查詢 Repository（目前支援 SysDepartment）
/// 提供部門資料的基本查詢：全部部門、部門 by ID、IQueryable 供延伸使用
/// </summary>
public class CommonRepository
{
    private readonly HnbdataContext _dbContext;

    /// <summary>
    /// 建構函式注入 DbContext
    /// </summary>
    public CommonRepository(HnbdataContext dbContext)
        => _dbContext = dbContext;

    /// <summary>
    /// 取得 SysDepartment 的 IQueryable，可自定條件查詢或組合 LINQ
    /// </summary>
    public IQueryable<SysDepartment> QueryDepartments()
        => _dbContext.Set<SysDepartment>();

    /// <summary>
    /// 查詢所有部門清單（ToList）
    /// </summary>
    public async Task<List<SysDepartment>> GetAllDepartmentsAsync()
        => await QueryDepartments().ToListAsync();

    /// <summary>
    /// 根據部門 ID 查找對應部門
    /// </summary>
    public async Task<SysDepartment> GetDepartmentByIdAsync(long id)
        => await QueryDepartments().FirstOrDefaultAsync(d => d.Id == id);
}


