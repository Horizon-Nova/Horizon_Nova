using Models.HnbHnbBackoffice;

namespace HNB.Areas.HnbBackoffice.Repositories;

public class SettingsRepositories(HnbHnbBackofficeDbContext dbo)
{
    #region 統一的查詢來源

    private IQueryable<vw_system_config_database> ValidVwSystemConfigDatabase
        => dbo.vw_system_config_databases;
    private IQueryable<vw_system_config_notification> ValidVwSystemConfigNotification
        => dbo.vw_system_config_notifications;
    private IQueryable<vw_system_config_security> ValidVwSystemConfigSecurity
        => dbo.vw_system_config_securities;
    private IQueryable<vw_system_config_server> ValidVwSystemConfigServer
        => dbo.vw_system_config_servers;
    private IQueryable<vw_system_config_system> ValidVwSystemConfigSystem
        => dbo.vw_system_config_systems;
    private IQueryable<system_config> ValidSystemConfig
        => dbo.system_configs;

    public List<vw_system_config_database> VwSystemConfigDatabaseMapping()
        => ValidVwSystemConfigDatabase?.ToList() ?? new List<vw_system_config_database>();
    public List<vw_system_config_notification> VwSystemConfigNotificationMapping()
        => ValidVwSystemConfigNotification?.ToList() ?? new List<vw_system_config_notification>();
    public List<vw_system_config_security> VwSystemConfigSecurityMapping()
        => ValidVwSystemConfigSecurity?.ToList() ?? new List<vw_system_config_security>();
    public List<vw_system_config_server> VwSystemConfigServerMapping()
        => ValidVwSystemConfigServer?.ToList() ?? new List<vw_system_config_server>();
    public List<vw_system_config_system> VwSystemConfigSystemMapping()
        => ValidVwSystemConfigSystem?.ToList() ?? new List<vw_system_config_system>();
    public List<system_config> SystemConfigMapping()
        => ValidSystemConfig?.ToList() ?? new List<system_config>();

    #endregion
}
