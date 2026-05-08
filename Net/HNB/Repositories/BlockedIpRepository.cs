using Models.Hnb;

namespace HNB.Repositories;

/// <summary>
/// 封鎖 IP 資料存取層，負責處理封鎖 IP 的資料庫操作
/// </summary>
public class BlockedIpRepository(HnbDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 封鎖 IP 的統一查詢來源
    /// </summary>
    private IQueryable<blocked_ip> ValidBlockedIps => db.blocked_ips;
    #endregion

    #region Insert Methods
    /// <summary>
    /// 新增一筆封鎖 IP 記錄
    /// </summary>
    /// <param name="blockedIp">封鎖 IP 物件</param>
    /// <returns>成功返回 true，失敗返回 false</returns>
    public bool Insert(blocked_ip blockedIp)
    {
        db.blocked_ips.Add(blockedIp);
        return db.SaveChanges() > 0;
    }
    #endregion

    #region Query Methods
    /// <summary>
    /// 檢查指定 IP 是否被封鎖
    /// </summary>
    /// <param name="ip">IP 位址</param>
    /// <returns>如果被封鎖返回 true，否則返回 false</returns>
    public bool QueryIsBlocked(string ip)
        => ValidBlockedIps.Any(b => b.ip_address == ip);
    #endregion
}

