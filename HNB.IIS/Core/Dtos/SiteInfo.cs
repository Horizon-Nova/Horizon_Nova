namespace HNB.IIS.Core.Dtos;

/// <summary>
/// 站台資訊
/// </summary>
public class SiteInfo
{
    /// <summary>
    /// 站台名稱
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// 站台路徑
    /// </summary>
    public string Path { get; set; } = null!;
    
    /// <summary>
    /// 應用程式集區名稱
    /// </summary>
    public string AppPool { get; set; } = null!;
    
    /// <summary>
    /// 埠號
    /// </summary>
    public int Port { get; set; }
    
    /// <summary>
    /// 運行狀態
    /// </summary>
    public string Status { get; set; } = null!;
    
    /// <summary>
    /// 最後發布時間
    /// </summary>
    public DateTime? LastPublish { get; set; }
    
    /// <summary>
    /// 檔案大小（位元組）
    /// </summary>
    public long FileSize { get; set; }
}

