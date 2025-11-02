namespace HNB.IIS.Core.Dtos;

public class SiteInfo
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string AppPool { get; set; } = null!;
    public int Port { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? LastPublish { get; set; }
    public long FileSize { get; set; }
}

