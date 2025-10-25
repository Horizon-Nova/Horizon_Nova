using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;
using Models.Hnbdata;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 系統設定資料存取層，負責處理硬體監控和日誌管理功能
/// </summary>
public class SettingsRepositories(HnbHnbBackofficeDbContext db, HnbdataDbContext hnbdataDb)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的硬體監控查詢來源
    /// </summary>
    private IQueryable<vw_hardware_monitoring> ValidHardwareMonitoring => db.vw_hardware_monitorings;
    
    /// <summary>
    /// 有效的錯誤日誌查詢來源
    /// </summary>
    private IQueryable<error_log> ValidErrorLogs => hnbdataDb.error_logs;
    
    /// <summary>
    /// 有效的存取記錄查詢來源
    /// </summary>
    private IQueryable<access_record> ValidAccessRecords => hnbdataDb.access_records;
    #endregion

    #region 專用查詢方法
    
    /// <summary>
    /// 查詢硬體監控資料
    /// </summary>
    public vw_hardware_monitoring? QueryHardwareMonitoring()
        => ValidHardwareMonitoring.FirstOrDefault();

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
        => days.HasValue 
            ? ValidErrorLogs.Where(x => x.created_at >= DateTime.Now.AddDays(-days.Value)).ToList()
            : ValidErrorLogs.ToList();

    /// <summary>
    /// 查詢存取記錄列表
    /// </summary>
    /// <param name="days">查詢天數（null 表示全部）</param>
    public List<access_record> QueryAccessRecordList(int? days = null)
        => days.HasValue 
            ? ValidAccessRecords.Where(x => x.created_at >= DateTime.Now.AddDays(-days.Value)).ToList()
            : ValidAccessRecords.ToList();

    /// <summary>
    /// 查詢硬體監控設定
    /// </summary>
    public hardware_monitoring? QueryHardwareMonitoringConfig()
        => db.hardware_monitorings.FirstOrDefault();

    #endregion

    #region 基本 CRUD 操作
        
    /// <summary>
    /// 刪除錯誤日誌
    /// </summary>
    public bool DeleteErrorLogs(List<error_log> logs)
    {
        hnbdataDb.error_logs.RemoveRange(logs);
        hnbdataDb.SaveChanges();
        return true;
    }

    /// <summary>
    /// 刪除存取記錄
    /// </summary>
    public bool DeleteAccessRecords(List<access_record> records)
    {
        hnbdataDb.access_records.RemoveRange(records);
        hnbdataDb.SaveChanges();
        return true;
    }

    /// <summary>
    /// 插入硬體監控設定（更新）
    /// </summary>
    public hardware_monitoring? InsertHardwareMonitoringConfig(hardware_monitoring config)
    {
        var existingEntity = QueryHardwareMonitoringConfig();
        if (existingEntity == null)
            return null;

        existingEntity.is_active = config.is_active;
        existingEntity.updated_at = DateTime.Now;
        db.SaveChanges();
        return existingEntity;
    }

    #endregion
    
}
