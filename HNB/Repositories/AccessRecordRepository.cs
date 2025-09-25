using Models.Hnbdata;
using Microsoft.EntityFrameworkCore;

namespace HNB.Repositories;

public class AccessRecordRepository
{
    private readonly HnbdataDbContext _db;

    public AccessRecordRepository(HnbdataDbContext db)
    {
        _db = db;
    }

    public async Task InsertAsync(access_record record)
    {
        record.created_at = DateTime.UtcNow;
        _db.access_records.Add(record);
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _db.access_records.CountAsync();
    }

    public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _db.access_records
            .Where(record => record.created_at >= startDate && record.created_at <= endDate)
            .CountAsync();
    }

    public async Task<int> GetCountByLogTypeAsync(string logType)
    {
        return await _db.access_records
            .Where(record => record.log_type == logType)
            .CountAsync();
    }

    public async Task ClearAllAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM dbo.access_records");
    }

    public async Task ClearByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM dbo.access_records WHERE created_at >= {0} AND created_at <= {1}",
            startDate, endDate);
    }

    public async Task ClearOlderThanAsync(DateTime cutoffDate)
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM dbo.access_records WHERE created_at < {0}",
            cutoffDate);
    }

    public async Task ClearByLogTypeAsync(string logType)
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM dbo.access_records WHERE log_type = {0}",
            logType);
    }

    public async Task<List<access_record>> GetRecentRecordsAsync(int count = 100)
    {
        return await _db.access_records
            .OrderByDescending(record => record.created_at)
            .Take(count)
            .ToListAsync();
    }
}
