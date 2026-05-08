using Microsoft.EntityFrameworkCore;
using Models.Hnb;
using Models.HnbBackoffice;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 系統設定資料存取層，負責處理硬體監控和日誌管理功能
/// </summary>
public class SettingsRepositories(HnbDbContext hnbDb)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的錯誤日誌查詢來源
    /// </summary>
    private IQueryable<error_log> ValidErrorLogs => hnbDb.error_logs;
    
    /// <summary>
    /// 有效的存取記錄查詢來源
    /// </summary>
    private IQueryable<access_record> ValidAccessRecords => hnbDb.access_records;
    #endregion

    #region 專用查詢方法
    
    /// <summary>
    /// 查詢錯誤日誌數量
    /// </summary>
    public int QueryErrorLogsCount()
        => ValidErrorLogs.Count();

    /// <summary>
    /// 查詢存取記錄數量
    /// </summary>
    public int QueryAccessLogsCount()
        => ValidAccessRecords.Count();

    /// <summary>
    /// 查詢錯誤日誌列表
    /// </summary>
    /// <param name="days">查詢天數（null 表示全部）</param>
    public List<error_log> QueryErrorLogList(int? days = null)
    {
        if (!days.HasValue)
            return ValidErrorLogs.ToList();
        
        var cutoffDate = DateTime.UtcNow.AddDays(-days.Value);
        return ValidErrorLogs
            .Where(x => x.created_at.HasValue && x.created_at.Value >= cutoffDate)
            .ToList();
    }

    /// <summary>
    /// 查詢存取記錄列表
    /// </summary>
    /// <param name="days">查詢天數（null 表示全部）</param>
    public List<access_record> QueryAccessRecordList(int? days = null)
    {
        if (!days.HasValue)
            return ValidAccessRecords.ToList();
        
        var cutoffDate = DateTime.UtcNow.AddDays(-days.Value);
        return ValidAccessRecords
            .Where(x => x.created_at.HasValue && x.created_at.Value >= cutoffDate)
            .ToList();
    }

    #endregion

    #region 基本 CRUD 操作
        
    /// <summary>
    /// 刪除錯誤日誌
    /// </summary>
    public bool DeleteErrorLogs(List<error_log> logs)
    {
        hnbDb.error_logs.RemoveRange(logs);
        hnbDb.SaveChanges();
        return true;
    }

    /// <summary>
    /// 刪除存取記錄
    /// </summary>
    public bool DeleteAccessRecords(List<access_record> records)
    {
        hnbDb.access_records.RemoveRange(records);
        hnbDb.SaveChanges();
        return true;
    }

    #endregion
    
}
