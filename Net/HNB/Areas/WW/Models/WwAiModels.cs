using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace HNB.Areas.WW.Models
{
    public class WwAiImportResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public List<WwAiWardrobeItemResult> Items { get; set; } = [];

        public List<string> Errors { get; set; } = [];

        public WwAiImportSummary Summary { get; set; } = new();
    }

    public class WwAiImportSummary
    {
        public int TotalImages { get; set; }

        public int DetectedItems { get; set; }

        public int ImportedItems { get; set; }

        public int GeneratedImages { get; set; }

        public int IndexedItems { get; set; }

        public int FailedImages { get; set; }
    }

    public class WwAiWardrobeItemResult
    {
        public string Id { get; set; } = string.Empty;

        public string Type { get; set; } = "wardrobe_item";

        public string CategoryKey { get; set; } = "uncategorized";

        public string Label { get; set; } = string.Empty;

        public float Score { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string? GeneratedImageUrl { get; set; }

        public string CroppedImageUrl { get; set; } = string.Empty;

        public string SourceImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsDetected { get; set; }

        public bool IsGenerated { get; set; }

        public bool IsIndexed { get; set; }

        public string? Error { get; set; }
    }

    public class WwAiOutfitRequest
    {
        public string Occasion { get; set; } = string.Empty;

        public string WeatherSummary { get; set; } = string.Empty;

        public string Temperature { get; set; } = string.Empty;
    }

    public class WwAiOutfitResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string OutfitId { get; set; } = string.Empty;

        public string Occasion { get; set; } = string.Empty;

        public string WeatherSummary { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public string? GeneratedImageUrl { get; set; }

        public List<WwAiWardrobeItemResult> Items { get; set; } = [];

        public List<string> Errors { get; set; } = [];
    }

    public class WwAiSaveLookPhotoRequest
    {
        public IFormFile? Image { get; set; }

        public string OutfitId { get; set; } = string.Empty;

        public string Occasion { get; set; } = string.Empty;

        public string WeatherSummary { get; set; } = string.Empty;
    }

    public class WwAiSaveLookPhotoResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string LookId { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsIndexed { get; set; }

        public string? Error { get; set; }
    }

    /// <summary>
    /// 微調穿搭請求（Tweak）
    /// </summary>
    public class WwAiTweakOutfitRequest
    {
        /// <summary>現有穿搭 ID，用於記錄上下文</summary>
        public string OutfitId { get; set; } = string.Empty;

        /// <summary>原始場合描述</summary>
        public string Occasion { get; set; } = string.Empty;

        /// <summary>現有穿搭名稱（給 GPT 作上下文）</summary>
        public string CurrentOutfitName { get; set; } = string.Empty;

        /// <summary>現有衣物標籤列表（給 GPT 作上下文）</summary>
        public List<string> CurrentItemLabels { get; set; } = [];

        /// <summary>使用者的微調指令，例如 "Warmer"、"More formal"、"No jacket" 或自由文字</summary>
        public string TweakInstruction { get; set; } = string.Empty;
    }

    /// <summary>
    /// 儲存未來日計畫請求
    /// </summary>
    public class WwFuturePlanRequest
    {
        /// <summary>日期文字，格式同前端 data-date（例如 "Tue May 13"）</summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>使用者描述的當天計畫</summary>
        public string Note { get; set; } = string.Empty;
    }

    /// <summary>
    /// 未來日計畫（用於讀取）
    /// </summary>
    public class WwFuturePlanResult
    {
        public string Date { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public string SavedAt { get; set; } = string.Empty;
    }

    /// <summary>
    /// 穿搭歷史單筆紀錄
    /// </summary>
    public class WwOutfitHistoryItem
    {
        public string OutfitId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Occasion { get; set; } = string.Empty;

        public string WeatherSummary { get; set; } = string.Empty;

        public string GeneratedImageUrl { get; set; } = string.Empty;

        public string CreatedAt { get; set; } = string.Empty;
    }

    /// <summary>
    /// 穿搭歷史查詢結果
    /// </summary>
    public class WwOutfitHistoryResult
    {
        public bool Success { get; set; }

        public List<WwOutfitHistoryItem> Items { get; set; } = [];

        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 儲存穿搭到 Calendar 的請求（不需上傳圖片，圖已存在伺服器）
    /// </summary>
    public class WwSaveOutfitLookRequest
    {
        public string OutfitId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Occasion { get; set; } = string.Empty;

        public string WeatherSummary { get; set; } = string.Empty;

        public string GeneratedImageUrl { get; set; } = string.Empty;

        public List<string> ItemLabels { get; set; } = [];

        /// <summary>ISO 日期 yyyy-MM-dd，前端帶入當天</summary>
        public string Date { get; set; } = string.Empty;
    }

    /// <summary>
    /// Calendar 中的單筆穿搭記錄
    /// </summary>
    public class WwCalendarLookResult
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Occasion { get; set; } = string.Empty;

        public string WeatherSummary { get; set; } = string.Empty;

        public string GeneratedImageUrl { get; set; } = string.Empty;

        public List<string> ItemLabels { get; set; } = [];

        public string Date { get; set; } = string.Empty;

        public string SavedAt { get; set; } = string.Empty;
    }
}
