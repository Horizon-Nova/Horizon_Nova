using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

public class SettingsServices(SettingsRepositories rep)
{

    public List<vw_system_config_server> GetVwSystemConfigServer()
        => rep.ValidVwSystemConfigServer();
    
    public List<vw_system_config_system> GetVwSystemConfigSystem()
        => rep.ValidVwSystemConfigSystem();

    public List<vw_system_config_security> GetVwSystemConfigSecurity()
        => rep.ValidVwSystemConfigSecurity();

    public List<vw_system_config_notification> GetVwSystemConfigNotification()
        => rep.ValidVwSystemConfigNotification();

    public List<vw_system_config_database> GetVwSystemConfigDatabase()
        => rep.ValidVwSystemConfigDatabase();

    public List<system_config> GetSystemConfig()
        => rep.ValidSystemConfig();

}
