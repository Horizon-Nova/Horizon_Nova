using Models.Hnbdata;
using Microsoft.EntityFrameworkCore;

namespace HNB.Repositories;

public class ErrorLogRepository(HnbdataDbContext db)
{
    public async Task InsertAsync(error_log log)
    {
        db.error_logs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await db.error_logs.CountAsync();
    }

    public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await db.error_logs
            .Where(x => x.created_at >= startDate && x.created_at <= endDate)
            .CountAsync();
    }

    public async Task ClearAllAsync()
    {
        var logs = await db.error_logs.ToListAsync();
        db.error_logs.RemoveRange(logs);
        await db.SaveChangesAsync();
    }

    public async Task ClearByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var logs = await db.error_logs
            .Where(x => x.created_at >= startDate && x.created_at <= endDate)
            .ToListAsync();
        db.error_logs.RemoveRange(logs);
        await db.SaveChangesAsync();
    }

    public async Task ClearOlderThanAsync(DateTime cutoffDate)
    {
        var logs = await db.error_logs
            .Where(x => x.created_at < cutoffDate)
            .ToListAsync();
        db.error_logs.RemoveRange(logs);
        await db.SaveChangesAsync();
    }

    public async Task<List<error_log>> GetRecentLogsAsync(int count = 100)
    {
        return await db.error_logs
            .OrderByDescending(x => x.created_at)
            .Take(count)
            .ToListAsync();
    }
}
