using Models.Hnbdata;
using Microsoft.EntityFrameworkCore;

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
        log.created_at = DateTime.UtcNow;
        _db.error_logs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _db.error_logs.CountAsync();
    }

    public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _db.error_logs
            .Where(log => log.created_at >= startDate && log.created_at <= endDate)
            .CountAsync();
    }

    public async Task ClearAllAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM dbo.error_logs");
    }

    public async Task ClearByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM dbo.error_logs WHERE created_at >= {0} AND created_at <= {1}",
            startDate, endDate);
    }

    public async Task ClearOlderThanAsync(DateTime cutoffDate)
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM dbo.error_logs WHERE created_at < {0}",
            cutoffDate);
    }

    public async Task<List<error_log>> GetRecentLogsAsync(int count = 100)
    {
        return await _db.error_logs
            .OrderByDescending(log => log.created_at)
            .Take(count)
            .ToListAsync();
    }
}
