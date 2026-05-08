using System;
using System.Collections.Generic;
using System.Linq;
using HNB.Areas.WW.Models;

namespace HNB.Areas.WW.Utilities
{
    public static class WardrobeUtilities
    {
        #region Public Methods

        public static WardrobeIndexModel QueryWardrobeIndexModel()
        {
            var categories = new List<WardrobeCategoryModel>
            {
                new() { Key = "tops", Name = "上衣" },
                new() { Key = "bottoms", Name = "下身" },
                new() { Key = "outerwear", Name = "外套" },
                new() { Key = "shoes", Name = "鞋子" },
                new() { Key = "accessories", Name = "配件" }
            };

            var items = new List<WardrobeItemModel>
            {
                new()
                {
                    Id = "it-001",
                    Name = "白色襯衫",
                    Type = "襯衫",
                    CategoryKey = "tops",
                    CategoryName = "上衣",
                    PrimaryColor = "白",
                    Season = "四季",
                    Occasion = "上班",
                    Tags = ["正式", "百搭"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-002",
                    Name = "米色針織上衣",
                    Type = "針織",
                    CategoryKey = "tops",
                    CategoryName = "上衣",
                    PrimaryColor = "米",
                    Season = "秋冬",
                    Occasion = "日常",
                    Tags = ["舒適", "保暖"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-003",
                    Name = "黑色素 T",
                    Type = "T 恤",
                    CategoryKey = "tops",
                    CategoryName = "上衣",
                    PrimaryColor = "黑",
                    Season = "春夏",
                    Occasion = "日常",
                    Tags = ["簡約", "百搭"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-004",
                    Name = "淺藍牛仔襯衫",
                    Type = "襯衫",
                    CategoryKey = "tops",
                    CategoryName = "上衣",
                    PrimaryColor = "淺藍",
                    Season = "春秋",
                    Occasion = "日常",
                    Tags = ["休閒", "耐穿"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-005",
                    Name = "深藍防潑水外套",
                    Type = "外套",
                    CategoryKey = "outerwear",
                    CategoryName = "外套",
                    PrimaryColor = "深藍",
                    Season = "秋冬",
                    Occasion = "通勤",
                    Tags = ["防潑水", "通勤"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-006",
                    Name = "淺卡其風衣",
                    Type = "風衣",
                    CategoryKey = "outerwear",
                    CategoryName = "外套",
                    PrimaryColor = "卡其",
                    Season = "春秋",
                    Occasion = "上班",
                    Tags = ["防風", "俐落"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-007",
                    Name = "灰色連帽外套",
                    Type = "連帽外套",
                    CategoryKey = "outerwear",
                    CategoryName = "外套",
                    PrimaryColor = "灰",
                    Season = "秋冬",
                    Occasion = "日常",
                    Tags = ["舒適", "好搭"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-008",
                    Name = "卡其長褲",
                    Type = "長褲",
                    CategoryKey = "bottoms",
                    CategoryName = "下身",
                    PrimaryColor = "卡其",
                    Season = "四季",
                    Occasion = "上班",
                    Tags = ["俐落", "百搭"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-009",
                    Name = "黑色牛仔褲",
                    Type = "牛仔褲",
                    CategoryKey = "bottoms",
                    CategoryName = "下身",
                    PrimaryColor = "黑",
                    Season = "四季",
                    Occasion = "日常",
                    Tags = ["耐穿", "百搭"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-010",
                    Name = "深灰西裝褲",
                    Type = "西裝褲",
                    CategoryKey = "bottoms",
                    CategoryName = "下身",
                    PrimaryColor = "深灰",
                    Season = "四季",
                    Occasion = "上班",
                    Tags = ["正式", "俐落"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-011",
                    Name = "米白長裙",
                    Type = "長裙",
                    CategoryKey = "bottoms",
                    CategoryName = "下身",
                    PrimaryColor = "米白",
                    Season = "春夏",
                    Occasion = "約會",
                    Tags = ["輕盈", "好拍"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-012",
                    Name = "白色運動鞋",
                    Type = "運動鞋",
                    CategoryKey = "shoes",
                    CategoryName = "鞋子",
                    PrimaryColor = "白",
                    Season = "四季",
                    Occasion = "日常",
                    Tags = ["好走", "百搭"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-013",
                    Name = "棕色皮鞋",
                    Type = "皮鞋",
                    CategoryKey = "shoes",
                    CategoryName = "鞋子",
                    PrimaryColor = "棕",
                    Season = "四季",
                    Occasion = "上班",
                    Tags = ["正式"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-014",
                    Name = "黑色短靴",
                    Type = "短靴",
                    CategoryKey = "shoes",
                    CategoryName = "鞋子",
                    PrimaryColor = "黑",
                    Season = "秋冬",
                    Occasion = "上班",
                    Tags = ["顯瘦", "好搭"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-015",
                    Name = "米色涼鞋",
                    Type = "涼鞋",
                    CategoryKey = "shoes",
                    CategoryName = "鞋子",
                    PrimaryColor = "米",
                    Season = "夏",
                    Occasion = "日常",
                    Tags = ["透氣"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-016",
                    Name = "黑色皮帶",
                    Type = "皮帶",
                    CategoryKey = "accessories",
                    CategoryName = "配件",
                    PrimaryColor = "黑",
                    Season = "四季",
                    Occasion = "上班",
                    Tags = ["正式", "實用"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-017",
                    Name = "灰色圍巾",
                    Type = "圍巾",
                    CategoryKey = "accessories",
                    CategoryName = "配件",
                    PrimaryColor = "灰",
                    Season = "冬",
                    Occasion = "通勤",
                    Tags = ["保暖"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-018",
                    Name = "金色耳環",
                    Type = "飾品",
                    CategoryKey = "accessories",
                    CategoryName = "配件",
                    PrimaryColor = "金",
                    Season = "四季",
                    Occasion = "約會",
                    Tags = ["精緻"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-019",
                    Name = "黑色托特包",
                    Type = "包",
                    CategoryKey = "accessories",
                    CategoryName = "配件",
                    PrimaryColor = "黑",
                    Season = "四季",
                    Occasion = "上班",
                    Tags = ["容量", "實用"],
                    IsPendingReview = false,
                    IsUncategorized = false
                },
                new()
                {
                    Id = "it-020",
                    Name = "未分類：條紋 T 恤",
                    Type = "T 恤",
                    CategoryKey = string.Empty,
                    CategoryName = "未分類",
                    PrimaryColor = "白/黑",
                    Season = "春夏",
                    Occasion = "日常",
                    Tags = ["輕鬆"],
                    IsPendingReview = false,
                    IsUncategorized = true
                }
            };

            // 讓 UI 能先展示「待確認」工作流：這些只是示意資料。
            items.AddRange(new[]
            {
                new WardrobeItemModel
                {
                    Id = "it-p-001",
                    Name = "待確認：新匯入照片 1",
                    Type = "待提取",
                    CategoryKey = string.Empty,
                    CategoryName = "待確認",
                    PrimaryColor = "未知",
                    Season = "未知",
                    Occasion = "未知",
                    Tags = [],
                    IsPendingReview = true,
                    IsUncategorized = false
                },
                new WardrobeItemModel
                {
                    Id = "it-p-002",
                    Name = "待確認：新匯入照片 2",
                    Type = "待提取",
                    CategoryKey = string.Empty,
                    CategoryName = "待確認",
                    PrimaryColor = "未知",
                    Season = "未知",
                    Occasion = "未知",
                    Tags = [],
                    IsPendingReview = true,
                    IsUncategorized = false
                }
            });

            foreach (var category in categories)
            {
                category.Count = items.Count(i => i.CategoryKey == category.Key && !i.IsPendingReview);
            }

            return new WardrobeIndexModel
            {
                Categories = categories,
                Items = items
            };
        }

        #endregion
    }
}
