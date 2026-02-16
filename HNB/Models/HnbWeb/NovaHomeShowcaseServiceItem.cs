namespace Models.HnbWeb;

/// <summary>
/// 首頁展示區單一服務項目
/// </summary>
public class NovaHomeShowcaseServiceItem
{
    public string Key { get; init; } = string.Empty;
    public string Accent { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Eyebrow { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Desc { get; init; } = string.Empty;
    public string[] UseCases { get; init; } = [];
    public string[] Deliverables { get; init; } = [];
    public string[] Tags { get; init; } = [];
    public string Cta { get; init; } = string.Empty;
}
