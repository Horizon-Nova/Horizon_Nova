using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using HNB.IntelligentSystems.ObjectDetection.Configuration;
using HNB.IntelligentSystems.ObjectDetection.Models;
using HNB.IntelligentSystems.ObjectDetection.Utils;
using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Core
{
    /// <summary>
    /// Main detection engine using GroundingDINO ONNX model.
    /// </summary>
    public class DetectionEngine : IDisposable
    {
        private readonly InferenceSession session;
        private readonly TextTokenizer tokenizer;
        private readonly ObjectDetectionConfig config;
        private readonly int[] targetSize;
        private const int MaxTextLen = 256;

        private readonly string[] inputNames = { "img", "input_ids", "attention_mask", "position_ids", "token_type_ids", "text_token_mask" };
        private readonly string[] outputNames = { "logits", "boxes" };

        /// <summary>
        /// Initializes a new instance of the DetectionEngine.
        /// </summary>
        public DetectionEngine(ObjectDetectionConfig config)
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
        public List<DetectionResult> Detect(Mat image, string textPrompt)
        {
            if (image == null || image.Empty())
                throw new ArgumentException("Image cannot be null or empty.", nameof(image));

            if (string.IsNullOrWhiteSpace(textPrompt))
                throw new ArgumentException("Text prompt cannot be null or empty.", nameof(textPrompt));

            int srcW = image.Width;
            int srcH = image.Height;

            float[] imgData = ImageUtils.PreprocessForModel(image);
            var imgTensor = new DenseTensor<float>(imgData, new[] { 1, 3, targetSize[1], targetSize[0] });

            string caption = textPrompt.Trim().ToLower();
            if (!caption.EndsWith("."))
                caption += " .";

            var (inputIds, tokenTypeIds, attentionMask, specialTokens) = tokenizer.TokenizeText(caption, MaxTextLen);
            var (textSelfAttentionMasks, positionIds) = tokenizer.GenerateMasksWithSpecialTokens(inputIds, specialTokens);

            int seqLen = inputIds.Length;

            if (seqLen > MaxTextLen)
            {
                Array.Resize(ref inputIds, MaxTextLen);
                Array.Resize(ref tokenTypeIds, MaxTextLen);
                Array.Resize(ref attentionMask, MaxTextLen);
                Array.Resize(ref positionIds, MaxTextLen);

                bool[,,] trimmedMasks = new bool[1, MaxTextLen, MaxTextLen];
                for (int i = 0; i < MaxTextLen; i++)
                {
                    for (int j = 0; j < MaxTextLen; j++)
                    {
                        trimmedMasks[0, i, j] = textSelfAttentionMasks[0, i, j];
                    }
                }
                textSelfAttentionMasks = trimmedMasks;
                seqLen = MaxTextLen;
            }

            bool[] textMaskFlat = new bool[seqLen * seqLen];
            for (int i = 0; i < seqLen; i++)
            {
                for (int j = 0; j < seqLen; j++)
                {
                    textMaskFlat[i * seqLen + j] = textSelfAttentionMasks[0, i, j];
                }
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("img", imgTensor),
                NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(inputIds, new[] { 1, seqLen })),
                NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<bool>(attentionMask, new[] { 1, seqLen })),
                NamedOnnxValue.CreateFromTensor("position_ids", new DenseTensor<long>(positionIds, new[] { 1, seqLen })),
                NamedOnnxValue.CreateFromTensor("token_type_ids", new DenseTensor<long>(tokenTypeIds, new[] { 1, seqLen })),
                NamedOnnxValue.CreateFromTensor("text_token_mask", new DenseTensor<bool>(textMaskFlat, new[] { 1, seqLen, seqLen }))
            };

            // ONNX Runtime InferenceSession.Run() is thread-safe and can be called concurrently
            // No lock needed - multiple threads can call Run() simultaneously
            using (var results = session.Run(inputs))
            {
                var logitsTensor = results.FirstOrDefault(r => r.Name == "logits")?.Value as DenseTensor<float>;
                var boxesTensor = results.FirstOrDefault(r => r.Name == "boxes")?.Value as DenseTensor<float>;

                if (logitsTensor == null || boxesTensor == null)
                    return new List<DetectionResult>();

                return PostProcess(logitsTensor, boxesTensor, inputIds, srcW, srcH);
            }
        }

        private List<DetectionResult> PostProcess(DenseTensor<float> logitsTensor, DenseTensor<float> boxesTensor,
            long[] inputIds, int srcW, int srcH)
        {
            var results = new List<DetectionResult>();

            int numQueries = logitsTensor.Dimensions[1];
            int numTokens = logitsTensor.Dimensions[2];

            // Store all potential detections with their scores
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

                // Check all tokens to find the best matching category
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

            // Apply Non-Maximum Suppression (NMS) to remove overlapping boxes
            // This helps with shoes (銝?? - merge nearby shoe detections
            var filteredDetections = ApplyNMS(candidateDetections, boxesTensor, srcW, srcH, inputIds, tokenizer);

            // Convert to DetectionResult
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
                    Box = new Rect(xmin, ymin, boxW, boxH),
                    Label = config.IncludeLogits ? $"{cleanLabel}({maxScore:F2})" : cleanLabel,
                    Score = maxScore
                });
            }

            return results;
        }

        /// <summary>
        /// Applies Non-Maximum Suppression with special handling for overlapping categories (jacket and clothes).
        /// </summary>
        private List<(int queryIndex, float maxScore, int bestTokenIndex, float tokenScore, long tokenId)> ApplyNMS(
            List<(int queryIndex, float maxScore, int bestTokenIndex, float tokenScore, long tokenId)> candidates,
            DenseTensor<float> boxesTensor, int srcW, int srcH, long[] inputIds, TextTokenizer tokenizer)
        {
            if (candidates.Count == 0)
                return new List<(int, float, int, float, long)>();

            // Group by category (tokenId)
            var grouped = candidates.GroupBy(c => c.tokenId).ToList();
            var filtered = new List<(int, float, int, float, long)>();

            // Get token IDs for jacket and clothes to allow them to overlap
            long? jacketTokenId = null;
            long? clothesTokenId = null;
            foreach (var group in grouped)
            {
                string categoryName = tokenizer.ConvertIdToToken(group.Key).ToLower();
                if (categoryName.Contains("jacket"))
                    jacketTokenId = group.Key;
                else if (categoryName.Contains("clothes") || categoryName.Contains("cloth"))
                    clothesTokenId = group.Key;
            }

            foreach (var group in grouped)
            {
                var categoryDetections = group.OrderByDescending(c => c.maxScore).ToList();
                string categoryName = tokenizer.ConvertIdToToken(group.Key).ToLower();
                long currentTokenId = group.Key;

                // Different NMS thresholds for different categories
                float nmsThreshold;
                if (categoryName.Contains("shoe") || categoryName == "shoes")
                {
                    // For shoes, use higher IoU threshold to merge pairs (銝??
                    nmsThreshold = 0.5f;
                }
                else if (categoryName.Contains("jacket") || categoryName.Contains("clothes") || categoryName.Contains("cloth"))
                {
                    // For clothing items, use lower threshold to allow detection of overlapping items
                    nmsThreshold = 0.6f; // Higher threshold allows more overlapping detections
                }
                else
                {
                    // Default NMS threshold
                    nmsThreshold = 0.4f;
                }

                // Apply NMS within this category
                var kept = new List<(int, float, int, float, long)>();

                for (int i = 0; i < categoryDetections.Count; i++)
                {
                    bool shouldKeep = true;
                    var current = categoryDetections[i];

                    // Get box coordinates
                    float cx1 = boxesTensor[0, current.queryIndex, 0];
                    float cy1 = boxesTensor[0, current.queryIndex, 1];
                    float w1 = boxesTensor[0, current.queryIndex, 2];
                    float h1 = boxesTensor[0, current.queryIndex, 3];
                    Rect box1 = new Rect(
                        (int)((cx1 - w1 * 0.5f) * srcW),
                        (int)((cy1 - h1 * 0.5f) * srcH),
                        (int)(w1 * srcW),
                        (int)(h1 * srcH)
                    );

                    // Check against already kept detections of the SAME category
                    foreach (var keptDet in kept)
                    {
                        float cx2 = boxesTensor[0, keptDet.Item1, 0];
                        float cy2 = boxesTensor[0, keptDet.Item1, 1];
                        float w2 = boxesTensor[0, keptDet.Item1, 2];
                        float h2 = boxesTensor[0, keptDet.Item1, 3];
                        Rect box2 = new Rect(
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

            // Allow jacket and clothes to coexist even if they overlap significantly
            // This handles the case where clothes are under jacket
            var finalFiltered = new List<(int, float, int, float, long)>();
            finalFiltered.AddRange(filtered);

            // Additional pass: ensure jacket and clothes can both be detected even if overlapping
            if (jacketTokenId.HasValue && clothesTokenId.HasValue)
            {
                var jacketDets = filtered.Where(f => f.Item5 == jacketTokenId.Value).ToList();
                var clothesDets = filtered.Where(f => f.Item5 == clothesTokenId.Value).ToList();

                // If we have both jacket and clothes, allow both even if they overlap
                // (clothes under jacket is a valid detection scenario)
                foreach (var jacketDet in jacketDets)
                {
                    float cx1 = boxesTensor[0, jacketDet.Item1, 0];
                    float cy1 = boxesTensor[0, jacketDet.Item1, 1];
                    float w1 = boxesTensor[0, jacketDet.Item1, 2];
                    float h1 = boxesTensor[0, jacketDet.Item1, 3];
                    Rect jacketBox = new Rect(
                        (int)((cx1 - w1 * 0.5f) * srcW),
                        (int)((cy1 - h1 * 0.5f) * srcH),
                        (int)(w1 * srcW),
                        (int)(h1 * srcH)
                    );

                    foreach (var clothesDet in clothesDets)
                    {
                        float cx2 = boxesTensor[0, clothesDet.Item1, 0];
                        float cy2 = boxesTensor[0, clothesDet.Item1, 1];
                        float w2 = boxesTensor[0, clothesDet.Item1, 2];
                        float h2 = boxesTensor[0, clothesDet.Item1, 3];
                        Rect clothesBox = new Rect(
                            (int)((cx2 - w2 * 0.5f) * srcW),
                            (int)((cy2 - h2 * 0.5f) * srcH),
                            (int)(w2 * srcW),
                            (int)(h2 * srcH)
                        );

                        float iou = CalculateIoU(jacketBox, clothesBox);
                        // If jacket and clothes overlap significantly (>50%), both are valid
                        // They are already in finalFiltered, so no action needed
                        // This comment explains why both are kept
                    }
                }
            }

            return finalFiltered;
        }

        /// <summary>
        /// Calculates Intersection over Union (IoU) between two bounding boxes.
        /// </summary>
        private static float CalculateIoU(Rect box1, Rect box2)
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

