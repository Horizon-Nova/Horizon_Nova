using HNB.IIS.Core.Models.Hnbdata;

namespace HNB.IIS.Core.Repositories;

public class BlockedIpRepository(HnbdataDbContext db)
{
    private IQueryable<blocked_ip> ValidBlockedIps => db.blocked_ips;

    public bool InsertBlockedIp(blocked_ip blockedIp)
    {
        db.blocked_ips.Add(blockedIp);
        return db.SaveChanges() > 0;
    }

    public bool QueryIsBlocked(string ip) => 
        ValidBlockedIps.Any(b => b.ip == ip && (b.expires_at == null || b.expires_at > DateTime.UtcNow));
}

