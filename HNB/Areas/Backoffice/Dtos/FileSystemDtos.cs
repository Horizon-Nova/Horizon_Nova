namespace HNB.Areas.Backoffice.Dtos;

/// <summary>
/// 檔案系統項目（直接從檔案系統讀取，不依賴資料庫）
/// </summary>
public class FileSystemItem
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "file" or "folder"
    public long? Size { get; set; }
    public string? MimeType { get; set; }
    public string Owner { get; set; } = string.Empty;
    public List<string> SharedUsers { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string VirtualPath { get; set; } = string.Empty;
    public DateTime? LastWriteUtc { get; set; }
    public string[] Owners { get; set; } = Array.Empty<string>();
    public string PrimaryOwner { get; set; } = string.Empty;
}

/// <summary>
/// 檔案系統詳細資訊
/// </summary>
public class FileSystemDetail
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long? Size { get; set; }
    public DateTime? LastWriteUtc { get; set; }
    public string VirtualPath { get; set; } = string.Empty;
    public string[] Owners { get; set; } = Array.Empty<string>();
    public string PrimaryOwner { get; set; } = string.Empty;
    public bool HasPermission { get; set; }
}
