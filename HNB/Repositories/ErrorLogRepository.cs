using Models.Hnbdata;

namespace HNB.Repositories;

public class ErrorLogRepository(HnbdataDbContext db)
{
    public async Task InsertAsync(error_log log)
    {
        db.error_logs.Add(log);
        await db.SaveChangesAsync();
    }
}
