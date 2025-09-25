using HNB.Repositories;
using Models.Hnbdata;

namespace HNB.Services;

public class LogManagementService
{
    private readonly ErrorLogRepository _errorLogRepo;
    private readonly AccessRecordRepository _accessRecordRepo;

    public LogManagementService(ErrorLogRepository errorLogRepo, AccessRecordRepository accessRecordRepo)
    {
        _errorLogRepo = errorLogRepo;
        _accessRecordRepo = accessRecordRepo;
    }

    #region 日誌統計

    public async Task<LogStatistics> GetLogStatisticsAsync()
    {
        var errorLogs = await _errorLogRepo.GetCountAsync();
        var accessLogs = await _accessRecordRepo.GetCountAsync();
        var systemLogs = await _accessRecordRepo.GetCountByLogTypeAsync("system");

        return new LogStatistics
        {
            ErrorLogs = errorLogs,
            AccessLogs = accessLogs,
            SystemLogs = systemLogs
        };
    }

    public async Task<LogStatistics> GetLogStatisticsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var errorLogs = await _errorLogRepo.GetCountByDateRangeAsync(startDate, endDate);
        var accessLogs = await _accessRecordRepo.GetCountByDateRangeAsync(startDate, endDate);

        return new LogStatistics
        {
            ErrorLogs = errorLogs,
            AccessLogs = accessLogs,
            SystemLogs = 0 // 系統日誌通常不按日期範圍統計
        };
    }

    #endregion

    #region 日誌清理

    public async Task<bool> ClearErrorLogsAsync(LogClearOptions options)
    {
        switch (options.ClearType)
        {
            case LogClearType.All:
                await _errorLogRepo.ClearAllAsync();
                break;
            case LogClearType.OlderThan:
                if (options.CutoffDate.HasValue)
                    await _errorLogRepo.ClearOlderThanAsync(options.CutoffDate.Value);
                break;
            case LogClearType.DateRange:
                if (options.StartDate.HasValue && options.EndDate.HasValue)
                    await _errorLogRepo.ClearByDateRangeAsync(options.StartDate.Value, options.EndDate.Value);
                break;
        }
        return true;
    }

    public async Task<bool> ClearAccessLogsAsync(LogClearOptions options)
    {
        switch (options.ClearType)
        {
            case LogClearType.All:
                await _accessRecordRepo.ClearAllAsync();
                break;
            case LogClearType.OlderThan:
                if (options.CutoffDate.HasValue)
                    await _accessRecordRepo.ClearOlderThanAsync(options.CutoffDate.Value);
                break;
            case LogClearType.DateRange:
                if (options.StartDate.HasValue && options.EndDate.HasValue)
                    await _accessRecordRepo.ClearByDateRangeAsync(options.StartDate.Value, options.EndDate.Value);
                break;
            case LogClearType.ByType:
                if (!string.IsNullOrEmpty(options.LogType))
                    await _accessRecordRepo.ClearByLogTypeAsync(options.LogType);
                break;
        }
        return true;
    }

    public async Task<bool> ClearSystemLogsAsync(LogClearOptions options)
    {
        switch (options.ClearType)
        {
            case LogClearType.All:
                await _accessRecordRepo.ClearByLogTypeAsync("system");
                break;
            case LogClearType.OlderThan:
                if (options.CutoffDate.HasValue)
                    await _accessRecordRepo.ClearByDateRangeAsync(DateTime.MinValue, options.CutoffDate.Value);
                break;
        }
        return true;
    }

    #endregion

    #region 日誌查看

    public async Task<List<error_log>> GetRecentErrorLogsAsync(int count = 50)
    {
        return await _errorLogRepo.GetRecentLogsAsync(count);
    }

    public async Task<List<access_record>> GetRecentAccessRecordsAsync(int count = 50)
    {
        return await _accessRecordRepo.GetRecentRecordsAsync(count);
    }

    #endregion
}

#region 支援類別

public class LogStatistics
{
    public int ErrorLogs { get; set; }
    public int AccessLogs { get; set; }
    public int SystemLogs { get; set; }
}

public class LogClearOptions
{
    public LogClearType ClearType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? CutoffDate { get; set; }
    public string? LogType { get; set; }
}

public enum LogClearType
{
    All,
    OlderThan,
    DateRange,
    ByType
}

#endregion
