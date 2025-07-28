using HNB.Models;

namespace HNB.Repositories;

public class ErrorLogRepository
{
    private readonly HnbdataContext _db;

    public ErrorLogRepository(HnbdataContext db)
    {
        _db = db;
    }

    public async Task InsertAsync(ErrorLog log)
    {
        _db.ErrorLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
