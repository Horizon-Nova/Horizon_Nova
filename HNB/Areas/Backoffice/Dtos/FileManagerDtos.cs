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

