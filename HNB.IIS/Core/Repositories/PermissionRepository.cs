using HNB.IIS.Core.Models;

namespace HNB.IIS.Core.Repositories;

public class PermissionRepository(HnbdataDbContext db)
{
    public permission_management? QueryUserByName(string name) =>
        db.Set<permission_management>()
            .FirstOrDefault(x => x.type == "user" && x.name == name);
}

