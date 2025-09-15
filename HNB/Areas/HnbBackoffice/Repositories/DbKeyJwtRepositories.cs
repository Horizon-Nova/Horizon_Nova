using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;
using System.Net;

namespace HNB.Areas.HnbBackoffice.Repositories;

public class DbKeyJwtRepositories(HnbHnbBackofficeDbContext dbo)
{
    #region 共同查詢
    private IQueryable<security_ip_key> ValidSecurityIpKeys => dbo.security_ip_keys.Where(d => !d.disabled);

    /// <summary>依主鍵取得該筆（用於 BuildJwtById）</summary>
    public security_ip_key GetById(long id) => ValidSecurityIpKeys.First(x => x.id == id);

    /// <summary>依 IP 取最新一筆（僅供舊程式或工具用，不建議用於發 Token）</summary>
    public security_ip_key SecurityIpKeyQuery(IPAddress ip)
        => ValidSecurityIpKeys.Where(x => x.ip_addr == ip)
                              .OrderByDescending(x => x.id)
                              .First();
    #endregion

    #region 儲存
    /// <summary>若同 IP 已存在，僅更新非 key 欄位；否則新增一筆</summary>
    public security_ip_key Save(security_ip_key entity)
    {
        var existing = ValidSecurityIpKeys
            .Where(x => x.ip_addr == entity.ip_addr)
            .OrderByDescending(x => x.id)
            .FirstOrDefault();

        if (existing is not null)
        {
            existing.key_components = entity.key_components;
            existing.note = entity.note;
            dbo.security_ip_keys.Update(existing);
            dbo.SaveChanges();
            return existing;
        }
        else
        {
            dbo.security_ip_keys.Add(entity);
            dbo.SaveChanges();
            return entity;
        }
    }
    #endregion
}
