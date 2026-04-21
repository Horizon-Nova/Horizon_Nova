using System;
using HNB.Areas.WW.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.WW.Controllers
{
    [Area("WW")]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        #region Public Methods

        public IActionResult Index()
        {
            ViewData["Title"] = "紀錄";
            ViewData["TopbarTitle"] = "紀錄";
            ViewData["TopbarLocation"] = $"Taipei · {DateTime.Today:yyyy-MM-dd}";

            var model = _historyService.QueryHistoryIndexModel(DateTime.Now);
            return View(model);
        }

        #endregion
    }
}

