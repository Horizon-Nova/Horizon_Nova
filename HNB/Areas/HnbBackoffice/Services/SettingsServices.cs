using HNB.Areas.HnbBackoffice.Repositories;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.HnbBackoffice.Services;

public class SettingsServices(SettingsRepositories rep)
{
    public List<vw_system_config_database> GetVwSystemConfigDatabase()
        => rep.VwSystemConfigDatabaseMapping();
    public List<vw_system_config_notification> GetVwSystemConfigNotification()
        => rep.VwSystemConfigNotificationMapping();
    public List<vw_system_config_security> GetVwSystemConfigSecurity()
        => rep.VwSystemConfigSecurityMapping();
    public List<vw_system_config_server> GetVwSystemConfigServer()
        => rep.VwSystemConfigServerMapping();
    public List<vw_system_config_system> GetVwSystemConfigSystem()
        => rep.VwSystemConfigSystemMapping();
    public List<system_config> GetSystemConfig()
        => rep.SystemConfigMapping();
}
