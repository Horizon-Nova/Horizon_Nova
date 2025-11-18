namespace HNB.Areas.Backoffice.Dtos;

/// <summary>
/// 建立檔案或資料夾請求
/// </summary>
public class CreateItemRequest
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string>? SharedUsers { get; set; }
}

/// <summary>
/// 刪除檔案或資料夾請求
/// </summary>
public class DeleteItemRequest
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 重新命名檔案或資料夾請求
/// </summary>
public class RenameItemRequest
{
    public string Path { get; set; } = string.Empty;
    public string OldName { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
    public List<string>? SharedUsers { get; set; }
}

/// <summary>
/// 儲存文字檔案請求
/// </summary>
public class SaveTextFileRequest
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Encoding { get; set; } = "utf-8";
}

/// <summary>
/// 檔案管理通用響應
/// </summary>
public class FileManagerResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool ShouldReloadSidebar { get; set; }
    public object? Data { get; set; }
}

/// <summary>
/// 讀取文字檔案響應
/// </summary>
public class ReadTextFileResponse
{
    public bool Success { get; set; }
    public string? Content { get; set; }
    public string? Encoding { get; set; }
    public DateTime? LastModified { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// 上傳檔案響應
/// </summary>
public class UploadResponse
{
    public bool Success { get; set; }
    public int Saved { get; set; }
    public int Failed { get; set; }
    public List<string>? Errors { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 更新資料夾權限請求（舊）
/// </summary>
public class UpdateFolderPermissionsRequest
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] Owners { get; set; } = Array.Empty<string>();
}

/// <summary>
/// 更新檔案/資料夾擁有者請求（分享）
/// </summary>
public class UpdateOwnersRequest
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string>? Owners { get; set; }
}

/// <summary>
/// 生成共享連結請求
/// </summary>
public class GenerateShareLinkRequest
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 資料夾樹狀節點
/// </summary>
public class FolderTreeNode
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<FolderTreeNode> Children { get; set; } = new();
}

/// <summary>
/// 檔案管理主頁面 DTO（用於 Index）
/// </summary>
public class FileManagerIndexDto
{
    public string CurrentPath { get; set; } = "/";
    public string? ViewMode { get; set; }
    public string UserStoragePath { get; set; } = string.Empty;
    public List<FolderTreeNode> UserFolderTree { get; set; } = new();
    public long UsedStorage { get; set; }
    public long TotalStorage { get; set; }
}

    /// <summary>
    /// 檔案管理 DTO
    /// </summary>
    public class FileManagerDto
    {
        /// <summary>
        /// 當前路徑
        /// </summary>
        public string CurrentPath { get; set; } = "/";

        /// <summary>
        /// 視圖模式（shared, recent, favorites, trash, null）
        /// </summary>
        public string? ViewMode { get; set; }

        /// <summary>
        /// 當前使用者名稱
        /// </summary>
        public string? CurrentUser { get; set; }

        /// <summary>
        /// 用戶儲存路徑
        /// </summary>
        public string UserStoragePath { get; set; } = string.Empty;

    /// <summary>
    /// 用戶資料夾列表（用於側邊欄）
    /// </summary>
    public List<FileSystemEntry> UserFolders { get; set; } = new();

    /// <summary>
    /// 用戶資料夾樹狀結構（用於側邊欄）
    /// </summary>
    public List<FolderTreeNode> UserFolderTree { get; set; } = new();

    /// <summary>
    /// 檔案系統項目列表
    /// </summary>
    public List<FileSystemEntry> Items { get; set; } = new();

    /// <summary>
    /// 資料夾數量
    /// </summary>
    public int FolderCount { get; set; }

    /// <summary>
    /// 檔案數量
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// 總大小（位元組）
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// 選中的項目詳細資訊
    /// </summary>
    public FileSystemEntry? SelectedItem { get; set; }
}

