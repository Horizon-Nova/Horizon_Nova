using Microsoft.AspNetCore.Mvc;
using HNB.Areas.Backoffice.Controllers;

namespace HNB.Areas.Backoffice.Controllers;

public class ClothingAIController : BaseController
{
    // 衣櫃管理頁面
    public IActionResult Wardrobe()
        => View();

    // AI 穿搭助理頁面
    public IActionResult Assistant()
        => View();
}
