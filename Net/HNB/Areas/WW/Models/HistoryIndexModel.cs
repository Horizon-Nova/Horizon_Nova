using System;
using System.Collections.Generic;

namespace HNB.Areas.WW.Models
{
    public class HistoryIndexModel
    {
        #region Properties

        public List<HistoryRecordModel> Records { get; set; } = [];

        #endregion
    }

    public class HistoryRecordModel
    {
        #region Properties

        public string Id { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string TypeKey { get; set; } = string.Empty;

        public string TypeName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string BadgeText { get; set; } = string.Empty;

        #endregion
    }
}

