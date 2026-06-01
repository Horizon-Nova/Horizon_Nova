using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class AIController : BaseController
{

    /// <summary> AI 控制台主頁面 </summary>
    public IActionResult Index() => View();

}
