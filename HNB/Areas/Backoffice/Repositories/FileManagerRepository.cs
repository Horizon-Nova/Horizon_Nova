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
    /// 查詢檔案管理列表（支援多欄位過濾）
    /// </summary>
    public List<file_manager> QueryFileManagerList(file_manager? filter = null, string? currentUsername = null)
    {
        var query = ValidFileManagers;
        
        if (filter != null)
        {
            query = query.Where(fm => 
                (string.IsNullOrEmpty(filter.code) || fm.code == filter.code) &&
                (string.IsNullOrEmpty(filter.file_name) || fm.file_name == filter.file_name) &&
                (string.IsNullOrEmpty(filter.file_path) || fm.file_path == filter.file_path) &&
                (string.IsNullOrEmpty(filter.item_type) || fm.item_type == filter.item_type) &&
                (string.IsNullOrEmpty(filter.owner_username) || fm.owner_username == filter.owner_username) &&
                (string.IsNullOrEmpty(filter.mime_type) || fm.mime_type == filter.mime_type) &&
                (string.IsNullOrEmpty(filter.parent_code) || fm.parent_code == filter.parent_code) &&
                (!filter.file_size.HasValue || fm.file_size == filter.file_size.Value)
            );
        }
        
        if (!string.IsNullOrEmpty(currentUsername))
            query = query.Where(fm => fm.shared_users != null && fm.shared_users.Contains(currentUsername));
        
        return query.ToList();
    }

    /// <summary>
    /// 查詢單一檔案管理記錄
    /// </summary>
    public file_manager? QueryFileManager(file_manager filter)
    {
        if (!string.IsNullOrEmpty(filter.code))
            return ValidFileManagers.FirstOrDefault(fm => fm.code == filter.code);
        
        if (!string.IsNullOrEmpty(filter.file_path) && !string.IsNullOrEmpty(filter.file_name))
            return ValidFileManagers.FirstOrDefault(fm => fm.file_path == filter.file_path && fm.file_name == filter.file_name);
        
        if (filter.id > 0)
            return ValidFileManagers.FirstOrDefault(fm => fm.id == filter.id);
        
        return null;
    }

    /// <summary>
    /// 查詢檔案管理視圖列表（支援多欄位過濾）
    /// </summary>
    public List<vw_file_manager> QueryVWFileManagerList(vw_file_manager? filter = null, string? currentUsername = null)
    {
        var query = ValidFileManagerViews;
        
        if (filter != null)
        {
            query = query.Where(fm => 
                (string.IsNullOrEmpty(filter.code) || fm.code == filter.code) &&
                (string.IsNullOrEmpty(filter.file_name) || fm.file_name == filter.file_name) &&
                (string.IsNullOrEmpty(filter.file_path) || fm.file_path == filter.file_path) &&
                (string.IsNullOrEmpty(filter.item_type) || fm.item_type == filter.item_type) &&
                (string.IsNullOrEmpty(filter.owner_username) || fm.owner_username == filter.owner_username) &&
                (string.IsNullOrEmpty(filter.mime_type) || fm.mime_type == filter.mime_type) &&
                (string.IsNullOrEmpty(filter.parent_code) || fm.parent_code == filter.parent_code) &&
                (!filter.file_size.HasValue || fm.file_size == filter.file_size.Value)
            );
        }
        
        if (!string.IsNullOrEmpty(currentUsername))
            query = query.Where(fm => fm.shared_users != null && fm.shared_users.Contains(currentUsername));
        
        return query.ToList();
    }

    /// <summary>
    /// 查詢單一檔案管理視圖記錄
    /// </summary>
    public vw_file_manager? QueryVWFileManager(vw_file_manager filter)
    {
        if (!string.IsNullOrEmpty(filter.code))
            return ValidFileManagerViews.FirstOrDefault(fm => fm.code == filter.code);
        
        if (!string.IsNullOrEmpty(filter.file_path) && !string.IsNullOrEmpty(filter.file_name))
            return ValidFileManagerViews.FirstOrDefault(fm => fm.file_path == filter.file_path && fm.file_name == filter.file_name);
        
        if (filter.id.HasValue && filter.id.Value > 0)
            return ValidFileManagerViews.FirstOrDefault(fm => fm.id == filter.id.Value);
        
        return null;
    }

    /// <summary>
    /// 檢查檔案是否存在
    /// </summary>
    public bool QueryFileManagerExists(string virtualPath, string fileName)
        => ValidFileManagers.Any(fm => fm.file_path == virtualPath && fm.file_name == fileName);

    #endregion

    #region 基本 CRUD 操作
    
    /// <summary>
    /// 完整覆蓋當前環境的檔案記錄
    /// </summary>
    public void InsertFileManagerBatch(List<file_manager> dataList, string? currentUsername = null)
    {
        // 防呆：同批資料先以 (file_path, file_name) 去重，避免上層重複導致唯一鍵衝突
        var distinct = dataList
            .GroupBy(d => new { Path = d.file_path ?? "/", Name = d.file_name })
            .Select(g => g.First())
            .ToList();

        using var tx = db.Database.BeginTransaction();

        // 先清空當前 mode 再寫入，確保鏡像一致
        db.file_managers.Where(fm => fm.mode == _currentMode).ExecuteDelete();

        foreach (var data in distinct)
        {
            data.code = GenerateUniqueCode(data.file_path ?? "/", data.file_name);
            data.is_deleted = false;
            data.mode = _currentMode;
            
            // 如果沒有設置 owner，則使用當前使用者或 system
            if (string.IsNullOrEmpty(data.owner_username))
                data.owner_username = !string.IsNullOrEmpty(currentUsername) ? currentUsername : "system";
            
            // 確保 shared_users 不為空
            if (data.shared_users == null || data.shared_users.Count == 0)
                data.shared_users = new List<string> { !string.IsNullOrEmpty(currentUsername) ? currentUsername : "system" };
        }

        db.file_managers.AddRange(distinct);
        db.SaveChanges();
        tx.Commit();
    }

    /// <summary>
    /// 更新檔案管理記錄
    /// </summary>
    public void UpdateFileManager(file_manager entity)
    {
        db.SaveChanges();
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

