using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.OnnxRuntime;          // 只有 MetaInfo 會用到
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;




namespace HNB.Controllers
{
    public class QASController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult GroundingDINOTest() => View();
    }
}
