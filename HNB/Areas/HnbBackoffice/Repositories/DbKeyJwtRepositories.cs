using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;
using System.Net;

namespace HNB.Areas.HnbBackoffice.Repositories;

public class DbKeyJwtRepositories(HnbHnbBackofficeDbContext dbo)
{
    #region 統一的查詢屬性
    private IQueryable<security_ip_key> ValidSecurityIpKeys
        => dbo.security_ip_keys.Where(d => !d.disabled);

    public security_ip_key SecurityIpKeyQuery(IPAddress ip)
        => ValidSecurityIpKeys.Where(x => x.ip_addr == ip).OrderByDescending(x => x.id).First();

    #endregion

    /// <summary> 新增一筆 security_ip_keys </summary>
    public async Task<security_ip_key> SaveAsync(security_ip_key entity, CancellationToken ct = default)
    {
        var existing = await dbo.security_ip_keys
            .FirstOrDefaultAsync(x => x.ip_addr == entity.ip_addr, ct);

        entity = existing is not null
            ? dbo.security_ip_keys.Update(existing).Entity
            : (await dbo.security_ip_keys.AddAsync(entity, ct)).Entity;

        await dbo.SaveChangesAsync(ct);
        return entity;
    }


}
