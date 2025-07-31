#region GroundingDinoDetector.cs 檔案說明
// GroundingDinoDetector.cs – 完整可編譯，直接吃 Image<Rgb24>
// 依賴：Microsoft.ML.OnnxRuntime 1.17、SixLabors.ImageSharp 3.x、Tokenizers.DotNet 1.2.x
// -----------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Tokenizers.DotNet;
using HFTokenizer = Tokenizers.DotNet.Tokenizer;
using OrtSessionOptions = Microsoft.ML.OnnxRuntime.SessionOptions;

namespace HorizonNova.AI.GroundingDino;

public record Detection(float XMin, float YMin, float XMax, float YMax, float Score);

public sealed class GroundingDinoDetector : IDisposable
{
    private readonly InferenceSession _session;
    private readonly HFTokenizer _tokenizer;

    private const int ImageSize = 800;
    private static readonly float[] Mean = { 0.485f, 0.456f, 0.406f };
    private static readonly float[] Std = { 0.229f, 0.224f, 0.225f };

    private readonly float _boxThreshold;

    public GroundingDinoDetector(string onnxPath, string tokenizerJsonPath,float boxThreshold = 0.35f, bool useCuda = false)
    {
        Console.WriteLine("[Init] GroundingDINO 模型初始化中，請勿中斷或關閉程式");

        if (!File.Exists(onnxPath))
            throw new FileNotFoundException("模型檔案不存在: " + onnxPath);

        if (!File.Exists(tokenizerJsonPath))
            throw new FileNotFoundException("Tokenizer 檔案不存在: " + tokenizerJsonPath);

        var opts = new OrtSessionOptions();

        try
        {
            _session = new InferenceSession(onnxPath, opts);
            Console.WriteLine("模型載入完成");
        }
        catch (Exception ex)
        {
            Console.WriteLine("載入模型失敗: " + ex.Message);
            throw;
        }

        _tokenizer = new HFTokenizer(vocabPath: tokenizerJsonPath);
        _boxThreshold = boxThreshold;
    }

    #region 主推論介面：Detect(Image)
    public IList<Detection> Detect(Image<Rgb24> img, string prompt)
    {
        using var clone = img.Clone();
        var pixel = Preprocess(clone, out int ow, out int oh);
        var (ids, mask) = Encode(prompt);

        var tokenTypeIds = new DenseTensor<long>(Enumerable.Repeat(0L, ids.Length).ToArray(),new[] { 1, ids.Length });

        var pixelMask = new DenseTensor<long>(Enumerable.Repeat(1L, ImageSize * ImageSize).ToArray(),new[] { 1, ImageSize, ImageSize });

        var inputIds = new DenseTensor<long>(ids, new[] { 1, ids.Length });
        var attentionMask = new DenseTensor<long>(mask, new[] { 1, mask.Length });

        var inputs = new List<NamedOnnxValue>
        {
            Nv("pixel_values",   pixel),
            Nv("pixel_mask",     pixelMask),
            Nv("input_ids",      inputIds),
            Nv("attention_mask", attentionMask),
            Nv("token_type_ids", tokenTypeIds)
        };

        using var results = _session.Run(inputs);
        return Postprocess(results, ow, oh, ids.Length);
    }
    #endregion

    #region Detect(path)：支援圖片路徑輸入
    public IList<Detection> Detect(string imagePath, string prompt)
    {
        using var img = Image.Load<Rgb24>(imagePath);
        return Detect(img, prompt);
    }
    #endregion

    #region 影像前處理
    private static DenseTensor<float> Preprocess(Image<Rgb24> img, out int oW, out int oH)
    {
        oW = img.Width; oH = img.Height;
        img.Mutate(x => x.Resize(ImageSize, ImageSize));

        var ts = new DenseTensor<float>(new[] { 1, 3, ImageSize, ImageSize });
        for (int y = 0; y < ImageSize; y++)
            for (int x = 0; x < ImageSize; x++)
            {
                var p = img[x, y];
                ts[0, 0, y, x] = (p.R / 255f - Mean[0]) / Std[0];
                ts[0, 1, y, x] = (p.G / 255f - Mean[1]) / Std[1];
                ts[0, 2, y, x] = (p.B / 255f - Mean[2]) / Std[2];
            }
        return ts;
    }
    #endregion

    #region 文字編碼
    private (long[] ids, long[] mask) Encode(string prompt)
    {
        prompt = prompt.Trim().ToLowerInvariant();
        if (!prompt.EndsWith('.')) prompt += ".";

        uint[] idsU = _tokenizer.Encode(prompt);
        if (idsU.Length > 256) idsU = idsU.Take(256).ToArray();

        long[] ids = idsU.Select(u => (long)u).ToArray();
        long[] mask = Enumerable.Repeat(1L, ids.Length).ToArray();
        return (ids, mask);
    }
    #endregion

    #region 後處理
    private IList<Detection> Postprocess(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results,int oW, int oH, int txtLen)
    {
        var boxes = results.First(r => r.Name == "pred_boxes").AsTensor<float>();
        var logits = results.First(r => r.Name == "logits").AsTensor<float>();

        int n = boxes.Dimensions[0];
        var dets = new List<Detection>();
        for (int i = 0; i < n; i++)
        {
            float cx = boxes[i, 0], cy = boxes[i, 1], w = boxes[i, 2], h = boxes[i, 3];
            float xmin = (cx - w / 2) * oW, ymin = (cy - h / 2) * oH,
                  xmax = (cx + w / 2) * oW, ymax = (cy + h / 2) * oH;

            float score = 0f;
            for (int t = 0; t < txtLen; t++)
                score = Math.Max(score, logits[i, t]);

            if (score >= _boxThreshold)
                dets.Add(new Detection(xmin, ymin, xmax, ymax, score));
        }
        return Nms(dets, 0.45f);
    }
    #endregion

    #region NMS 與 IoU
    private static List<Detection> Nms(List<Detection> dets, float iouTh)
    {
        var sorted = dets.OrderByDescending(d => d.Score).ToList();
        var kept = new List<Detection>();
        while (sorted.Count > 0)
        {
            var best = sorted[0];
            kept.Add(best);
            sorted.RemoveAt(0);
            sorted.RemoveAll(d => Iou(best, d) > iouTh);
        }
        return kept;
    }

    private static float Iou(Detection a, Detection b)
    {
        float ix1 = Math.Max(a.XMin, b.XMin), iy1 = Math.Max(a.YMin, b.YMin);
        float ix2 = Math.Min(a.XMax, b.XMax), iy2 = Math.Min(a.YMax, b.YMax);
        float iw = Math.Max(0, ix2 - ix1), ih = Math.Max(0, iy2 - iy1);
        float inter = iw * ih;
        float areaA = (a.XMax - a.XMin) * (a.YMax - a.YMin);
        float areaB = (b.XMax - b.XMin) * (b.YMax - b.YMin);
        return inter / (areaA + areaB - inter + 1e-6f);
    }
    #endregion

    #region NamedOnnxValue 快捷建立
    private static NamedOnnxValue Nv(string name, DenseTensor<float> t) =>
        NamedOnnxValue.CreateFromTensor(name, t);
    private static NamedOnnxValue Nv(string name, DenseTensor<long> t) =>
        NamedOnnxValue.CreateFromTensor(name, t);
    #endregion

    public void Dispose() => _session.Dispose();
}
