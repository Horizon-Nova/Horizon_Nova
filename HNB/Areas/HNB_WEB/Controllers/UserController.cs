using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;

namespace HNB.Areas.HNB_WEB.Controllers
{
    [Area("HNB_WEB")]
    public class UserController : Controller
    {
        private readonly HNB_WEB.Services.UserServices _userServices;
        public UserController(HNB_WEB.Services.UserServices userServices)
        {
            _userServices = userServices;
        }

        public IActionResult UserIndex()
        {
            _userServices.ViewBagModelUser(ViewBag);
            return View();
        }

        //public IActionResult UserForm()
        //{
        //    return View();
        //}

        //public IActionResult UserDetail()
        //{
        //    ViewBag.Ip = NetHelper.Ip;
        //    return View();
        //}

        //public IActionResult ResetPassword()
        //{
        //    return View();
        //}

        //public async Task<IActionResult> ChangePassword()
        //{
        //    ViewBag.OperatorInfo = await Operator.Instance.Current();
        //    return View();
        //}

        //public IActionResult ChangeUser()
        //{
        //    return View();
        //}

        //public async Task<IActionResult> UserPortrait()
        //{
        //    ViewBag.OperatorInfo = await Operator.Instance.Current();
        //    return View();
        //}

        public IActionResult UserImport()
        {
            return View();
        }

    }
}
