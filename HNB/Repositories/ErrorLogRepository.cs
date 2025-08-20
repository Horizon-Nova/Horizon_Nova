using Models.Hnbdata;

namespace HNB.Repositories;

public class ErrorLogRepository
{
    private readonly HnbdataDbContext _db;

    public ErrorLogRepository(HnbdataDbContext db)
    {
        _db = db;
    }

    public async Task InsertAsync(error_log log)
    {
        _db.error_logs.Add(log);
        await _db.SaveChangesAsync();
    }
}
