using HNB.Areas.HNB_WEB.Repositories;
using Models.HnbWeb;

namespace HNB.Areas.HNB_WEB.Services;

/// <summary>
/// 首頁服務層，負責處理頁面所需的資料組裝。
/// </summary>
public class NovaHomeService(NovaHomeRepository repository)
{
    #region 統一的查詢方法

    /// <summary>
    /// 載入首頁展示區服務列表
    /// </summary>
    public List<NovaHomeShowcaseServiceItem> LoadShowcaseServiceList() => repository.QueryShowcaseServiceList();

    #endregion

    #region 頁面模型

    /// <summary>
    /// 載入首頁 Index 畫面模型
    /// </summary>
    public NovaHomeIndexModel LoadNovaHomeModel() => new() { ShowcaseServiceList = LoadShowcaseServiceList() };

    #endregion
}
