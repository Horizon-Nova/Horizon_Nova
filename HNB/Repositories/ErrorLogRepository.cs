using Models.Hnb;

namespace HNB.Repositories;

/// <summary>
/// 錯誤日誌資料存取層，負責處理錯誤日誌的資料庫操作
/// </summary>
public class ErrorLogRepository(HnbDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 錯誤日誌的統一查詢來源
    /// </summary>
    private IQueryable<error_log> ValidErrorLogs => db.error_logs;
    #endregion

    #region Insert Methods
    /// <summary>
    /// 新增一筆錯誤日誌
    /// </summary>
    /// <param name="log">錯誤日誌物件</param>
    /// <returns>成功返回 true，失敗返回 false</returns>
    public bool Insert(error_log log)
    {
        db.error_logs.Add(log);
        return db.SaveChanges() > 0;
    }
    #endregion

    #region Query Methods
    /// <summary>
    /// 查詢錯誤日誌總數
    /// </summary>
    /// <returns>日誌總數</returns>
    public int QueryCount()
        => ValidErrorLogs.Count();

    /// <summary>
    /// 根據日期範圍查詢錯誤日誌數量
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>符合條件的日誌數量</returns>
    public int QueryCountByDateRange(DateTime startDate, DateTime endDate)
        => ValidErrorLogs
            .Where(x => x.created_at >= startDate && x.created_at <= endDate)
            .Count();

    /// <summary>
    /// 查詢最近的錯誤日誌
    /// </summary>
    /// <param name="count">要取得的數量，預設為 100</param>
    /// <returns>錯誤日誌清單</returns>
    public List<error_log> QueryRecentLogs(int count = 100)
        => ValidErrorLogs
            .OrderByDescending(x => x.created_at)
            .Take(count)
            .ToList();
    #endregion

    #region Delete Methods
    /// <summary>
    /// 清除所有錯誤日誌
    /// </summary>
    /// <returns>成功返回 true，失敗返回 false</returns>
    public bool ClearAll()
    {
        var logs = ValidErrorLogs.ToList();
        if (!logs.Any()) return false;
        
        db.error_logs.RemoveRange(logs);
        return db.SaveChanges() > 0;
    }

    /// <summary>
    /// 清除指定日期範圍內的錯誤日誌
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>成功返回 true，失敗返回 false</returns>
    public bool ClearByDateRange(DateTime startDate, DateTime endDate)
    {
        var logs = ValidErrorLogs
            .Where(x => x.created_at >= startDate && x.created_at <= endDate)
            .ToList();
        if (!logs.Any()) return false;
        
        db.error_logs.RemoveRange(logs);
        return db.SaveChanges() > 0;
    }

    /// <summary>
    /// 清除指定日期之前的錯誤日誌
    /// </summary>
    /// <param name="cutoffDate">截止日期</param>
    /// <returns>成功返回 true，失敗返回 false</returns>
    public bool ClearOlderThan(DateTime cutoffDate)
    {
        var logs = ValidErrorLogs
            .Where(x => x.created_at < cutoffDate)
            .ToList();
        if (!logs.Any()) return false;
        
        db.error_logs.RemoveRange(logs);
        return db.SaveChanges() > 0;
    }
    #endregion
}
