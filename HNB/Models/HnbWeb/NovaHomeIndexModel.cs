namespace Models.HnbWeb;

/// <summary>
/// 首頁 Index 畫面所需模型
/// </summary>
public class NovaHomeIndexModel
{
    /// <summary>
    /// 展示區服務列表
    /// </summary>
    public List<NovaHomeShowcaseServiceItem> ShowcaseServiceList { get; init; } = [];
}
