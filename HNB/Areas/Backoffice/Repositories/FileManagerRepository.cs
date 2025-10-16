using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;
using System.Security.Cryptography;
using System.Text;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 檔案管理資料存取層
/// </summary>
public class FileManagerRepository(HnbHnbBackofficeDbContext db)
{
#if DEBUG
    private readonly string _currentMode = "development";
#else
    private readonly string _currentMode = "production";
#endif
    
    #region 統一的查詢來源
    
    /// <summary>
    /// 有效的檔案管理查詢來源（當前環境且未刪除）
    /// </summary>
    private IQueryable<file_manager> ValidFileManagers => db.file_managers
        .Where(fm => fm.is_deleted == false || fm.is_deleted == null)
        .Where(fm => fm.mode == _currentMode);
    
    /// <summary>
    /// 有效的檔案管理視圖查詢來源
    /// </summary>
    private IQueryable<vw_file_manager> ValidFileManagerViews => db.vw_file_managers
        .Where(fm => fm.mode == _currentMode);
    
    #endregion

    #region 專用查詢方法
    
    /// <summary>
    /// 查詢檔案管理列表（根據當前用戶權限過濾）
    /// </summary>
    public List<file_manager> QueryFileManagerList(string? currentUsername = null, string? virtualPath = null, string? itemType = null)
        => ValidFileManagers
            .Where(fm => string.IsNullOrEmpty(virtualPath) || fm.file_path == virtualPath)
            .Where(fm => string.IsNullOrEmpty(itemType) || fm.item_type == itemType)
            .Where(fm => !string.IsNullOrEmpty(currentUsername) && fm.shared_users != null && fm.shared_users.Contains(currentUsername))
            .ToList();

    /// <summary>
    /// 查詢單一檔案管理記錄（不過濾權限，內部使用）
    /// </summary>
    public file_manager? QueryFileManager(string? code = null, string? virtualPath = null, string? fileName = null)
    {
        if (!string.IsNullOrEmpty(code))
            return ValidFileManagers.FirstOrDefault(fm => fm.code == code);
        
        if (!string.IsNullOrEmpty(virtualPath) && !string.IsNullOrEmpty(fileName))
            return ValidFileManagers.FirstOrDefault(fm => fm.file_path == virtualPath && fm.file_name == fileName);
        
        return null;
    }

    /// <summary>
    /// 查詢檔案管理視圖列表
    /// </summary>
    public List<vw_file_manager> QueryFileManagerViewList(string? virtualPath = null)
        => ValidFileManagerViews
            .Where(fm => string.IsNullOrEmpty(virtualPath) || fm.file_path == virtualPath)
            .ToList();

    /// <summary>
    /// 檢查檔案是否存在
    /// </summary>
    public bool QueryFileManagerExists(string virtualPath, string fileName)
        => ValidFileManagers.Any(fm => fm.file_path == virtualPath && fm.file_name == fileName);

    #endregion

    #region 基本 CRUD 操作
    
    /// <summary>
    /// 插入或更新檔案管理記錄（一張表只有一個 Insert 方法）
    /// </summary>
    public file_manager InsertFileManager(file_manager data)
    {
        var existingEntity = QueryFileManager(virtualPath: data.file_path, fileName: data.file_name);
        
        if (existingEntity == null)
        {
            data.code = GenerateUniqueCode(data.file_path ?? "/", data.file_name);
            data.is_deleted = false;
            data.mode = _currentMode;
            data.owner_username = "system";
            db.file_managers.Add(data);
            db.SaveChanges();
            return data;
        }
        
        existingEntity.file_size = data.file_size;
        existingEntity.mime_type = data.mime_type;
        existingEntity.item_type = data.item_type;
        existingEntity.shared_users = data.shared_users ?? existingEntity.shared_users;
        existingEntity.is_deleted = false;
        existingEntity.deleted_at = null;
        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 刪除檔案管理記錄（物理刪除）
    /// </summary>
    public bool DeleteFileManager(string virtualPath, string fileName)
    {
        var entity = QueryFileManager(virtualPath: virtualPath, fileName: fileName);
        if (entity != null)
        {
            db.file_managers.Remove(entity);
            db.SaveChanges();
            return true;
        }
        return false;
    }

    #endregion

    #region 輔助方法

    /// <summary>
    /// 生成唯一的 code
    /// </summary>
    private string GenerateUniqueCode(string virtualPath, string fileName)
    {
        var input = $"{virtualPath}|{fileName}|{_currentMode}|{DateTime.UtcNow.Ticks}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        var code = Convert.ToBase64String(hashBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .Substring(0, 32);
        
        // 確保唯一性
        var counter = 0;
        var finalCode = code;
        while (db.file_managers.Any(fm => fm.code == finalCode))
        {
            finalCode = $"{code}_{++counter}";
        }
        
        return finalCode;
    }

    #endregion
}

