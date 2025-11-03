namespace HNB.IIS.Core.Dtos;

/// <summary>
/// 建立站台請求
/// </summary>
public class CreateSiteRequest
{
    /// <summary>
    /// 站台名稱
    /// </summary>
    public string SiteName { get; set; } = null!;
    
    /// <summary>
    /// 專案路徑
    /// </summary>
    public string Path { get; set; } = null!;
}

