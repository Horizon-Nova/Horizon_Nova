using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HNB.Areas.WW.Models;
using HNB.IntelligentSystems.DallE3.Module;
using HNB.IntelligentSystems.Embedding.Module;
using HNB.IntelligentSystems.GroundingDINO.Models;
using HNB.IntelligentSystems.GroundingDINO.Module;
using HNB.IntelligentSystems.GroundingDINO.Utils;
using HNB.IntelligentSystems.Qdrant.Models;
using HNB.IntelligentSystems.Qdrant.Module;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HNB.Areas.WW.Services
{
    public interface IWwAiWardrobeService
    {
        Task<WwAiImportResult> ImportWardrobeImagesAsync(
            IReadOnlyList<IFormFile> files,
            CancellationToken cancellationToken = default);

        Task<WwAiOutfitResult> RecommendOutfitAsync(
            WwAiOutfitRequest request,
            CancellationToken cancellationToken = default);

        Task<WwAiOutfitResult> TweakOutfitAsync(
            WwAiTweakOutfitRequest request,
            CancellationToken cancellationToken = default);

        Task<WwAiSaveLookPhotoResult> SaveLookPhotoAsync(
            WwAiSaveLookPhotoRequest request,
            CancellationToken cancellationToken = default);

        Task<WwOutfitHistoryResult> GetOutfitHistoryAsync(
            int limit = 20,
            CancellationToken cancellationToken = default);

        Task<WwFuturePlanResult> SaveFuturePlanAsync(
            WwFuturePlanRequest request,
            CancellationToken cancellationToken = default);

        Task<List<WwFuturePlanResult>> GetFuturePlansAsync(
            CancellationToken cancellationToken = default);

        Task<WwCalendarLookResult> SaveOutfitLookAsync(
            WwSaveOutfitLookRequest request,
            CancellationToken cancellationToken = default);

        Task<List<WwCalendarLookResult>> GetOutfitLooksByDateAsync(
            string date,
            CancellationToken cancellationToken = default);

        Task<List<string>> GetOutfitLookDatesAsync(
            CancellationToken cancellationToken = default);
    }

    public class WwAiWardrobeService : IWwAiWardrobeService
    {
        private const string WardrobeCollectionName = "ww_wardrobe_clip_vit_b_32";
        private const int VectorSize = 512;
        private const string StorageRoot = "Areas/Backoffice/storage";
        private const string ClothingPrompt = "shirt . top . pants . skirt . jacket . coat . shoes . bag . accessory .";
        private const string DefaultRemoteGroundingDinoBaseUrl = "https://horizon-nova.up.railway.app";
        private const string RemoteGroundingDinoDetectPath = "/api/GroundingDINOApi/DetectBase64";
        private const string RemoteGroundingDinoModelUrl = "https://horizon-nova.up.railway.app/storage/AI/groundingdino.onnx";
        private const string RemoteGroundingDinoVocabUrl = "https://horizon-nova.up.railway.app/storage/AI/vocab.txt";
        private static readonly string[] ClothingDetectionPrompts =
        [
            "shirt . blouse . t shirt . polo shirt . sweater . top .",
            "pants . trousers . jeans . shorts . skirt .",
            "jacket . coat . blazer . cardigan . hoodie .",
            "shoes . sneakers . boots . loafers .",
            "bag . belt . hat . scarf . accessory ."
        ];

        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GroundingDINOModule _groundingDinoModule;
        private readonly EmbeddingModule _embeddingModule;
        private readonly QdrantModule _qdrantModule;
        private readonly DallE3Module _dallE3Module;
        private readonly ILogger<WwAiWardrobeService> _logger;

        public WwAiWardrobeService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            GroundingDINOModule groundingDinoModule,
            EmbeddingModule embeddingModule,
            QdrantModule qdrantModule,
            DallE3Module dallE3Module,
            ILogger<WwAiWardrobeService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _groundingDinoModule = groundingDinoModule;
            _embeddingModule = embeddingModule;
            _qdrantModule = qdrantModule;
            _dallE3Module = dallE3Module;
            _logger = logger;
        }

        public async Task<WwAiImportResult> ImportWardrobeImagesAsync(
            IReadOnlyList<IFormFile> files,
            CancellationToken cancellationToken = default)
        {
            var result = new WwAiImportResult();
            result.Summary.TotalImages = files?.Count ?? 0;

            if (files == null || files.Count == 0)
            {
                result.Message = "未找到可上傳的檔案。";
                result.Errors.Add(result.Message);
                return result;
            }

            foreach (var file in files)
            {
                if (file == null || file.Length == 0 || !IsImageFile(file))
                {
                    result.Summary.FailedImages++;
                    result.Errors.Add($"無效的圖片檔案：{file?.FileName ?? "unknown"}");
                    continue;
                }

                try
                {
                    var imageBytes = await ReadFileBytesAsync(file, cancellationToken);
                    var sourceImageUrl = await SaveImageBytesAsync(
                        imageBytes,
                        "WW/Closet/Original",
                        Path.GetExtension(file.FileName),
                        cancellationToken);

                    var detections = await DetectWardrobeObjectsAsync(imageBytes, result.Errors, cancellationToken);
                    var imageId = $"ww_img_{Guid.NewGuid():N}";
                    var outputs = _groundingDinoModule.ProcessDetections(imageBytes, detections, imageId);
                    var hasRealDetections = outputs.Count > 0;
                    if (hasRealDetections)
                        outputs = ExpandFullBodyWardrobeOutputs(imageBytes, outputs, imageId);

                    if (outputs.Count == 0)
                    {
                        outputs.Add(new DetectionOutput
                        {
                            Id = $"{imageId}_obj_001",
                            Box = [0, 0, 0, 0],
                            Label = InferWardrobeLabel(file.FileName),
                            Score = 0,
                            CroppedImageBytes = imageBytes
                        });
                    }

                    foreach (var output in outputs)
                    {
                        var item = await BuildAndIndexWardrobeItemAsync(
                            output,
                            sourceImageUrl,
                            file.FileName,
                            hasRealDetections,
                            cancellationToken);

                        result.Items.Add(item);
                        result.Summary.ImportedItems++;
                        if (item.IsDetected)
                            result.Summary.DetectedItems++;
                        if (item.IsGenerated)
                            result.Summary.GeneratedImages++;
                        if (item.IsIndexed)
                            result.Summary.IndexedItems++;
                    }
                }
                catch (Exception ex)
                {
                    result.Summary.FailedImages++;
                    result.Errors.Add($"{file.FileName}: {ex.Message}");
                    _logger.LogWarning(ex, "WW AI wardrobe import failed for {FileName}", file.FileName);
                }
            }

            result.Success = result.Items.Count > 0;
            result.Message = result.Success
                ? $"已匯入 {result.Summary.ImportedItems} 件，完成 {result.Summary.IndexedItems} 筆向量索引。"
                : "匯入失敗。";

            return result;
        }

        public async Task<WwAiOutfitResult> RecommendOutfitAsync(
            WwAiOutfitRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = new WwAiOutfitResult
            {
                // UUID with hyphens — Qdrant point ID 格式要求
                OutfitId = Guid.NewGuid().ToString(),
                Occasion = request.Occasion,
                WeatherSummary = request.WeatherSummary
            };

            // 依場合與天氣產生搜尋語句、AI 穿搭名稱與圖片提示詞
            var stylePlan = await BuildStylePlanAsync(request.Occasion, request.WeatherSummary, cancellationToken);
            var queryText = stylePlan.WardrobeQuery;
            List<float>? queryVector = null;
            List<WwAiWardrobeItemResult> items = [];

            try
            {
                var embedding = await _embeddingModule.EncodeTextAsync(queryText, null, cancellationToken);
                if (embedding.Success && embedding.Vector != null && embedding.Vector.Count > 0)
                {
                    queryVector = embedding.Vector;
                }
                else
                {
                    _logger.LogInformation(
                        "WW AI outfit embedding unavailable; falling back to saved closet images. {Message}",
                        embedding.ErrorMessage ?? BuildClipModelUnavailableMessage(includeTokenizerFiles: true));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WW AI outfit query embedding failed");
            }

            if (queryVector != null)
            {
                try
                {
                    var searchResults = await _qdrantModule.Search(
                        WardrobeCollectionName,
                        queryVector,
                        25,
                        null,
                        BuildTypeFilter("wardrobe_item"),
                        cancellationToken);

                    items = searchResults
                        .Select(ConvertSearchResultToWardrobeItem)
                        .Where(item => item != null)
                        .Cast<WwAiWardrobeItemResult>()
                        .GroupBy(item => item.Id)
                        .Select(group => group.First())
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "WW AI outfit Qdrant search failed");
                }
            }

            if (items.Count == 0)
            {
                items = LoadRecentWardrobeItemsFromStorage();
            }

            result.Items = PickOutfitItems(items);
            if (result.Items.Count == 0)
            {
                result.Message = "目前衣櫥中找不到可搭配的品項。";
                return result;
            }

            result.Success = true;
            result.Name = string.IsNullOrWhiteSpace(stylePlan.OutfitName)
                ? BuildOutfitName(request)
                : stylePlan.OutfitName;
            result.Reason = string.IsNullOrWhiteSpace(stylePlan.Reason)
                ? BuildOutfitReason(request, result.Items)
                : stylePlan.Reason;
            result.Message = "穿搭已生成。";
            result.GeneratedImageUrl = await TryGenerateOutfitImageAsync(result, stylePlan.ImagePrompt, cancellationToken);
            if (queryVector != null)
            {
                await TryIndexOutfitHistoryAsync(result, queryVector, cancellationToken);
            }

            return result;
        }

        public async Task<WwAiSaveLookPhotoResult> SaveLookPhotoAsync(
            WwAiSaveLookPhotoRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = new WwAiSaveLookPhotoResult
            {
                LookId = Guid.NewGuid().ToString() // UUID with hyphens for Qdrant
            };

            if (request.Image == null || request.Image.Length == 0 || !IsImageFile(request.Image))
            {
                result.Message = "請上傳有效的圖片檔案。";
                result.Error = result.Message;
                return result;
            }

            try
            {
                var imageBytes = await ReadFileBytesAsync(request.Image, cancellationToken);
                result.ImageUrl = await SaveImageBytesAsync(
                    imageBytes,
                    "WW/Calendar/Looks",
                    Path.GetExtension(request.Image.FileName),
                    cancellationToken);

                var embedding = await _embeddingModule.EncodeImageAsync(imageBytes, null, cancellationToken);
                if (embedding.Success && embedding.Vector != null && embedding.Vector.Count > 0)
                {
                    var point = new VectorPoint
                    {
                        Id = result.LookId,
                        Vector = embedding.Vector,
                        Payload = new Dictionary<string, object>
                        {
                            ["id"] = result.LookId,
                            ["type"] = "look_photo",
                            ["categoryKey"] = "history",
                            ["label"] = "撖衣忽?抒?",
                            ["imageUrl"] = result.ImageUrl,
                            ["croppedImageUrl"] = result.ImageUrl,
                            ["sourceImageUrl"] = result.ImageUrl,
                            ["createdAt"] = DateTime.UtcNow.ToString("O"),
                            ["occasion"] = request.Occasion,
                            ["weatherSummary"] = request.WeatherSummary,
                            ["outfitId"] = request.OutfitId
                        }
                    };

                    result.IsIndexed = await _qdrantModule.UpsertVectors(
                        WardrobeCollectionName,
                        [point],
                        cancellationToken);
                }
                else
                {
                    _logger.LogInformation(
                        "WW AI look photo embedding unavailable; saved photo without vector index. {Message}",
                        embedding.ErrorMessage ?? BuildClipModelUnavailableMessage(includeTokenizerFiles: false));
                }

                result.Success = true;
                result.Message = result.IsIndexed ? "實穿照片已儲存並建立索引。" : "實穿照片已儲存。";
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                result.Message = $"儲存實穿照片失敗：{ex.Message}";
                _logger.LogWarning(ex, "WW AI look photo save failed");
            }

            return result;
        }

        public async Task<WwAiOutfitResult> TweakOutfitAsync(
            WwAiTweakOutfitRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = new WwAiOutfitResult
            {
                OutfitId = Guid.NewGuid().ToString(), // UUID with hyphens
                Occasion = request.Occasion,
            };

            var stylePlan = await BuildStylePlanForTweakAsync(request, cancellationToken);

            List<float>? queryVector = null;
            List<WwAiWardrobeItemResult> items = [];

            try
            {
                var embedding = await _embeddingModule.EncodeTextAsync(stylePlan.WardrobeQuery, null, cancellationToken);
                if (embedding.Success && embedding.Vector != null && embedding.Vector.Count > 0)
                    queryVector = embedding.Vector;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WW AI tweak outfit query embedding failed");
            }

            if (queryVector != null)
            {
                try
                {
                    var searchResults = await _qdrantModule.Search(
                        WardrobeCollectionName, queryVector, 25, null, BuildTypeFilter("wardrobe_item"), cancellationToken);

                    items = searchResults
                        .Select(ConvertSearchResultToWardrobeItem)
                        .Where(i => i != null)
                        .Cast<WwAiWardrobeItemResult>()
                        .GroupBy(i => i.Id)
                        .Select(g => g.First())
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "WW AI tweak outfit Qdrant search failed");
                }
            }

            if (items.Count == 0)
                items = LoadRecentWardrobeItemsFromStorage();

            result.Items = PickOutfitItems(items);
            if (result.Items.Count == 0)
            {
                result.Message = "目前衣櫥中找不到可搭配的品項。";
                return result;
            }

            result.Success = true;
            result.Name = string.IsNullOrWhiteSpace(stylePlan.OutfitName)
                ? $"{request.Occasion} ({request.TweakInstruction})"
                : stylePlan.OutfitName;
            result.Reason = string.IsNullOrWhiteSpace(stylePlan.Reason)
                ? $"Adjusted your outfit for \"{request.TweakInstruction}\"."
                : stylePlan.Reason;
            result.Message = "已完成穿搭微調。";
            result.GeneratedImageUrl = await TryGenerateOutfitImageAsync(result, stylePlan.ImagePrompt, cancellationToken);

            if (queryVector != null)
                await TryIndexOutfitHistoryAsync(result, queryVector, cancellationToken);

            return result;
        }

        public async Task<WwOutfitHistoryResult> GetOutfitHistoryAsync(
            int limit = 20,
            CancellationToken cancellationToken = default)
        {
            var result = new WwOutfitHistoryResult();
            try
            {
                // 用零向量 + type 過濾，從共用集合取回穿搭歷史
                var zeroVector = Enumerable.Repeat(0f, VectorSize).ToList();
                var searchResults = await _qdrantModule.Search(
                    WardrobeCollectionName,
                    zeroVector,
                    limit,
                    null,
                    BuildTypeFilter("outfit"),
                    cancellationToken);

                result.Items = searchResults
                    .Select(sr =>
                    {
                        if (sr.Payload == null) return null;
                        return new WwOutfitHistoryItem
                        {
                            OutfitId          = ReadPayloadString(sr.Payload, "id"),
                            Name              = ReadPayloadString(sr.Payload, "label"),
                            Occasion          = ReadPayloadString(sr.Payload, "occasion"),
                            WeatherSummary    = ReadPayloadString(sr.Payload, "weatherSummary"),
                            GeneratedImageUrl = ReadPayloadString(sr.Payload, "imageUrl"),
                            CreatedAt         = ReadPayloadString(sr.Payload, "createdAt"),
                        };
                    })
                    .Where(item => item != null)
                    .Cast<WwOutfitHistoryItem>()
                    .OrderByDescending(item => item.CreatedAt)
                    .ToList();

                result.Success = true;
                result.Message = $"已載入 {result.Items.Count} 筆穿搭歷史。";
            }
            catch (Exception ex)
            {
                result.Message = $"查詢穿搭歷史失敗：{ex.Message}";
                _logger.LogWarning(ex, "WW AI outfit history query failed");
            }

            return result;
        }

        public async Task<WwFuturePlanResult> SaveFuturePlanAsync(
            WwFuturePlanRequest request,
            CancellationToken cancellationToken = default)
        {
            var date = request.Date?.Trim() ?? string.Empty;
            var note = request.Note?.Trim() ?? string.Empty;
            var plan = new WwFuturePlanResult
            {
                Date = date,
                Note = note,
                SavedAt = DateTime.UtcNow.ToString("O")
            };

            var pointId = BuildFuturePlanPointId(date);
            var vector = await BuildFuturePlanVectorAsync(date, note, cancellationToken);
            var point = new VectorPoint
            {
                Id = pointId,
                Vector = vector,
                Payload = new Dictionary<string, object>
                {
                    ["id"] = pointId,
                    ["type"] = "future_plan",
                    ["date"] = date,
                    ["note"] = note,
                    ["savedAt"] = plan.SavedAt,
                    ["label"] = note,
                    ["createdAt"] = plan.SavedAt
                }
            };

            try
            {
                await _qdrantModule.UpsertVectors(WardrobeCollectionName, [point], cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WW AI future plan upsert failed for Date={Date}; returning plan without persistence", date);
            }

            return plan;
        }

        public async Task<List<WwFuturePlanResult>> GetFuturePlansAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var zeroVector = Enumerable.Repeat(0f, VectorSize).ToList();
                var searchResults = await _qdrantModule.Search(
                    WardrobeCollectionName,
                    zeroVector,
                    128,
                    null,
                    BuildTypeFilter("future_plan"),
                    cancellationToken);

                return searchResults
                    .Select(sr => sr.Payload)
                    .Where(payload => payload != null)
                    .Select(payload => new WwFuturePlanResult
                    {
                        Date    = ReadPayloadString(payload!, "date"),
                        Note    = ReadPayloadString(payload!, "note"),
                        SavedAt = ReadPayloadString(payload!, "savedAt")
                    })
                    .Where(plan => !string.IsNullOrWhiteSpace(plan.Date) && !string.IsNullOrWhiteSpace(plan.Note))
                    .GroupBy(plan => plan.Date, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.OrderByDescending(item => item.SavedAt).First())
                    .OrderByDescending(item => item.SavedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WW AI future plans query failed; returning empty list");
                return [];
            }
        }

        public async Task<WwCalendarLookResult> SaveOutfitLookAsync(
            WwSaveOutfitLookRequest request,
            CancellationToken cancellationToken = default)
        {
            var date = string.IsNullOrWhiteSpace(request.Date)
                ? DateTime.UtcNow.ToString("yyyy-MM-dd")
                : request.Date.Trim();

            // Qdrant 要求 point ID 為 RFC-4122 UUID（含連字號）或無符號整數
            // Guid.NewGuid().ToString() 產生標準 UUID，Guid:N 不帶連字號故無效
            var qdrantPointId = Guid.NewGuid().ToString();           // "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
            var displayId     = $"ww_cal_{qdrantPointId.Replace("-", "")}"; // payload 內友善識別碼
            var savedAt       = DateTime.UtcNow.ToString("O");

            var result = new WwCalendarLookResult
            {
                Id               = displayId,
                Name             = request.Name,
                Occasion         = request.Occasion,
                WeatherSummary   = request.WeatherSummary,
                GeneratedImageUrl = request.GeneratedImageUrl,
                ItemLabels       = request.ItemLabels ?? [],
                Date             = date,
                SavedAt          = savedAt
            };

            try
            {
                var collectionReady = await EnsureCollectionExistsAsync(WardrobeCollectionName, VectorSize, cancellationToken);
                if (!collectionReady)
                {
                    throw new InvalidOperationException($"Qdrant collection bootstrap failed: {WardrobeCollectionName}");
                }

                // 使用零向量：calendar_look 以 filter 檢索，不依賴向量相似度
                var vector = Enumerable.Repeat(0f, VectorSize).ToList();

                var point = new VectorPoint
                {
                    Id     = qdrantPointId,   // 有效 UUID，Qdrant 接受
                    Vector = vector,
                    Payload = new Dictionary<string, object>
                    {
                        ["id"]                = displayId,
                        ["type"]              = "calendar_look",
                        ["date"]              = date,
                        ["name"]              = request.Name,
                        ["label"]             = request.Name,
                        ["occasion"]          = request.Occasion,
                        ["weatherSummary"]    = request.WeatherSummary,
                        ["generatedImageUrl"] = request.GeneratedImageUrl,
                        ["itemLabels"]        = string.Join(",", request.ItemLabels ?? []),
                        ["savedAt"]           = savedAt,
                        ["outfitId"]          = request.OutfitId
                    }
                };

                var saved = await _qdrantModule.UpsertVectors(WardrobeCollectionName, [point], cancellationToken);
                if (!saved)
                {
                    var config = _qdrantModule.LoadConfig();
                    _logger.LogError(
                        "WW AI calendar look Qdrant upsert failed. OutfitId={OutfitId}, PointId={PointId}, Collection={CollectionName}, Endpoint={Url}:{Port}, UseHttps={UseHttps}",
                        request.OutfitId,
                        qdrantPointId,
                        WardrobeCollectionName,
                        config.Url,
                        config.Port,
                        config.UseHttps);
                    throw new InvalidOperationException("Qdrant upsert failed when saving calendar look.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WW AI calendar look save failed for OutfitId={OutfitId}", request.OutfitId);
                throw;
            }

            return result;
        }

        public async Task<List<WwCalendarLookResult>> GetOutfitLooksByDateAsync(
            string date,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Scroll API：純 filter 檢索，不需向量，避免 cosine(0,v)=NaN 導致無結果
                var scrollResults = await _qdrantModule.Scroll(
                    WardrobeCollectionName,
                    BuildCalendarLookFilter(date),
                    100,
                    cancellationToken);

                return scrollResults
                    .Select(sr => sr.Payload)
                    .Where(p => p != null)
                    .Select(p => new WwCalendarLookResult
                    {
                        Id = ReadPayloadString(p!, "id"),
                        Name = ReadPayloadString(p!, "name"),
                        Occasion = ReadPayloadString(p!, "occasion"),
                        WeatherSummary = ReadPayloadString(p!, "weatherSummary"),
                        GeneratedImageUrl = ReadPayloadString(p!, "generatedImageUrl"),
                        ItemLabels = ReadPayloadString(p!, "itemLabels")
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList(),
                        Date = ReadPayloadString(p!, "date"),
                        SavedAt = ReadPayloadString(p!, "savedAt")
                    })
                    .Where(r => !string.IsNullOrWhiteSpace(r.Id))
                    .OrderByDescending(r => r.SavedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WW AI calendar looks query failed for Date={Date}", date);
                return [];
            }
        }

        public async Task<List<string>> GetOutfitLookDatesAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var scrollResults = await _qdrantModule.Scroll(
                    WardrobeCollectionName,
                    BuildCalendarLookFilter(),
                    500,
                    cancellationToken);

                return scrollResults
                    .Select(sr => sr.Payload)
                    .Where(p => p != null)
                    .Select(p => ReadPayloadString(p!, "date"))
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WW AI calendar look dates query failed");
                return [];
            }
        }

        private static Dictionary<string, object> BuildCalendarLookFilter(string? date = null)
        {
            var musts = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["key"] = "type",
                    ["match"] = new Dictionary<string, object> { ["value"] = "calendar_look" }
                }
            };

            if (!string.IsNullOrWhiteSpace(date))
            {
                musts.Add(new Dictionary<string, object>
                {
                    ["key"] = "date",
                    ["match"] = new Dictionary<string, object> { ["value"] = date }
                });
            }

            return new Dictionary<string, object> { ["must"] = musts };
        }

        private async Task<bool> EnsureCollectionExistsAsync(
            string collectionName,
            int vectorSize,
            CancellationToken cancellationToken)
        {
            try
            {
                var exists = await _qdrantModule.CollectionExists(collectionName, cancellationToken);
                if (exists)
                    return true;

                var created = await _qdrantModule.CreateCollection(collectionName, vectorSize, "Cosine", cancellationToken);
                if (!created)
                {
                    var config = _qdrantModule.LoadConfig();
                    _logger.LogError(
                        "Qdrant collection create failed. Collection={CollectionName}, VectorSize={VectorSize}, Endpoint={Url}:{Port}, UseHttps={UseHttps}",
                        collectionName,
                        vectorSize,
                        config.Url,
                        config.Port,
                        config.UseHttps);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                var config = _qdrantModule.LoadConfig();
                _logger.LogError(
                    ex,
                    "Qdrant collection bootstrap exception. Collection={CollectionName}, VectorSize={VectorSize}, Endpoint={Url}:{Port}, UseHttps={UseHttps}",
                    collectionName,
                    vectorSize,
                    config.Url,
                    config.Port,
                    config.UseHttps);
                return false;
            }
        }

        private static bool IsQdrantNotFound(Exception ex)
        {
            return ex.Message.Contains("NotFound", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("404", StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<string, object> BuildTypeFilter(string type) =>
            new()
            {
                ["must"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["key"] = "type",
                        ["match"] = new Dictionary<string, object> { ["value"] = type }
                    }
                }
            };

        private static string BuildFuturePlanPointId(string date)
        {
            // 將日期 hash 成 16 bytes → 標準 UUID 格式，Qdrant 保證接受
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("ww_future_plan_" + date));
            return new Guid(bytes[..16]).ToString(); // "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
        }

        private async Task<List<float>> BuildFuturePlanVectorAsync(
            string date,
            string note,
            CancellationToken cancellationToken)
        {
            var fallback = Enumerable.Repeat(0f, VectorSize).ToList();
            var text = $"{date} {note}".Trim();
            if (string.IsNullOrWhiteSpace(text))
                return fallback;

            try
            {
                var embedding = await _embeddingModule.EncodeTextAsync(text, null, cancellationToken);
                if (embedding.Success && embedding.Vector != null && embedding.Vector.Count == VectorSize)
                    return embedding.Vector;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "WW AI future plan embedding failed; using zero-vector fallback");
            }

            return fallback;
        }

        private sealed class OutfitStylePlan
        {
            public string WardrobeQuery { get; init; } = string.Empty;
            public string ImagePrompt { get; init; } = string.Empty;
            public string OutfitName { get; init; } = string.Empty;
            public string Reason { get; init; } = string.Empty;
        }

        private async Task<OutfitStylePlan> BuildStylePlanAsync(
            string occasion,
            string weatherSummary,
            CancellationToken cancellationToken)
        {
            var occ = string.IsNullOrWhiteSpace(occasion) ? "casual" : occasion.Trim();
            var weather = string.IsNullOrWhiteSpace(weatherSummary) ? "everyday weather" : weatherSummary.Trim();

            // Fallback plan（GPT 失敗時使用）
            var fallback = new OutfitStylePlan
            {
                WardrobeQuery = $"outfit {occ} {weather} top bottom outerwear shoes accessories",
                ImagePrompt = $"Clean flat lay of a complete {occ} outfit styled for {weather}. Neatly arranged on a neutral background, realistic and cohesive.",
                OutfitName = ToTitleCase(occ.Split(' ').Take(4)) + " Look",
                Reason = $"Picked for {occ} with your current weather."
            };

            try
            {
                var apiKey = _dallE3Module.LoadConfig().ApiKey;
                if (string.IsNullOrWhiteSpace(apiKey))
                    return fallback;

                var prompt =
                    $"You are a personal stylist AI. Given the occasion and weather, respond with ONLY a valid JSON object.\n" +
                    $"Occasion: \"{occ}\"\n" +
                    $"Weather: \"{weather}\"\n\n" +
                    $"Return this exact JSON (no markdown, no extra text):\n" +
                    $"{{\"name\":\"<3-5 word creative outfit name>\",\"imagePrompt\":\"<detailed flat-lay photography prompt for image generation>\",\"wardrobeQuery\":\"<keywords to search clothing items>\",\"reason\":\"<one sentence why this outfit fits the occasion>\"}}";

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 300,
                    temperature = 0.8
                };

                var json = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                var baseUrl = _dallE3Module.LoadConfig().BaseUrl.TrimEnd('/');

                using var response = await httpClient.PostAsync(
                    $"{baseUrl}/chat/completions",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return fallback;

                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(responseText);

                var messageContent = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;

                // Strip markdown code fences if present
                var cleaned = messageContent.Trim();
                if (cleaned.StartsWith("```")) cleaned = cleaned[(cleaned.IndexOf('\n') + 1)..];
                if (cleaned.EndsWith("```")) cleaned = cleaned[..cleaned.LastIndexOf("```")];
                cleaned = cleaned.Trim();

                using var gptDoc = JsonDocument.Parse(cleaned);
                var root = gptDoc.RootElement;

                return new OutfitStylePlan
                {
                    WardrobeQuery = root.TryGetProperty("wardrobeQuery", out var wq) && wq.ValueKind == JsonValueKind.String
                        ? $"outfit {wq.GetString()} {weather}"
                        : fallback.WardrobeQuery,
                    ImagePrompt = root.TryGetProperty("imagePrompt", out var ip) && ip.ValueKind == JsonValueKind.String
                        ? ip.GetString()!
                        : fallback.ImagePrompt,
                    OutfitName = root.TryGetProperty("name", out var nm) && nm.ValueKind == JsonValueKind.String
                        ? nm.GetString()!
                        : fallback.OutfitName,
                    Reason = root.TryGetProperty("reason", out var rs) && rs.ValueKind == JsonValueKind.String
                        ? rs.GetString()!
                        : fallback.Reason
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WW AI outfit style plan GPT call failed; using fallback for occasion={Occasion}", occ);
                return fallback;
            }
        }

        private async Task<OutfitStylePlan> BuildStylePlanForTweakAsync(
            WwAiTweakOutfitRequest request,
            CancellationToken cancellationToken)
        {
            var occ = string.IsNullOrWhiteSpace(request.Occasion) ? "casual" : request.Occasion.Trim();
            var tweak = string.IsNullOrWhiteSpace(request.TweakInstruction) ? "balanced" : request.TweakInstruction.Trim();
            var currentName = string.IsNullOrWhiteSpace(request.CurrentOutfitName) ? occ : request.CurrentOutfitName.Trim();
            var currentItems = request.CurrentItemLabels == null || request.CurrentItemLabels.Count == 0
                ? "top bottom shoes"
                : string.Join(" ", request.CurrentItemLabels.Where(label => !string.IsNullOrWhiteSpace(label)));

            var fallback = new OutfitStylePlan
            {
                WardrobeQuery = $"outfit {occ} {tweak} {currentItems}",
                ImagePrompt = $"Clean flat lay of a {tweak.ToLowerInvariant()} {occ} outfit using these clothing categories: {currentItems}. Neutral background, realistic styling.",
                OutfitName = $"{currentName} · {tweak}",
                Reason = $"Adjusted your outfit for \"{tweak}\"."
            };

            try
            {
                var apiKey = _dallE3Module.LoadConfig().ApiKey;
                if (string.IsNullOrWhiteSpace(apiKey))
                    return fallback;

                var prompt =
                    $"You are a personal stylist AI. The user wants to tweak an existing outfit.\n" +
                    $"Original outfit: \"{currentName}\" for \"{occ}\"\n" +
                    $"Items: {currentItems}\n" +
                    $"Tweak request: \"{tweak}\"\n\n" +
                    $"Return ONLY a valid JSON object (no markdown):\n" +
                    $"{{\"name\":\"<3-5 word creative new outfit name>\",\"imagePrompt\":\"<detailed flat-lay photography prompt>\",\"wardrobeQuery\":\"<keywords to search clothing items>\",\"reason\":\"<one sentence describing the adjustment>\"}}";

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[] { new { role = "user", content = prompt } },
                    max_tokens = 300,
                    temperature = 0.8
                };

                var json = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                var baseUrl = _dallE3Module.LoadConfig().BaseUrl.TrimEnd('/');

                using var response = await httpClient.PostAsync(
                    $"{baseUrl}/chat/completions",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return fallback;

                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(responseText);

                var messageContent = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;

                var cleaned = messageContent.Trim();
                if (cleaned.StartsWith("```")) cleaned = cleaned[(cleaned.IndexOf('\n') + 1)..];
                if (cleaned.EndsWith("```")) cleaned = cleaned[..cleaned.LastIndexOf("```")];
                cleaned = cleaned.Trim();

                using var gptDoc = JsonDocument.Parse(cleaned);
                var root = gptDoc.RootElement;

                return new OutfitStylePlan
                {
                    WardrobeQuery = root.TryGetProperty("wardrobeQuery", out var wq) && wq.ValueKind == JsonValueKind.String
                        ? $"outfit {wq.GetString()}"
                        : fallback.WardrobeQuery,
                    ImagePrompt = root.TryGetProperty("imagePrompt", out var ip) && ip.ValueKind == JsonValueKind.String
                        ? ip.GetString()!
                        : fallback.ImagePrompt,
                    OutfitName = root.TryGetProperty("name", out var nm) && nm.ValueKind == JsonValueKind.String
                        ? nm.GetString()!
                        : fallback.OutfitName,
                    Reason = root.TryGetProperty("reason", out var rs) && rs.ValueKind == JsonValueKind.String
                        ? rs.GetString()!
                        : fallback.Reason
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WW AI tweak style plan GPT call failed; using fallback for tweak={Tweak}", tweak);
                return fallback;
            }
        }

        private static string ToTitleCase(IEnumerable<string> words)
        {
            return string.Join(" ", words.Select(w =>
                string.IsNullOrWhiteSpace(w) ? w : char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));
        }

        private async Task<List<DetectionResult>> DetectWardrobeObjectsAsync(
            byte[] imageBytes,
            List<string> errors,
            CancellationToken cancellationToken)
        {
            try
            {
                if (UseRemoteGroundingDino())
                {
                    var remoteDetections = await DetectWardrobeObjectsFromRailwayAsync(imageBytes, cancellationToken);
                    if (remoteDetections.Count > 0)
                        return MergeDuplicateDetections(remoteDetections);
                }

                await EnsureGroundingDinoModelFilesAsync(cancellationToken);

                if (!_groundingDinoModule.IsModelReady())
                    _groundingDinoModule.StartEngine();

                if (!_groundingDinoModule.IsModelReady())
                {
                    var status = _groundingDinoModule.LoadDetailedStatus();
                    _logger.LogInformation("GroundingDINO unavailable; using full image fallback. {Message}", BuildGroundingDinoUnavailableMessage(status));
                    return [];
                }

                var detections = new List<DetectionResult>();
                foreach (var prompt in ClothingDetectionPrompts)
                {
                    detections.AddRange(_groundingDinoModule.DetectObjectsFromBytes(imageBytes, prompt));
                }

                if (detections.Count == 0)
                    detections.AddRange(_groundingDinoModule.DetectObjectsFromBytes(imageBytes, ClothingPrompt));

                return MergeDuplicateDetections(detections);
            }
            catch (Exception ex)
            {
                var status = _groundingDinoModule.LoadDetailedStatus();
                _logger.LogWarning(ex, "GroundingDINO detection failed; using full image fallback. {Message}", BuildGroundingDinoUnavailableMessage(status));
                return [];
            }
        }

        private bool UseRemoteGroundingDino()
        {
            return bool.TryParse(_configuration["WwAi:UseRemoteGroundingDINO"], out var enabled) && enabled;
        }

        private async Task<List<DetectionResult>> DetectWardrobeObjectsFromRailwayAsync(
            byte[] imageBytes,
            CancellationToken cancellationToken)
        {
            var baseUrl = ResolveRemoteGroundingDinoBaseUrl();
            if (string.IsNullOrWhiteSpace(baseUrl))
                return [];

            var detections = new List<DetectionResult>();
            foreach (var prompt in ClothingDetectionPrompts.Append(ClothingPrompt))
            {
                var promptDetections = await DetectWardrobeObjectsFromRailwayAsync(
                    imageBytes,
                    prompt,
                    baseUrl,
                    cancellationToken);

                detections.AddRange(promptDetections);
                if (detections.Count > 0)
                    break;
            }

            return detections;
        }

        private async Task<List<DetectionResult>> DetectWardrobeObjectsFromRailwayAsync(
            byte[] imageBytes,
            string prompt,
            string baseUrl,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = new
                {
                    imagesBase64 = new[] { Convert.ToBase64String(imageBytes) },
                    textPrompt = prompt
                };

                using var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(5);

                using var response = await httpClient.PostAsync(
                    BuildRemoteGroundingDinoUrl(baseUrl),
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Remote GroundingDINO returned {StatusCode}; falling back to local module.",
                        response.StatusCode);
                    return [];
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                return ParseRemoteGroundingDinoDetections(document.RootElement);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Remote GroundingDINO detection failed; falling back to local module.");
                return [];
            }
        }

        private string ResolveRemoteGroundingDinoBaseUrl()
        {
            var configured = _configuration["WwAi:GroundingDINOBaseUrl"];
            if (string.IsNullOrWhiteSpace(configured))
                configured = _configuration["GroundingDINO:RemoteBaseUrl"];

            if (string.IsNullOrWhiteSpace(configured))
                configured = DefaultRemoteGroundingDinoBaseUrl;

            return configured.Trim().TrimEnd('/');
        }

        private static string BuildRemoteGroundingDinoUrl(string baseUrl)
        {
            return $"{baseUrl.TrimEnd('/')}{RemoteGroundingDinoDetectPath}";
        }

        private static List<DetectionResult> ParseRemoteGroundingDinoDetections(JsonElement root)
        {
            var detections = new List<DetectionResult>();
            if (!root.TryGetProperty("images", out var images) || images.ValueKind != JsonValueKind.Array)
                return detections;

            foreach (var image in images.EnumerateArray())
            {
                if (!image.TryGetProperty("detections", out var detectionItems) ||
                    detectionItems.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var detectionItem in detectionItems.EnumerateArray())
                {
                    var detection = ParseRemoteGroundingDinoDetection(detectionItem);
                    if (detection != null)
                        detections.Add(detection);
                }
            }

            return detections;
        }

        private static DetectionResult? ParseRemoteGroundingDinoDetection(JsonElement detectionItem)
        {
            if (!detectionItem.TryGetProperty("box", out var boxElement) ||
                boxElement.ValueKind != JsonValueKind.Array ||
                boxElement.GetArrayLength() < 4)
            {
                return null;
            }

            var x = ReadJsonInt(boxElement[0]);
            var y = ReadJsonInt(boxElement[1]);
            var width = ReadJsonInt(boxElement[2]);
            var height = ReadJsonInt(boxElement[3]);
            if (width <= 0 || height <= 0)
                return null;

            return new DetectionResult
            {
                Box = new HNB.IntelligentSystems.GroundingDINO.Models.Rectangle(x, y, width, height),
                Label = ReadJsonString(detectionItem, "label"),
                Score = ReadJsonFloat(detectionItem, "score")
            };
        }

        private static int ReadJsonInt(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var value))
                return value;

            if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var doubleValue))
                return (int)Math.Round(doubleValue);

            return 0;
        }

        private static float ReadJsonFloat(JsonElement parent, string propertyName)
        {
            if (!parent.TryGetProperty(propertyName, out var element))
                return 0;

            if (element.ValueKind == JsonValueKind.Number && element.TryGetSingle(out var value))
                return value;

            if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var doubleValue))
                return (float)doubleValue;

            return 0;
        }

        private static string ReadJsonString(JsonElement parent, string propertyName)
        {
            if (!parent.TryGetProperty(propertyName, out var element))
                return string.Empty;

            return element.ValueKind == JsonValueKind.String
                ? element.GetString() ?? string.Empty
                : element.GetRawText();
        }

        private async Task EnsureGroundingDinoModelFilesAsync(CancellationToken cancellationToken)
        {
            var modelPath = ResolveGroundingDinoConfiguredPath(
                "GroundingDINO:ModelPath",
                Path.Combine(StorageRoot, "AI", "DINO", "groundingdino", "groundingdino.onnx"));

            var vocabPath = ResolveGroundingDinoConfiguredPath(
                "GroundingDINO:VocabPath",
                Path.Combine(StorageRoot, "AI", "DINO", "groundingdino", "vocab.txt"));

            await DownloadGroundingDinoFileIfMissingAsync(
                modelPath,
                RemoteGroundingDinoModelUrl,
                "groundingdino.onnx",
                cancellationToken);

            await DownloadGroundingDinoFileIfMissingAsync(
                vocabPath,
                RemoteGroundingDinoVocabUrl,
                "vocab.txt",
                cancellationToken);
        }

        private string ResolveGroundingDinoConfiguredPath(string key, string fallbackRelativePath)
        {
            var configuredPath = _configuration[key];
            var path = string.IsNullOrWhiteSpace(configuredPath)
                ? fallbackRelativePath
                : configuredPath;

            if (Path.IsPathRooted(path))
                return path;

            return Path.Combine(_environment.ContentRootPath, path);
        }

        private async Task DownloadGroundingDinoFileIfMissingAsync(
            string destinationPath,
            string remoteUrl,
            string expectedFileName,
            CancellationToken cancellationToken)
        {
            var fileName = Path.GetFileName(destinationPath);
            if (!string.Equals(fileName, expectedFileName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Skipping GroundingDINO auto-download because destination file name is not expected. Expected {ExpectedFileName}, got {FileName}",
                    expectedFileName,
                    fileName);
                return;
            }

            if (File.Exists(destinationPath) && new FileInfo(destinationPath).Length > 0)
                return;

            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            _logger.LogInformation(
                "GroundingDINO file missing. Downloading {FileName} from {RemoteUrl} to {DestinationPath}",
                expectedFileName,
                remoteUrl,
                destinationPath);

            var tempPath = $"{destinationPath}.download";
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(30);

                using var response = await httpClient.GetAsync(
                    remoteUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                await using (var remoteStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                await using (var fileStream = new FileStream(
                    tempPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    8192,
                    useAsync: true))
                {
                    await remoteStream.CopyToAsync(fileStream, cancellationToken);
                }

                var fileInfo = new FileInfo(tempPath);
                if (fileInfo.Length == 0)
                    throw new InvalidOperationException($"{expectedFileName} download completed with 0 bytes.");

                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);

                File.Move(tempPath, destinationPath);
            }
            catch (Exception ex)
            {
                TryDeleteFile(tempPath);
                _logger.LogWarning(
                    ex,
                    "GroundingDINO auto-download failed for {FileName} from {RemoteUrl}",
                    expectedFileName,
                    remoteUrl);
            }
        }

        private async Task<WwAiWardrobeItemResult> BuildAndIndexWardrobeItemAsync(
            DetectionOutput output,
            string sourceImageUrl,
            string sourceFileName,
            bool isDetected,
            CancellationToken cancellationToken)
        {
            var detectedLabel = NormalizeWardrobeLabel(output.Label, sourceFileName);
            var categoryKey = ResolveCategoryKey(detectedLabel);
            var label = BuildWardrobeDisplayLabel(detectedLabel, output.CroppedImageBytes, sourceFileName);
            var croppedImageUrl = await SaveImageBytesAsync(
                output.CroppedImageBytes,
                "WW/Closet/Cropped",
                ".jpg",
                cancellationToken);

            var item = new WwAiWardrobeItemResult
            {
                Id = output.Id,
                CategoryKey = categoryKey,
                Label = label,
                Score = output.Score,
                ImageUrl = croppedImageUrl,
                CroppedImageUrl = croppedImageUrl,
                SourceImageUrl = sourceImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsDetected = isDetected
            };

            var generatedImageUrl = await TryGenerateWardrobeItemImageAsync(item, output.CroppedImageBytes, cancellationToken);
            if (!string.IsNullOrWhiteSpace(generatedImageUrl))
            {
                item.GeneratedImageUrl = generatedImageUrl;
                item.ImageUrl = generatedImageUrl;
                item.IsGenerated = true;
            }

            try
            {
                var embedding = await _embeddingModule.EncodeImageAsync(output.CroppedImageBytes, null, cancellationToken);
                if (!embedding.Success || embedding.Vector == null || embedding.Vector.Count == 0)
                {
                    _logger.LogInformation(
                        "WW AI wardrobe item embedding unavailable for {ItemId}; imported without vector index. {Message}",
                        item.Id,
                        embedding.ErrorMessage ?? BuildClipModelUnavailableMessage(includeTokenizerFiles: false));
                    return item;
                }

                var point = new VectorPoint
                {
                    Id = item.Id,
                    Vector = embedding.Vector,
                    Payload = BuildWardrobePayload(item)
                };

                item.IsIndexed = await _qdrantModule.UpsertVectors(
                    WardrobeCollectionName,
                    [point],
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WW AI wardrobe item indexing failed for {ItemId}", item.Id);
            }

            return item;
        }

        private async Task<string?> TryGenerateWardrobeItemImageAsync(
            WwAiWardrobeItemResult item,
            byte[] croppedImageBytes,
            CancellationToken cancellationToken)
        {
            try
            {
                var prompt =
                    "Create a clean single clothing product image from this reference. " +
                    "Keep the garment shape and color, remove people and background, centered on transparent or plain light background.";

                var outputs = await _dallE3Module.EditImage(
                    prompt,
                    [croppedImageBytes],
                    "gpt-image-1",
                    cancellationToken);

                var first = outputs.FirstOrDefault();
                if (first == null || first.ImageBytes.Length == 0)
                    return null;

                return await SaveImageBytesAsync(first.ImageBytes, "WW/Closet/Generated", ".png", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WW AI wardrobe item generation failed for {ItemId}", item.Id);
                return null;
            }
        }

        private async Task<string?> TryGenerateOutfitImageAsync(
            WwAiOutfitResult outfit,
            string imagePrompt,
            CancellationToken cancellationToken)
        {
            var imageBytes = new List<byte[]>();
            foreach (var item in outfit.Items.Take(6))
            {
                var path = ResolveStoragePathFromUrl(item.CroppedImageUrl);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    imageBytes.Add(await File.ReadAllBytesAsync(path, cancellationToken));
            }

            if (imageBytes.Count == 0)
                return null;

            try
            {
                // 直接使用 DallE3 模組，若提示詞為空則使用預設提示
                var prompt = string.IsNullOrWhiteSpace(imagePrompt)
                    ? $"Clean flat lay of a complete outfit for {outfit.Occasion}. Neatly arranged clothing items on a neutral background."
                    : imagePrompt;
                var outputs = await _dallE3Module.EditImage(prompt, imageBytes, "gpt-image-1", cancellationToken);
                var first = outputs.FirstOrDefault();
                if (first == null || first.ImageBytes.Length == 0)
                    return null;

                return await SaveImageBytesAsync(first.ImageBytes, "WW/Today/Generated", ".png", cancellationToken);
            }
            catch (Exception ex)
            {
                outfit.Errors.Add($"DallE3 生成失敗，改回傳文字推薦：{ex.Message}");
                _logger.LogWarning(ex, "WW AI outfit image generation failed for {OutfitId}", outfit.OutfitId);
                return null;
            }
        }

        private async Task TryIndexOutfitHistoryAsync(
            WwAiOutfitResult outfit,
            List<float> vector,
            CancellationToken cancellationToken)
        {
            try
            {
                var point = new VectorPoint
                {
                    Id = outfit.OutfitId,
                    Vector = vector,
                    Payload = new Dictionary<string, object>
                    {
                        ["id"] = outfit.OutfitId,
                        ["type"] = "outfit",
                        ["categoryKey"] = "history",
                        ["label"] = outfit.Name,
                        ["imageUrl"] = outfit.GeneratedImageUrl ?? string.Empty,
                        ["croppedImageUrl"] = outfit.GeneratedImageUrl ?? string.Empty,
                        ["sourceImageUrl"] = outfit.GeneratedImageUrl ?? string.Empty,
                        ["createdAt"] = DateTime.UtcNow.ToString("O"),
                        ["occasion"] = outfit.Occasion,
                        ["weatherSummary"] = outfit.WeatherSummary,
                        ["itemIds"] = string.Join(",", outfit.Items.Select(item => item.Id))
                    }
                };

                await _qdrantModule.UpsertVectors(WardrobeCollectionName, [point], cancellationToken);
            }
            catch (Exception ex)
            {
                outfit.Errors.Add($"穿搭歷史索引失敗：{ex.Message}");
                _logger.LogWarning(ex, "WW AI outfit history indexing failed for {OutfitId}", outfit.OutfitId);
            }
        }

        private static Dictionary<string, object> BuildWardrobePayload(WwAiWardrobeItemResult item)
        {
            return new Dictionary<string, object>
            {
                ["id"] = item.Id,
                ["type"] = item.Type,
                ["categoryKey"] = item.CategoryKey,
                ["label"] = item.Label,
                ["imageUrl"] = item.ImageUrl,
                ["generatedImageUrl"] = item.GeneratedImageUrl ?? string.Empty,
                ["croppedImageUrl"] = item.CroppedImageUrl,
                ["sourceImageUrl"] = item.SourceImageUrl,
                ["createdAt"] = item.CreatedAt.ToString("O"),
                ["occasion"] = string.Empty,
                ["weatherSummary"] = string.Empty,
                ["isDetected"] = item.IsDetected.ToString(CultureInfo.InvariantCulture),
                ["isGenerated"] = item.IsGenerated.ToString(CultureInfo.InvariantCulture),
                ["score"] = item.Score.ToString(CultureInfo.InvariantCulture)
            };
        }

        private static WwAiWardrobeItemResult? ConvertSearchResultToWardrobeItem(SearchResult searchResult)
        {
            if (searchResult.Payload == null)
                return null;

            var item = new WwAiWardrobeItemResult
            {
                Id = ReadPayloadString(searchResult.Payload, "id"),
                Type = ReadPayloadString(searchResult.Payload, "type"),
                CategoryKey = ReadPayloadString(searchResult.Payload, "categoryKey"),
                Label = ReadPayloadString(searchResult.Payload, "label"),
                ImageUrl = ReadPayloadString(searchResult.Payload, "imageUrl"),
                GeneratedImageUrl = ReadPayloadString(searchResult.Payload, "generatedImageUrl"),
                CroppedImageUrl = ReadPayloadString(searchResult.Payload, "croppedImageUrl"),
                SourceImageUrl = ReadPayloadString(searchResult.Payload, "sourceImageUrl"),
                IsDetected = string.Equals(ReadPayloadString(searchResult.Payload, "isDetected"), "True", StringComparison.OrdinalIgnoreCase),
                IsGenerated = string.Equals(ReadPayloadString(searchResult.Payload, "isGenerated"), "True", StringComparison.OrdinalIgnoreCase),
                IsIndexed = true,
                Score = searchResult.Score
            };

            if (string.IsNullOrWhiteSpace(item.Id))
                item.Id = searchResult.Id;

            if (string.IsNullOrWhiteSpace(item.CategoryKey))
                item.CategoryKey = "uncategorized";

            return item;
        }

        private List<WwAiWardrobeItemResult> LoadRecentWardrobeItemsFromStorage()
        {
            var roots = new[]
            {
                Path.Combine(_environment.ContentRootPath, StorageRoot, "WW", "Closet", "Generated"),
                Path.Combine(_environment.ContentRootPath, StorageRoot, "WW", "Closet", "Cropped"),
                Path.Combine(_environment.ContentRootPath, StorageRoot, "WW", "Closet", "Original")
            };

            var files = roots
                .Where(Directory.Exists)
                .SelectMany(root => Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
                .Where(IsSupportedImagePath)
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .Take(8)
                .ToList();

            var categories = new[] { "tops", "bottoms", "outerwear", "shoes", "accessories", "uncategorized" };
            var items = new List<WwAiWardrobeItemResult>();

            for (var i = 0; i < files.Count; i++)
            {
                var imageUrl = ResolveStorageUrlFromPath(files[i].FullName);
                if (string.IsNullOrWhiteSpace(imageUrl))
                    continue;

                var categoryKey = categories[Math.Min(i, categories.Length - 1)];
                items.Add(new WwAiWardrobeItemResult
                {
                    Id = $"ww_file_{Path.GetFileNameWithoutExtension(files[i].Name)}",
                    Type = "wardrobe_item",
                    CategoryKey = categoryKey,
                    Label = $"Closet reference {i + 1}",
                    ImageUrl = imageUrl,
                    CroppedImageUrl = imageUrl,
                    SourceImageUrl = imageUrl,
                    CreatedAt = files[i].LastWriteTimeUtc,
                    IsGenerated = files[i].FullName.Contains($"{Path.DirectorySeparatorChar}Generated{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase),
                    IsIndexed = false
                });
            }

            return items;
        }

        private static string ReadPayloadString(Dictionary<string, object> payload, string key)
        {
            if (!payload.TryGetValue(key, out var value) || value == null)
                return string.Empty;

            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.String => jsonElement.GetString() ?? string.Empty,
                    JsonValueKind.Number => jsonElement.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => jsonElement.GetRawText()
                };
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static List<WwAiWardrobeItemResult> PickOutfitItems(List<WwAiWardrobeItemResult> items)
        {
            var order = new[] { "tops", "bottoms", "outerwear", "shoes", "accessories" };
            var picked = new List<WwAiWardrobeItemResult>();

            foreach (var categoryKey in order)
            {
                var item = items.FirstOrDefault(i =>
                    string.Equals(i.CategoryKey, categoryKey, StringComparison.OrdinalIgnoreCase));

                if (item != null)
                    picked.Add(item);
            }

            if (picked.Count == 0)
                picked.AddRange(items.Take(4));

            return picked;
        }

        private static string BuildOutfitName(WwAiOutfitRequest request)
        {
            var occasion = string.IsNullOrWhiteSpace(request.Occasion) ? "Daily" : request.Occasion.Trim();
            return $"{occasion} Weather Look";
        }

        private static string BuildOutfitReason(WwAiOutfitRequest request, List<WwAiWardrobeItemResult> items)
        {
            var names = string.Join(", ", items.Select(item => item.Label).Where(label => !string.IsNullOrWhiteSpace(label)));
            var weather = string.IsNullOrWhiteSpace(request.WeatherSummary) ? "today's weather" : request.WeatherSummary;
            var occasion = string.IsNullOrWhiteSpace(request.Occasion) ? "your plan" : request.Occasion;
            return $"Based on {weather} and {occasion}, I picked {names}.";
        }

        private static List<DetectionOutput> ExpandFullBodyWardrobeOutputs(
            byte[] imageBytes,
            List<DetectionOutput> outputs,
            string imageId)
        {
            if (outputs.Count == 0)
                return outputs;

            try
            {
                using var image = ImageUtils.DecodeImage(imageBytes);
                if (!LooksLikeFullBodyImage(image))
                    return outputs;

                var categories = outputs
                    .Select(output => ResolveCategoryKey(output.Label))
                    .Where(category => !string.Equals(category, "uncategorized", StringComparison.OrdinalIgnoreCase))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var expanded = outputs.ToList();
                if (!categories.Contains("bottoms"))
                {
                    expanded.Add(CreateHeuristicDetectionOutput(
                        image,
                        $"{imageId}_obj_body_bottoms",
                        "pants",
                        0.12f,
                        0.24f,
                        0.46f,
                        0.52f,
                        0.37f));
                }

                if (!categories.Contains("shoes"))
                {
                    expanded.Add(CreateHeuristicDetectionOutput(
                        image,
                        $"{imageId}_obj_body_shoes",
                        "shoes",
                        0.10f,
                        0.08f,
                        0.76f,
                        0.72f,
                        0.22f));
                }

                return expanded;
            }
            catch
            {
                return outputs;
            }
        }

        private static bool LooksLikeFullBodyImage(Image<Rgb24> image)
        {
            if (image.Height < 720)
                return false;

            return image.Height >= image.Width * 1.25;
        }

        private static DetectionOutput CreateHeuristicDetectionOutput(
            Image<Rgb24> image,
            string id,
            string label,
            float score,
            float xRatio,
            float yRatio,
            float widthRatio,
            float heightRatio)
        {
            var box = new HNB.IntelligentSystems.GroundingDINO.Models.Rectangle(
                (int)Math.Round(image.Width * xRatio),
                (int)Math.Round(image.Height * yRatio),
                (int)Math.Round(image.Width * widthRatio),
                (int)Math.Round(image.Height * heightRatio));

            using var croppedImage = ImageUtils.CropBox(image, box);
            var croppedImageBytes = ImageUtils.EncodeImage(croppedImage, ".jpg");

            return new DetectionOutput
            {
                Id = id,
                Box = [box.X, box.Y, box.Width, box.Height],
                Label = label,
                Score = score,
                CroppedImageBytes = croppedImageBytes
            };
        }

        private static List<DetectionResult> MergeDuplicateDetections(List<DetectionResult> detections)
        {
            var ordered = detections
                .Where(detection => detection.Box.Width > 0 && detection.Box.Height > 0)
                .OrderByDescending(detection => detection.Score)
                .ToList();

            var merged = new List<DetectionResult>();
            foreach (var detection in ordered)
            {
                var categoryKey = ResolveCategoryKey(detection.Label);
                var overlapsExisting = merged.Any(existing =>
                    string.Equals(ResolveCategoryKey(existing.Label), categoryKey, StringComparison.OrdinalIgnoreCase) &&
                    CalculateIoU(existing.Box, detection.Box) > 0.45f);

                if (!overlapsExisting)
                    merged.Add(detection);
            }

            return merged;
        }

        private static float CalculateIoU(
            HNB.IntelligentSystems.GroundingDINO.Models.Rectangle first,
            HNB.IntelligentSystems.GroundingDINO.Models.Rectangle second)
        {
            var x1 = Math.Max(first.X, second.X);
            var y1 = Math.Max(first.Y, second.Y);
            var x2 = Math.Min(first.X + first.Width, second.X + second.Width);
            var y2 = Math.Min(first.Y + first.Height, second.Y + second.Height);

            var intersectionWidth = Math.Max(0, x2 - x1);
            var intersectionHeight = Math.Max(0, y2 - y1);
            var intersection = intersectionWidth * intersectionHeight;
            if (intersection == 0)
                return 0;

            var firstArea = Math.Max(0, first.Width) * Math.Max(0, first.Height);
            var secondArea = Math.Max(0, second.Width) * Math.Max(0, second.Height);
            var union = firstArea + secondArea - intersection;
            if (union <= 0)
                return 0;

            return (float)intersection / union;
        }

        private static string BuildWardrobeDisplayLabel(string detectedLabel, byte[] croppedImageBytes, string sourceFileName)
        {
            var garmentName = ResolveDisplayGarmentName(detectedLabel, sourceFileName);
            var colorName = TryDescribeDominantColor(croppedImageBytes);

            if (string.IsNullOrWhiteSpace(colorName))
                return garmentName;

            return $"{colorName}{garmentName}";
        }

        private static string ResolveDisplayGarmentName(string detectedLabel, string sourceFileName)
        {
            var text = $"{detectedLabel} {sourceFileName}".ToLowerInvariant();

            if (ContainsAny(text, "shoe", "shoes", "sneaker", "boot", "boots", "loafer", "heel"))
                return "鞋子";

            if (ContainsAny(text, "pant", "pants", "trouser", "trousers", "jean", "jeans"))
                return "長褲";

            if (ContainsAny(text, "skirt"))
                return "裙子";

            if (ContainsAny(text, "short", "shorts"))
                return "短褲";

            if (ContainsAny(text, "jacket", "coat", "blazer", "cardigan", "hoodie", "outer"))
                return "外套";

            if (ContainsAny(text, "bag"))
                return "包包";

            if (ContainsAny(text, "belt"))
                return "皮帶";

            if (ContainsAny(text, "hat", "cap"))
                return "帽子";

            if (ContainsAny(text, "scarf"))
                return "圍巾";

            if (ContainsAny(text, "shirt", "blouse", "polo", "top", "tee", "tshirt", "t shirt"))
                return "襯衫";

            if (ContainsAny(text, "sweater", "knit"))
                return "毛衣";

            if (ContainsAny(text, "accessory", "accessories"))
                return "配件";

            return "衣物";
        }

        private static string TryDescribeDominantColor(byte[] imageBytes)
        {
            try
            {
                using var image = ImageUtils.DecodeImage(imageBytes);

                // 先計算整張裁切圖平均色，若判斷為白色就直接回傳白色
                var allPixels = AccumulateColor(image, skipPlainBackground: false);
                if (allPixels.Count == 0)
                    return string.Empty;

                var allR = allPixels.R / allPixels.Count;
                var allG = allPixels.G / allPixels.Count;
                var allB = allPixels.B / allPixels.Count;

                if (DescribeRgbColor(allR, allG, allB) == "白色")
                    return "白色";

                // 非白色時排除背景與低飽和像素，再估算主要色
                var selected = AccumulateColor(image, skipPlainBackground: true);
                if (selected.Count < 40)
                    selected = allPixels;

                var r = selected.R / selected.Count;
                var g = selected.G / selected.Count;
                var b = selected.B / selected.Count;
                return DescribeRgbColor(r, g, b);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static ColorAccumulator AccumulateColor(Image<Rgb24> image, bool skipPlainBackground)
        {
            var accumulator = new ColorAccumulator();
            var stepX = Math.Max(1, image.Width / 80);
            var stepY = Math.Max(1, image.Height / 80);

            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y += stepY)
                {
                    var row = accessor.GetRowSpan(y);
                    for (var x = 0; x < row.Length; x += stepX)
                    {
                        var pixel = row[x];
                        var max = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B));
                        var min = Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));
                        var brightness = (pixel.R + pixel.G + pixel.B) / 3;
                        var saturation = max - min;

                        if (skipPlainBackground)
                        {
                            // 排除過亮且低飽和，通常是背景白牆或打光
                            if (brightness > 230 && saturation < 35)
                                continue;
                            // 排除近灰階的像素，避免背景干擾主色判斷
                            if (saturation < 18)
                                continue;
                        }

                        accumulator.Add(pixel.R, pixel.G, pixel.B);
                    }
                }
            });

            return accumulator;
        }

        private static string DescribeRgbColor(long r, long g, long b)
        {
            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));
            var brightness = (r + g + b) / 3;
            var saturation = max - min;

            // 白色：高亮且低飽和
            if (brightness >= 215 && saturation < 40)
                return "白色";

            if (brightness <= 50)
                return "黑色";

            // 灰階：低飽和
            if (saturation <= 22)
                return brightness >= 150 ? "灰色" : "深灰色";

            // 明度前綴（淺/深）
            var tone = brightness >= 190 ? "淺" : brightness <= 90 ? "深" : string.Empty;

            // 紅色 / 橘色
            if (r > g + 38 && r > b + 38)
                return g > b + 28 ? $"{tone}橘色" : $"{tone}紅色";

            // 黃色
            if (r > 140 && g > 140 && b < r - 45 && b < g - 45)
                return $"{tone}黃色";

            // 綠色
            if (g > r + 18 && g > b + 18)
                return $"{tone}綠色";

            // 藍色
            if (b > r + 20 && b >= g)
                return $"{tone}藍色";

            // 紫色
            if (r > 120 && b > 120 && g < r - 15 && g < b + 10 && b > r - 30)
                return $"{tone}紫色";

            // 粉色
            if (r > 170 && b > 120 && g < r - 25 && g < 165)
                return "粉色";

            // 米色 / 卡其色
            if (r > 140 && g > 115 && g > b + 20 && r > b + 45 && saturation < 75)
                return brightness >= 170 ? "米色" : "卡其色";

            // 棕色
            if (r > 90 && g > 50 && b < 95 && r > g + 12)
                return "棕色";

            // fallback：以最大色相通道決定主色
            if (max == r) return $"{tone}紅色";
            if (max == g) return $"{tone}綠色";
            if (max == b) return $"{tone}藍色";

            return string.Empty;
        }

        private struct ColorAccumulator
        {
            public long R { get; private set; }

            public long G { get; private set; }

            public long B { get; private set; }

            public long Count { get; private set; }

            public void Add(byte r, byte g, byte b)
            {
                R += r;
                G += g;
                B += b;
                Count++;
            }
        }

        private static string NormalizeWardrobeLabel(string? detectedLabel, string sourceFileName)
        {
            var label = (detectedLabel ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(label) &&
                !string.Equals(label, "uncategorized", StringComparison.OrdinalIgnoreCase))
            {
                return label;
            }

            return InferWardrobeLabel(sourceFileName);
        }

        private static string InferWardrobeLabel(string sourceFileName)
        {
            var text = Path.GetFileNameWithoutExtension(sourceFileName ?? string.Empty)
                .Replace('_', ' ')
                .Replace('-', ' ')
                .ToLowerInvariant();

            if (ContainsAny(text, "shoe", "shoes", "sneaker", "boot", "boots", "heel", "heels", "loafer"))
                return "shoes";

            if (ContainsAny(text, "pant", "pants", "trouser", "trousers", "jean", "jeans", "skirt", "short", "shorts"))
                return "bottom";

            if (ContainsAny(text, "jacket", "coat", "blazer", "cardigan", "hoodie", "outer"))
                return "jacket";

            if (ContainsAny(text, "bag", "belt", "hat", "cap", "scarf", "watch", "accessory", "accessories"))
                return "accessory";

            if (ContainsAny(text, "shirt", "top", "tee", "tshirt", "t shirt", "blouse", "sweater", "knit", "polo"))
                return "top";

            return "top";
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private string BuildClipModelUnavailableMessage(bool includeTokenizerFiles)
        {
            var modelDirectory = Path.Combine(
                _environment.ContentRootPath,
                StorageRoot,
                "Ming",
                "AI",
                "openai",
                "clip-vit-base-patch32-onnx");

            var requiredFiles = new List<string>
            {
                "clip_vision_encoder.onnx",
                "clip_text_encoder.onnx",
                "clip_vision_projection.onnx",
                "clip_text_projection.onnx"
            };

            if (includeTokenizerFiles)
            {
                requiredFiles.Add("vocab.json");
                requiredFiles.Add("merges.txt");
            }

            var missingFiles = requiredFiles
                .Where(file => !File.Exists(Path.Combine(modelDirectory, file)))
                .ToList();

            if (missingFiles.Count == 0)
                return "Embedding returned an empty vector. Check CLIP model compatibility.";

            return $"CLIP embedding model is not ready. Missing files under {modelDirectory}: {string.Join(", ", missingFiles)}";
        }

        private static string BuildGroundingDinoUnavailableMessage(
            HNB.IntelligentSystems.GroundingDINO.Core.ModelHealthChecker.ModelStatusInfo status)
        {
            if (status.IsDownloading)
            {
                return $"GroundingDINO model is downloading ({status.OverallProgress:0.##}%). Using original image for review.";
            }

            if (!status.ModelFileExists || !status.VocabFileExists)
            {
                return $"GroundingDINO model files are missing. Using original image for review. Model: {status.ModelPath}; Vocab: {status.VocabPath}";
            }

            if (!string.IsNullOrWhiteSpace(status.InitializationError))
            {
                return $"GroundingDINO is not ready. Using original image for review. {status.InitializationError}";
            }

            return $"GroundingDINO is not ready. Using original image for review. {status.Message}";
        }

        private static string ResolveCategoryKey(string label)
        {
            var text = (label ?? string.Empty).ToLowerInvariant();

            if (text.Contains("shoe") || text.Contains("boot") || text.Contains("sneaker"))
                return "shoes";

            if (text.Contains("bottom") || text.Contains("pant") || text.Contains("trouser") || text.Contains("jean") || text.Contains("skirt") || text.Contains("short"))
                return "bottoms";

            if (text.Contains("jacket") || text.Contains("coat") || text.Contains("outer"))
                return "outerwear";

            if (text.Contains("bag") || text.Contains("accessory") || text.Contains("belt") || text.Contains("hat"))
                return "accessories";

            if (text.Contains("shirt") || text.Contains("top") || text.Contains("tee") || text.Contains("blouse") || text.Contains("sweater"))
                return "tops";

            return "uncategorized";
        }

        private async Task<string> SaveImageBytesAsync(
            byte[] imageBytes,
            string relativeFolder,
            string extension,
            CancellationToken cancellationToken)
        {
            var safeExtension = NormalizeImageExtension(extension);
            var dateText = DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.InvariantCulture);
            var storageDirectory = Path.Combine(_environment.ContentRootPath, StorageRoot, relativeFolder, dateText);
            Directory.CreateDirectory(storageDirectory);

            var fileName = $"{Guid.NewGuid():N}{safeExtension}";
            var filePath = Path.Combine(storageDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

            return $"/storage/{relativeFolder.Replace("\\", "/")}/{dateText}/{fileName}";
        }

        private string? ResolveStorageUrlFromPath(string path)
        {
            var storageRootPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, StorageRoot));
            var fullPath = Path.GetFullPath(path);

            if (!fullPath.StartsWith(storageRootPath, StringComparison.OrdinalIgnoreCase))
                return null;

            var relative = Path.GetRelativePath(storageRootPath, fullPath).Replace("\\", "/");
            return $"/storage/{relative}";
        }

        private string? ResolveStoragePathFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/storage/", StringComparison.OrdinalIgnoreCase))
                return null;

            var relative = url["/storage/".Length..]
                .Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            return Path.Combine(_environment.ContentRootPath, StorageRoot, relative);
        }

        private static async Task<byte[]> ReadFileBytesAsync(IFormFile file, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }
        }

        private static string NormalizeImageExtension(string extension)
        {
            var value = string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension.ToLowerInvariant();

            return value switch
            {
                ".png" => ".png",
                ".webp" => ".webp",
                ".gif" => ".gif",
                ".jpeg" => ".jpg",
                ".jpg" => ".jpg",
                _ => ".jpg"
            };
        }

        private static bool IsImageFile(IFormFile file)
        {
            return file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSupportedImagePath(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension is ".jpg" or ".jpeg" or ".png" or ".webp";
        }
    }
}





