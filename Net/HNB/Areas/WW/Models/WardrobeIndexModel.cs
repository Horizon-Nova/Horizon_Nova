using System.Collections.Generic;

namespace HNB.Areas.WW.Models
{
    public class WardrobeIndexModel
    {
        #region Properties

        public List<WardrobeCategoryModel> Categories { get; set; } = [];

        public List<WardrobeItemModel> Items { get; set; } = [];

        #endregion
    }

    public class WardrobeCategoryModel
    {
        #region Properties

        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }

        #endregion
    }

    public class WardrobeItemModel
    {
        #region Properties

        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string CategoryKey { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string PrimaryColor { get; set; } = string.Empty;

        public string Season { get; set; } = string.Empty;

        public string Occasion { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = [];

        public bool IsPendingReview { get; set; }

        public bool IsUncategorized { get; set; }

        #endregion
    }
}

