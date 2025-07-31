using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.OnnxRuntime;          // 只有 MetaInfo 會用到
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using HorizonNova.AI.GroundingDino;
using HNB.Areas.AI.Utilities;            // 你原本的 OnnxModelInspector / TokenizerInspector
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace HNB.Controllers
{
    public class QASController : Controller
    {
        private readonly GroundingDinoDetector _dino;

        // 透過 DI 注入，Startup / Program.cs 已註冊 Singleton
        public QASController(GroundingDinoDetector dino) => _dino = dino;

        public IActionResult Index() => View();
        public IActionResult GroundingDINOTest() => View();

        /* ---------- 其他 MetaInfo ---------- */

        public JsonResult ModelMetaInfo(string modelName)
        {
            var path = Path.Combine("Areas", "AI", "Modules", "OWL-ViT", modelName);
            var info = OnnxModelInspector.GetModelInfo(path);
            return Json(info);
        }

        public JsonResult TokenizerMeta()
        {
            var path = Path.Combine("Areas", "AI", "Models", "tokenizer.json");
            var info = TokenizerInspector.GetTokenizerInfo(path);
            return Json(info);
        }

        /* ---------- Grounding DINO ---------- */

        [HttpPost]
        public JsonResult GroundingDINOModelTest(IFormFile image, string prompt)
        {
            if (image == null || string.IsNullOrWhiteSpace(prompt))
                return Json(new { error = "need image + prompt" });

            using var imgStream = image.OpenReadStream();
            using var img = Image.Load<Rgb24>(imgStream);

            var dets = _dino.Detect(img, prompt);

            var result = dets.Select(d => new {
                xmin = (int)d.XMin,
                ymin = (int)d.YMin,
                xmax = (int)d.XMax,
                ymax = (int)d.YMax,
                score = Math.Round(d.Score, 3)
            });

            return Json(result);
        }
    }
}
