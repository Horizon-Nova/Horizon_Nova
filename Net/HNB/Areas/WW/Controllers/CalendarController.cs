using System;
using HNB.Areas.WW.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.WW.Controllers
{
    [Area("WW")]
    public class CalendarController : Controller
    {
        private readonly IHistoryService _historyService;

        public CalendarController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        #region Public Methods

        public IActionResult Index()
        {
            ViewData["Title"] = "Calendar";

            var model = _historyService.QueryHistoryIndexModel(DateTime.Now);
            return View(model);
        }

        #endregion
    }
}
