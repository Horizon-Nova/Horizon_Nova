using System;
using System.Collections.Generic;
using System.Linq;
using HNB.Areas.WW.Models;

namespace HNB.Areas.WW.Utilities
{
    public static class HistoryUtilities
    {
        #region Public Methods

        public static HistoryIndexModel QueryHistoryIndexModel(DateTime nowLocal)
        {
            // 假資料：讓列表有足夠長度以便觀察捲動、篩選與視覺密度。
            var records = new List<HistoryRecordModel>();

            // 今日近況（較貼近真實操作節奏）
            records.AddRange(new[]
            {
                new HistoryRecordModel
                {
                    Id = "rec-001",
                    CreatedAt = nowLocal.AddHours(-2),
                    TypeKey = "import",
                    TypeName = "匯入",
                    Title = "匯入 3 張照片",
                    Description = "已加入提取佇列，等待確認欄位後入庫。",
                    BadgeText = "衣櫃"
                },
                new HistoryRecordModel
                {
                    Id = "rec-002",
                    CreatedAt = nowLocal.AddHours(-1),
                    TypeKey = "extract",
                    TypeName = "提取",
                    Title = "提取完成：2 件待確認",
                    Description = "已推送到「待確認」，可逐筆確認分類與標籤。",
                    BadgeText = "衣櫃"
                },
                new HistoryRecordModel
                {
                    Id = "rec-003",
                    CreatedAt = nowLocal.AddMinutes(-35),
                    TypeKey = "ai_outfit",
                    TypeName = "AI 生成",
                    Title = "生成穿搭：雨天通勤",
                    Description = "已產生今日穿搭建議與原因說明。",
                    BadgeText = "Today"
                },
                new HistoryRecordModel
                {
                    Id = "rec-004",
                    CreatedAt = nowLocal.AddMinutes(-12),
                    TypeKey = "save",
                    TypeName = "保存",
                    Title = "保存穿搭到紀錄",
                    Description = "已保存本次穿搭組合，方便日後回看與複用。",
                    BadgeText = "Today"
                }
            });

            // 近 10 天的操作軌跡（匯入/提取/生成/保存混合）
            var recordIndex = 5;
            var typePlan = new (string TypeKey, string TypeName, string BadgeText, string Title, string Description)[]
            {
                ("import", "匯入", "衣櫃", "匯入 5 張照片", "已加入提取佇列，等待提取與確認。"),
                ("extract", "提取", "衣櫃", "提取完成：4 件待確認", "已完成欄位提取，請確認分類、顏色與標籤。"),
                ("ai_outfit", "AI 生成", "Today", "生成穿搭：上班俐落", "已產生穿搭建議，可進一步微調並保存。"),
                ("save", "保存", "Today", "保存穿搭", "已保存本次穿搭，方便日後回看與複用。")
            };

            for (var dayOffset = 1; dayOffset <= 10; dayOffset++)
            {
                // 每天 2 筆，時間分散，避免都擠在同一刻。
                var dayBase = nowLocal.Date.AddDays(-dayOffset).AddHours(9);

                var planA = typePlan[(dayOffset * 2) % typePlan.Length];
                var planB = typePlan[(dayOffset * 2 + 1) % typePlan.Length];

                records.Add(new HistoryRecordModel
                {
                    Id = $"rec-{recordIndex:000}",
                    CreatedAt = dayBase.AddHours(2),
                    TypeKey = planA.TypeKey,
                    TypeName = planA.TypeName,
                    Title = planA.Title,
                    Description = planA.Description,
                    BadgeText = planA.BadgeText
                });
                recordIndex++;

                records.Add(new HistoryRecordModel
                {
                    Id = $"rec-{recordIndex:000}",
                    CreatedAt = dayBase.AddHours(6).AddMinutes(15),
                    TypeKey = planB.TypeKey,
                    TypeName = planB.TypeName,
                    Title = planB.Title,
                    Description = planB.Description,
                    BadgeText = planB.BadgeText
                });
                recordIndex++;
            }

            return new HistoryIndexModel
            {
                Records = records
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList()
            };
        }

        #endregion
    }
}
