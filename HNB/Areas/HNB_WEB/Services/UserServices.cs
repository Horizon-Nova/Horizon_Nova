using HNB.Areas.HNB_WEB.Enum;
using HNB.Areas.HNB_WEB.Models;
using HNB.Areas.HNB_WEB.Repositories;
using HNB.Areas.HNB_WEB.Utilities;
using Newtonsoft.Json;

namespace HNB.Areas.HNB_WEB.Services;

public class UserServices
{
    private readonly UserRepositories _repo;
    public UserServices(UserRepositories repo) => _repo = repo;

    public async Task<TData<List<ZtreeInfo>>> GetDepartmentTreeZtreeAsync()
    {
        var result = new TData<List<ZtreeInfo>>();
        result.Data = new List<ZtreeInfo>();

        var deptEntities = await _repo.GetZtreeDepartmentListAsync();

        result.Data = deptEntities.Select(d => new ZtreeInfo
        {
            id = d.Id,
            pId = d.ParentId,
            name = d.DepartmentName
        }).ToList();

        result.Tag = 1;
        return result;
    }



}
