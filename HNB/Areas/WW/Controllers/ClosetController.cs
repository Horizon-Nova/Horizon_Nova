using HNB.Areas.WW.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HNB.Areas.WW.Controllers
{
    [Area("WW")]
    public class ClosetController : Controller
    {
        private readonly IWardrobeService _wardrobeService;

        public ClosetController(IWardrobeService wardrobeService)
        {
            _wardrobeService = wardrobeService;
        }

        #region Public Methods

        public IActionResult Index()
        {
            ViewData["Title"] = "Closet";

            var model = _wardrobeService.QueryWardrobeIndexModel();
            return View(model);
        }

        #endregion
    }
}
