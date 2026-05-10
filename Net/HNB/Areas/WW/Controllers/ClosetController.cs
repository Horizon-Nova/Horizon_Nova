using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HNB.Areas.WW.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.WW.Controllers
{
    [Area("WW")]
    public class ClosetController : Controller
    {
        private readonly IWardrobeService _wardrobeService;
        private readonly IWwAiWardrobeService _wwAiWardrobeService;

        public ClosetController(
            IWardrobeService wardrobeService,
            IWwAiWardrobeService wwAiWardrobeService)
        {
            _wardrobeService = wardrobeService;
            _wwAiWardrobeService = wwAiWardrobeService;
        }

        #region Public Methods

        public IActionResult Index()
        {
            ViewData["Title"] = "Closet";

            var model = _wardrobeService.QueryWardrobeIndexModel();
            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(5368709120)]
        public async Task<IActionResult> ImportImages(
            [FromForm(Name = "image[]")] List<IFormFile> images,
            CancellationToken cancellationToken)
        {
            var result = await _wwAiWardrobeService.ImportWardrobeImagesAsync(images, cancellationToken);

            if (!result.Success && result.Items.Count == 0)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion
    }
}
