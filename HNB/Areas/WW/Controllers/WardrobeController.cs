using HNB.Areas.WW.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HNB.Areas.WW.Controllers
{
    [Area("WW")]
    public class WardrobeController : Controller
    {
        private readonly IWardrobeService _wardrobeService;

        public WardrobeController(IWardrobeService wardrobeService)
        {
            _wardrobeService = wardrobeService;
        }

        #region Public Methods

        public IActionResult Index()
        {
            ViewData["Title"] = "衣櫃";
            ViewData["TopbarTitle"] = "衣櫃";
            ViewData["TopbarLocation"] = $"Taipei · {DateTime.Today:yyyy-MM-dd}";

            var model = _wardrobeService.QueryWardrobeIndexModel();
            return View(model);
        }

        #endregion
    }
}
