using System;
using System.Threading;
using System.Threading.Tasks;
using HNB.Areas.WW.Models;
using HNB.Areas.WW.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HNB.Areas.WW.Controllers
{
    [Area("WW")]
    public class CalendarController : Controller
    {
        private readonly IHistoryService _historyService;
        private readonly IWwAiWardrobeService _wwAiWardrobeService;
        private readonly ILogger<CalendarController> _logger;

        public CalendarController(
            IHistoryService historyService,
            IWwAiWardrobeService wwAiWardrobeService,
            ILogger<CalendarController> logger)
        {
            _historyService = historyService;
            _wwAiWardrobeService = wwAiWardrobeService;
            _logger = logger;
        }

        #region Public Methods

        public IActionResult Index()
        {
            ViewData["Title"] = "Calendar";

            var model = _historyService.QueryHistoryIndexModel(DateTime.Now);
            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(5368709120)]
        public async Task<IActionResult> SaveLookPhoto(
            [FromForm] WwAiSaveLookPhotoRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _wwAiWardrobeService.SaveLookPhotoAsync(request, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveOutfitLook(
            [FromBody] WwSaveOutfitLookRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.OutfitId) && string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "穿搭識別或名稱不可為空" });

            try
            {
                var result = await _wwAiWardrobeService.SaveOutfitLookAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveOutfitLook failed. OutfitId={OutfitId}, Name={Name}", request.OutfitId, request.Name);
                return StatusCode(500, new { message = $"儲存穿搭到 Outfit Log 失敗：{ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OutfitLooks(
            [FromQuery] string date,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(date))
                return BadRequest(new { message = "日期格式需為 yyyy-MM-dd" });

            try
            {
                var looks = await _wwAiWardrobeService.GetOutfitLooksByDateAsync(date, cancellationToken);
                return Ok(looks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutfitLooks query failed. Date={Date}", date);
                return StatusCode(500, new { message = $"讀取 Outfit Log 失敗：{ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OutfitLookDates(CancellationToken cancellationToken)
        {
            try
            {
                var dates = await _wwAiWardrobeService.GetOutfitLookDatesAsync(cancellationToken);
                return Ok(dates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutfitLookDates query failed");
                return StatusCode(500, new { message = $"讀取 Outfit Log 日期失敗：{ex.Message}" });
            }
        }

        #endregion
    }
}
