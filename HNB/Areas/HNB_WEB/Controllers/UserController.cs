using HNB.Areas.HNB_WEB.Repositories;
using HNB.Areas.HNB_WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;

namespace HNB.Areas.HNB_WEB.Controllers;

[Area("HNB_WEB")]
public class UserController : Controller
{
    private readonly UserServices _svc;
    public UserController(UserServices svc) => _svc = svc;

    public IActionResult UserIndex()
    {
        return View();
    }

    public async Task<IActionResult> GetDepartmentTreeListJson()
    {
        var result = _svc.GetZtreeDepartmentListAsync();
        return Json(result);
    }
}
