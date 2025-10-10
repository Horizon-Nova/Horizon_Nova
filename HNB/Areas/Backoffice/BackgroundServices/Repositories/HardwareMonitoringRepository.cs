using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.BackgroundServices.Repositories;

/// <summary>
/// 硬體監控資料存取層，負責處理硬體監控資料的存取功能
/// </summary>
public class HardwareMonitoringRepository(HnbHnbBackofficeDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的硬體監控查詢來源
    /// </summary>
    private IQueryable<hardware_monitoring> ValidHardwareMonitoring => db.hardware_monitorings.Where(h => h.is_active == true).OrderBy(h => h.host_name);
    
    /// <summary>
    /// 有效的硬體監控視圖查詢來源
    /// </summary>
    private IQueryable<vw_hardware_monitoring> ValidHardwareMonitoringView => db.vw_hardware_monitorings.OrderBy(h => h.host_name);
    
    #endregion

    #region 專用查詢方法
    /// <summary>
    /// 查詢硬體監控列表
    /// </summary>
    /// <returns>硬體監控列表</returns>
    public List<vw_hardware_monitoring> QueryHardwareMonitoringList()
        => ValidHardwareMonitoringView.ToList();

    /// <summary>
    /// 查詢單一硬體監控記錄
    /// </summary>
    /// <param name="id">硬體監控ID</param>
    /// <returns>硬體監控記錄或null</returns>
    public vw_hardware_monitoring? QueryHardwareMonitoring(int id)
        => ValidHardwareMonitoringView.FirstOrDefault(h => h.id == id);

    #endregion

    #region 基本 CRUD 操作
    /// <summary>
    /// 插入硬體監控資料（新增或更新）
    /// 使用 server_ip 作為定位鍵，EF Core 會自動判斷新增或更新
    /// </summary>
    public hardware_monitoring InsertHardwareMonitoring(hardware_monitoring hardware)
    {
        var existingEntity = db.hardware_monitorings.FirstOrDefault(h => h.server_ip == hardware.server_ip);
        if (existingEntity == null)
        {
            db.Add(hardware);
            existingEntity = hardware;
        }
        else
        {
            hardware.id = existingEntity.id;
        }

        db.Entry(existingEntity).CurrentValues.SetValues(hardware);
        
        existingEntity.updated_at = DateTime.UtcNow;

        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 刪除硬體監控資料
    /// </summary>
    public bool Delete(int id)
    {
        var entity = db.hardware_monitorings.Find(id);
        if (entity != null)
        {
            db.hardware_monitorings.Remove(entity);
            db.SaveChanges();
            return true;
        }
        return false;
    }

    #endregion
}
