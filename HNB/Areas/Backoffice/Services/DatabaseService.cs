using HNB.Areas.Backoffice.Dtos;
using HNB.Areas.Backoffice.Utilities;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 資料庫服務 - 統一管理所有資料庫相關業務邏輯
/// </summary>
public class DatabaseService()
{
    #region 連線管理

    /// <summary>
    /// 建立 DbContext 選項建構器
    /// </summary>
    public DbContextOptionsBuilder DbContextOptionsBuilder(string provider, string connectionString)
        => DatabaseUtilities.CreateDbContextOptionsBuilder(provider, connectionString);


    #endregion

    #region 連線測試

    /// <summary>
    /// 測試資料庫連線
    /// </summary>
    public (bool Success, string Message) TestConnection(string provider, string connectionString)
        => DatabaseUtilities.TestConnectionAsync(provider, connectionString).GetAwaiter().GetResult();

    /// <summary>
    /// 載入資料庫中的資料表列表
    /// </summary>
    public (bool Success, List<string> Tables, string Message) LoadDatabaseTables(string provider, string connectionString)
        => DatabaseUtilities.LoadDatabaseTablesAsync(provider, connectionString).GetAwaiter().GetResult();

    /// <summary>
    /// 載入資料表欄位詳情
    /// </summary>
    public (bool Success, List<TableColumnDto> Columns, string Message) LoadTableDetails(string provider, string connectionString, string tableName)
        => DatabaseUtilities.LoadTableDetailsAsync(provider, connectionString, tableName).GetAwaiter().GetResult();


    #endregion

    #region 資料表備份

    /// <summary>
    /// 備份資料庫資料表
    /// </summary>
    public (bool Success, string Message) BackupDatabaseTables(GenerateModelsRequestDto request)
        => DatabaseUtilities.BackupDatabaseTables(request).GetAwaiter().GetResult();

    #endregion

}
