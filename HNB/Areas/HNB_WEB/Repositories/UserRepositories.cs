using HNB.Areas.HNB_WEB.Services;
using HNB.Models;
using Microsoft.EntityFrameworkCore;
namespace HNB.Areas.HNB_WEB.Repositories;

public class UserRepositories
{
    private readonly HnbdataContext _dbContext;
    public UserRepositories(HnbdataContext dbContext) => _dbContext = dbContext;


    #region 統一查詢
    private IQueryable<SysDepartment> ValidSysDepartment => _dbContext.SysDepartments;
    #endregion


    public async Task<List<long>> GetZtreeDepartmentListAsync(List<SysDepartment> allDepartments, long parentId)
    {
        var result = new List<long> { parentId };

        var stack = new Stack<long>();
        stack.Push(parentId);

        while (stack.Count > 0)
        {
            var currentId = stack.Pop();

            var children = allDepartments
                .Where(d => d.ParentId == currentId)
                .Select(d => d.Id)
                .ToList();

            foreach (var childId in children)
            {
                result.Add(childId);
                stack.Push(childId);
            }
        }

        return result;
    }


}

