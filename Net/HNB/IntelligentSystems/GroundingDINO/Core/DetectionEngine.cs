using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using HNB.IntelligentSystems.GroundingDINO.Configuration;
using HNB.IntelligentSystems.GroundingDINO.Models;
using HNB.IntelligentSystems.GroundingDINO.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HNB.IntelligentSystems.GroundingDINO.Core
{
    /// <summary>
    /// Main detection engine using GroundingDINO ONNX model.
    /// </summary>
    public class DetectionEngine : IDisposable
    {
        private readonly InferenceSession session;
        private readonly TextTokenizer tokenizer;
        private readonly GroundingDINOConfig config;
        private readonly int[] targetSize;
        private const int MaxTextLen = 256;

        // 對應 ONNX 模型實際的 input/output 名稱（由 onnxruntime 檢查確認）
        // Inputs : pixel_values, input_ids, token_type_ids, attention_mask, pixel_mask
        // Outputs: logits, pred_boxes
        private readonly string[] inputNames = { "pixel_values", "input_ids", "token_type_ids", "attention_mask", "pixel_mask" };
        private readonly string[] outputNames = { "logits", "pred_boxes" };

        /// <summary>
        /// Initializes a new instance of the DetectionEngine.
        /// </summary>
        public DetectionEngine(GroundingDINOConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            var sessionOptions = new Microsoft.ML.OnnxRuntime.SessionOptions();
            sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_BASIC;

            string modelPath = config.ModelPath;
            if (!System.IO.File.Exists(modelPath))
                throw new System.IO.FileNotFoundException($"Model file not found: {modelPath}");

            session = new InferenceSession(modelPath, sessionOptions);

            tokenizer = new TextTokenizer();
            string vocabPath = config.VocabPath;
            if (!tokenizer.LoadVocab(vocabPath))
                throw new Exception($"Failed to load vocabulary file: {vocabPath}");

            targetSize = ImageUtils.GetModelTargetSize();
        }

        /// <summary>
        /// Detects objects in an image based on the text prompt.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <param name="textPrompt">Text prompt describing objects to detect.</param>
        /// <returns>List of detection results.</returns>
    public List<DetectionResult> Detect(Image<Rgb24> image, string textPrompt)
    {
        if (string.IsNullOrWhiteSpace(textPrompt))
            throw new ArgumentException("Text prompt cannot be null or empty.", nameof(textPrompt));

        int srcW = image.Width;
        int srcH = image.Height;

        // 1. 圖片前處理 → pixel_values [1, 3, H, W]（H=W=800）
        float[] imgData = ImageUtils.PreprocessForModel(image);
        var imgTensor = new DenseTensor<float>(imgData, new[] { 1, 3, targetSize[1], targetSize[0] });

        // 2. pixel_mask：全1，形狀 [1, H, W]，代表所有像素均有效（int64）
        int maskH = targetSize[1];
        int maskW = targetSize[0];
        long[] pixelMaskData = new long[maskH * maskW];
        Array.Fill(pixelMaskData, 1L);
        var pixelMaskTensor = new DenseTensor<long>(pixelMaskData, new[] { 1, maskH, maskW });

        // 3. 文字 tokenize（caption 結尾需有 "."，tokenizer 內部會再處理一次，不影響結果）
        string caption = textPrompt.Trim().ToLower();
        if (!caption.EndsWith("."))
            caption += " .";

        // _（specialTokens）在此版本模型不需要，忽略
        var (inputIds, tokenTypeIds, attentionMaskBool, _) = tokenizer.TokenizeText(caption, MaxTextLen);

        int seqLen = inputIds.Length;
        if (seqLen > MaxTextLen)
        {
            Array.Resize(ref inputIds, MaxTextLen);
            Array.Resize(ref tokenTypeIds, MaxTextLen);
            Array.Resize(ref attentionMaskBool, MaxTextLen);
            seqLen = MaxTextLen;
        }

        // 4. attention_mask 模型期望 int64（1/0），從 bool[] 轉換
        long[] attentionMaskLong = new long[seqLen];
        for (int i = 0; i < seqLen; i++)
            attentionMaskLong[i] = attentionMaskBool[i] ? 1L : 0L;

        // 5. 組裝 inputs，名稱與型別完全對應模型 metadata
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("pixel_values",   imgTensor),
            NamedOnnxValue.CreateFromTensor("input_ids",      new DenseTensor<long>(inputIds,          new[] { 1, seqLen })),
            NamedOnnxValue.CreateFromTensor("token_type_ids", new DenseTensor<long>(tokenTypeIds,      new[] { 1, seqLen })),
            NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(attentionMaskLong, new[] { 1, seqLen })),
            NamedOnnxValue.CreateFromTensor("pixel_mask",     pixelMaskTensor),
        };

        // 6. 執行推理，output name 為 pred_boxes（非 boxes）
        using (var results = session.Run(inputs))
        {
            var logitsTensor = results.FirstOrDefault(r => r.Name == "logits")?.Value as DenseTensor<float>;
            var boxesTensor  = results.FirstOrDefault(r => r.Name == "pred_boxes")?.Value as DenseTensor<float>;

            if (logitsTensor == null || boxesTensor == null)
                throw new Exception("模型輸出無效：缺少 logits 或 pred_boxes");

            return PostProcess(logitsTensor, boxesTensor, inputIds, srcW, srcH);
        }
    }

        private List<DetectionResult> PostProcess(DenseTensor<float> logitsTensor, DenseTensor<float> boxesTensor,
            long[] inputIds, int srcW, int srcH)
        {
            var results = new List<DetectionResult>();

            int numQueries = logitsTensor.Dimensions[1];
            int numTokens = logitsTensor.Dimensions[2];

            var candidateDetections = new List<(int queryIndex, float maxScore, int bestTokenIndex, float tokenScore, long tokenId)>();

            for (int i = 0; i < numQueries; i++)
            {
                float maxScore = 0;
                for (int j = 0; j < numTokens; j++)
                {
                    float score = Sigmoid(logitsTensor[0, i, j]);
                    if (score > maxScore)
                        maxScore = score;
                }

                if (maxScore <= config.BoxThreshold)
                    continue;

                int bestTokenIndex = -1;
                float bestTokenScore = 0;

                for (int j = 1; j < numTokens - 1 && j < inputIds.Length; j++)
                {
                    float score = Sigmoid(logitsTensor[0, i, j]);
                    if (score > config.TextThreshold && score > bestTokenScore)
                    {
                        bestTokenScore = score;
                        bestTokenIndex = j;
                    }
                }

                if (bestTokenIndex >= 0 && bestTokenIndex < inputIds.Length)
                {
                    long tokenId = inputIds[bestTokenIndex];
                    candidateDetections.Add((i, maxScore, bestTokenIndex, bestTokenScore, tokenId));
                }
            }

            var filteredDetections = ApplyNMS(candidateDetections, boxesTensor, srcW, srcH, inputIds, tokenizer);

            foreach (var (queryIndex, maxScore, bestTokenIndex, tokenScore, tokenId) in filteredDetections)
            {
                float cx = boxesTensor[0, queryIndex, 0];
                float cy = boxesTensor[0, queryIndex, 1];
                float w = boxesTensor[0, queryIndex, 2];
                float h = boxesTensor[0, queryIndex, 3];

                int xmin = (int)((cx - w * 0.5f) * srcW);
                int ymin = (int)((cy - h * 0.5f) * srcH);
                int boxW = (int)(w * srcW);
                int boxH = (int)(h * srcH);

                xmin = Math.Max(0, Math.Min(srcW - 1, xmin));
                ymin = Math.Max(0, Math.Min(srcH - 1, ymin));
                boxW = Math.Max(1, Math.Min(srcW - xmin, boxW));
                boxH = Math.Max(1, Math.Min(srcH - ymin, boxH));

                string label = tokenizer.ConvertIdToToken(tokenId);
                string cleanLabel = label.StartsWith("##") ? label.Substring(2) : label;

                results.Add(new DetectionResult
                {
                    Box = new Models.Rectangle(xmin, ymin, boxW, boxH),
                    Label = config.IncludeLogits ? $"{cleanLabel}({maxScore:F2})" : cleanLabel,
                    Score = maxScore
                });
            }

            return results;
        }

        private List<(int queryIndex, float maxScore, int bestTokenIndex, float tokenScore, long tokenId)> ApplyNMS(
            List<(int queryIndex, float maxScore, int bestTokenIndex, float tokenScore, long tokenId)> candidates,
            DenseTensor<float> boxesTensor, int srcW, int srcH, long[] inputIds, TextTokenizer tokenizer)
        {
            if (candidates.Count == 0)
                return new List<(int, float, int, float, long)>();

            var grouped = candidates.GroupBy(c => c.tokenId).ToList();
            var filtered = new List<(int, float, int, float, long)>();

            const float nmsThreshold = 0.4f;

            foreach (var group in grouped)
            {
                var categoryDetections = group.OrderByDescending(c => c.maxScore).ToList();
                var kept = new List<(int, float, int, float, long)>();

                for (int i = 0; i < categoryDetections.Count; i++)
                {
                    bool shouldKeep = true;
                    var current = categoryDetections[i];

                    float cx1 = boxesTensor[0, current.queryIndex, 0];
                    float cy1 = boxesTensor[0, current.queryIndex, 1];
                    float w1 = boxesTensor[0, current.queryIndex, 2];
                    float h1 = boxesTensor[0, current.queryIndex, 3];
                    Models.Rectangle box1 = new Models.Rectangle(
                        (int)((cx1 - w1 * 0.5f) * srcW),
                        (int)((cy1 - h1 * 0.5f) * srcH),
                        (int)(w1 * srcW),
                        (int)(h1 * srcH)
                    );

                    foreach (var keptDet in kept)
                    {
                        float cx2 = boxesTensor[0, keptDet.Item1, 0];
                        float cy2 = boxesTensor[0, keptDet.Item1, 1];
                        float w2 = boxesTensor[0, keptDet.Item1, 2];
                        float h2 = boxesTensor[0, keptDet.Item1, 3];
                        Models.Rectangle box2 = new Models.Rectangle(
                            (int)((cx2 - w2 * 0.5f) * srcW),
                            (int)((cy2 - h2 * 0.5f) * srcH),
                            (int)(w2 * srcW),
                            (int)(h2 * srcH)
                        );

                        float iou = CalculateIoU(box1, box2);
                        if (iou > nmsThreshold)
                        {
                            shouldKeep = false;
                            break;
                        }
                    }

                    if (shouldKeep)
                    {
                        kept.Add(current);
                    }
                }

                filtered.AddRange(kept);
            }

            return filtered;
        }

    private static float CalculateIoU(Models.Rectangle box1, Models.Rectangle box2)
        {
            int x1 = Math.Max(box1.X, box2.X);
            int y1 = Math.Max(box1.Y, box2.Y);
            int x2 = Math.Min(box1.X + box1.Width, box2.X + box2.Width);
            int y2 = Math.Min(box1.Y + box1.Height, box2.Y + box2.Height);

            if (x2 < x1 || y2 < y1)
                return 0f;

            int intersection = (x2 - x1) * (y2 - y1);
            int area1 = box1.Width * box1.Height;
            int area2 = box2.Width * box2.Height;
            int union = area1 + area2 - intersection;

            if (union == 0)
                return 0f;

            return (float)intersection / union;
        }

        private static float Sigmoid(float x)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-x));
        }

        public void Dispose()
        {
            session?.Dispose();
        }
    }
}

