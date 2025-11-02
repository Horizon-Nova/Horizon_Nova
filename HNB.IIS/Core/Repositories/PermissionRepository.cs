using HNB.IIS.Core.Models.HnbHnbBackoffice;

namespace HNB.IIS.Core.Repositories;

public class PermissionRepository(HnbHnbBackofficeDbContext db)
{
    public permission_management? QueryUserByName(string name) =>
        db.permission_managements
            .FirstOrDefault(x => x.type == "user" && x.name == name);
}

