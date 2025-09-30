using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;
using Models.Hnbdata;

namespace HNB.Areas.Backoffice.Repositories;

public class SettingsRepositories(HnbHnbBackofficeDbContext db, HnbdataDbContext hnbdataDb)
{

    public List<vw_system_config_server> ValidVwSystemConfigServer()
        => db.vw_system_config_servers.ToList();

    public List<vw_system_config_system> ValidVwSystemConfigSystem()
        => db.vw_system_config_systems.ToList();

    public List<vw_system_config_security> ValidVwSystemConfigSecurity()
        => db.vw_system_config_securities.ToList();

    public List<vw_system_config_notification> ValidVwSystemConfigNotification()
        => db.vw_system_config_notifications.ToList();

    public List<vw_system_config_database> ValidVwSystemConfigDatabase()
        => db.vw_system_config_databases.ToList();

    public List<system_config> ValidSystemConfig()
        => db.system_configs.ToList();

    public async Task<int> ErrorLogsCountAsync()
        => await hnbdataDb.error_logs.CountAsync();

    public async Task<int> AccessLogsCountAsync()
        => await hnbdataDb.access_records.CountAsync();

    public async Task<bool> ClearErrorLogsAsync(bool is30Days = false)
    {
        try
        {
            var logs = is30Days 
                ? await hnbdataDb.error_logs.Where(x => x.created_at < DateTime.Now.AddDays(-30)).ToListAsync()
                : await hnbdataDb.error_logs.ToListAsync();
            
            hnbdataDb.error_logs.RemoveRange(logs);
            await hnbdataDb.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ClearAccessLogsAsync(bool is30Days = false)
    {
        try
        {
            var records = is30Days 
                ? await hnbdataDb.access_records.Where(x => x.created_at < DateTime.Now.AddDays(-30)).ToListAsync()
                : await hnbdataDb.access_records.ToListAsync();
            
            hnbdataDb.access_records.RemoveRange(records);
            await hnbdataDb.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
