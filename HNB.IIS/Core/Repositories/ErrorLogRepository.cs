using HNB.IIS.Core.Models.Hnbdata;

namespace HNB.IIS.Core.Repositories;

public class ErrorLogRepository(HnbdataDbContext db)
{
    private IQueryable<error_log> ValidErrorLogs => db.error_logs;

    public bool InsertErrorLog(error_log log)
    {
        db.error_logs.Add(log);
        return db.SaveChanges() > 0;
    }

    public int QueryErrorLogCount() => ValidErrorLogs.Count();

    public List<error_log> QueryErrorLogList(int count = 100) => 
        ValidErrorLogs
            .OrderByDescending(x => x.created_at)
            .Take(count)
            .ToList();
}

