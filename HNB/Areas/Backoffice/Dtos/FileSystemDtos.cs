namespace HNB.Areas.Backoffice.Dtos;

/// <summary>
/// 檔案系統項目/詳細資訊（統一 DTO）
/// 以單一型別涵蓋列表與詳細檢視所需欄位
/// </summary>
public class FileSystemEntry
{
    /// <summary>
    /// 檔案或資料夾名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 類型："file" 或 "folder"
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 檔案大小（bytes），資料夾為 null
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// MIME 類型（僅檔案）
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// 主要擁有者（保留相容性；等同 PrimaryOwner）
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// 所有擁有者列表（用於 UI 顯示 badge）
    /// </summary>
    public List<string> SharedUsers { get; set; } = new();

    /// <summary>
    /// 建立時間（UTC）
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新時間（UTC）
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 虛擬路徑（例如：/、/folder1）
    /// </summary>
    public string VirtualPath { get; set; } = string.Empty;

    /// <summary>
    /// 最後寫入時間（UTC）- 用於排序
    /// </summary>
    public DateTime? LastWriteUtc { get; set; }

    /// <summary>
    /// 擁有者陣列（用於權限判斷）
    /// </summary>
    public string[] Owners { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 主要擁有者（用於權限判斷）
    /// </summary>
    public string PrimaryOwner { get; set; } = string.Empty;

    /// <summary>
    /// 完整路徑（詳細視圖使用）
    /// </summary>
    public string? FullPath { get; set; }

    /// <summary>
    /// 格式化後的檔案大小（例如：1.5 MB）
    /// </summary>
    public string? FormattedSize { get; set; }

    /// <summary>
    /// 當前用戶是否有權限
    /// </summary>
    public bool HasPermission { get; set; }
}
