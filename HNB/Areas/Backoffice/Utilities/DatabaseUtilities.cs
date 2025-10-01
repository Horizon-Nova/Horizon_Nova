using HNB.Areas.Backoffice.Dtos;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.Backoffice.Utilities;

/// <summary>
/// 資料庫工具類 - 統一管理所有資料庫相關功能
/// </summary>
public static class DatabaseUtilities
{
    #region 連線測試功能

    /// <summary>
    /// 測試資料庫連線是否正常
    /// </summary>
    /// <param name="provider">資料庫提供者</param>
    /// <param name="connectionString">連線字串</param>
    /// <returns>連線測試結果</returns>
    public static async Task<(bool Success, string Message)> TestConnectionAsync(string provider, string connectionString)
    {
        try
        {
            using var context = new DynamicDbContext(provider, connectionString);
            await context.Database.OpenConnectionAsync();
            await context.Database.CloseConnectionAsync();

            return (true, "連線成功");
        }
        catch (Exception ex)
        {
            return (false, $"連線失敗: {ex.Message}");
        }
    }

    #endregion

    #region 資料表清單功能

    /// <summary>
    /// 載入資料庫中的所有資料表名稱
    /// </summary>
    /// <param name="provider">資料庫提供者</param>
    /// <param name="connectionString">連線字串</param>
    /// <returns>資料表清單</returns>
    public static async Task<(bool Success, List<string> Tables, string Message)> LoadDatabaseTablesAsync(string provider, string connectionString)
    {
        try
        {
            using var context = new DynamicDbContext(provider, connectionString);
            var tables = await context.QueryTableNamesAsync();
            return (true, tables, $"成功載入 {tables.Count} 個資料表");
        }
        catch (Exception ex)
        {
            return (false, new List<string>(), $"載入資料表失敗: {ex.Message}");
        }
    }

    #endregion

    #region 模型生成功能

    /// <summary>
    /// 備份資料庫資料表到指定位置
    /// </summary>
    /// <param name="request">備份請求參數</param>
    /// <returns>備份結果</returns>
    public static async Task<(bool Success, string Message)> BackupDatabaseTables(GenerateModelsRequestDto request)
    {
        try
        {
            var databaseName = ExtractDatabaseName(request.ConnectionString);
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = "Unknown";
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var folderName = $"{databaseName}_{timestamp}";
            var fullOutputPath = Path.Combine(Directory.GetCurrentDirectory(), request.OutputDirectory, folderName);
            Directory.CreateDirectory(fullOutputPath);

            using var context = new DynamicDbContext(request.Provider, request.ConnectionString);
            var tables = await context.QueryTableNamesAsync();

            if (!tables.Any())
            {
                return (false, "資料庫中沒有找到任何資料表");
            }

            var backupResults = new List<string>();

            foreach (var tableName in tables)
            {
                try
                {
                    using var columnContext = new DynamicDbContext(request.Provider, request.ConnectionString);
                    var columns = await columnContext.QueryTableColumnsAsync(tableName);
                    var structureSql = GenerateTableStructureSql(columns, tableName, request.Provider);
                    var structureFile = Path.Combine(fullOutputPath, $"{tableName}_structure.sql");
                    await File.WriteAllTextAsync(structureFile, structureSql);

                    var dataSql = await GenerateTableDataTemplateSql(columnContext, tableName, request.Provider);
                    var dataFile = Path.Combine(fullOutputPath, $"{tableName}_data.sql");
                    await File.WriteAllTextAsync(dataFile, dataSql);

                    backupResults.Add($"✓ {tableName} (結構 + 資料)");
                }
                catch (Exception ex)
                {
                    backupResults.Add($"✗ {tableName} (錯誤: {ex.Message})");
                }
            }

            // 生成備份摘要
            var summaryFile = Path.Combine(fullOutputPath, "backup_summary.txt");
            var summary = $@"資料庫備份摘要
備份時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
資料庫類型: {request.Provider}
資料庫名稱: {databaseName}
備份資料夾: {folderName}
輸出目錄: {request.OutputDirectory}

備份結果:
{string.Join("\n", backupResults)}

總計: {tables.Count} 個資料表
成功: {backupResults.Count(r => r.StartsWith("✓"))} 個
失敗: {backupResults.Count(r => r.StartsWith("✗"))} 個
";
            await File.WriteAllTextAsync(summaryFile, summary);

            return (true, $"資料表備份完成！\n找到 {tables.Count} 個資料表\n備份資料夾: {folderName}\n輸出目錄: {request.OutputDirectory}");
        }
        catch (Exception ex)
        {
            return (false, $"資料表備份過程中發生錯誤: {ex.Message}");
        }
    }

    #endregion

    #region 資料表詳情功能

    /// <summary>
    /// 載入資料表欄位詳情
    /// </summary>
    /// <param name="provider">資料庫提供者</param>
    /// <param name="connectionString">連線字串</param>
    /// <param name="tableName">資料表名稱</param>
    /// <returns>欄位詳情</returns>
    public static async Task<(bool Success, List<TableColumnDto> Columns, string Message)> LoadTableDetailsAsync(string provider, string connectionString, string tableName)
    {
        try
        {
            using var context = new DynamicDbContext(provider, connectionString);
            var columns = await context.QueryTableColumnsAsync(tableName);
            return (true, columns, $"成功獲取資料表 {tableName} 的 {columns.Count} 個欄位");
        }
        catch (Exception ex)
        {
            return (false, new List<TableColumnDto>(), $"載入資料表 {tableName} 詳細資訊失敗: {ex.Message}");
        }
    }

    #endregion

    #region 私有輔助方法

    /// <summary>
    /// 建立通用的 DbContext 選項建構器 (不綁定特定 DbContext)
    /// </summary>
    /// <param name="provider">資料庫提供者</param>
    /// <param name="connectionString">連線字串</param>
    /// <returns>DbContextOptionsBuilder</returns>
    public static DbContextOptionsBuilder CreateDbContextOptionsBuilder(string provider, string connectionString) =>
        provider.ToLower() switch
        {
            "postgresql" => new DbContextOptionsBuilder().UseNpgsql(connectionString),
            "sqlserver" => new DbContextOptionsBuilder().UseSqlServer(connectionString),
            _ => throw new ArgumentException($"不支援的資料庫提供者: {provider}")
        };

    /// <summary>
    /// 生成資料表結構的 SQL 語句
    /// </summary>
    private static string GenerateTableStructureSql(List<TableColumnDto> columns, string tableName, string provider)
    {
        try
        {
            if (!columns.Any())
            {
                return $@"-- 無法獲取資料表 {tableName} 的結構資訊
-- 生成時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }

            var columnDefinitions = new List<string>();

            foreach (var column in columns)
            {
                var columnDef = GenerateColumnDefinition(column, provider);
                columnDefinitions.Add($"    {columnDef}");
            }

            var sql = provider.ToLower() switch
            {
                "postgresql" => $@"-- PostgreSQL 資料表結構: {tableName}
-- 生成時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

-- 刪除現有資料表 (如果存在)
DROP TABLE IF EXISTS ""{tableName}"" CASCADE;

-- 建立資料表結構
CREATE TABLE ""{tableName}"" (
{string.Join(",\n", columnDefinitions)}
);",
                "sqlserver" => $@"-- SQL Server 資料表結構: {tableName}
-- 生成時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

-- 刪除現有資料表 (如果存在)
IF OBJECT_ID('{tableName}', 'U') IS NOT NULL
    DROP TABLE [{tableName}];

-- 建立資料表結構
CREATE TABLE [{tableName}] (
{string.Join(",\n", columnDefinitions)}
);",
                _ => throw new ArgumentException($"不支援的資料庫提供者: {provider}")
            };

            return sql;
        }
        catch (Exception ex)
        {
            return $@"-- 生成資料表結構時發生錯誤: {ex.Message}
-- 資料表: {tableName}
-- 生成時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
    }

    /// <summary>
    /// 生成資料表資料的 SQL 語句
    /// </summary>
    private static async Task<string> GenerateTableDataTemplateSql(DynamicDbContext context, string tableName, string provider)
    {
        try
        {
            // 查詢資料表的實際資料
            var data = await QueryTableData(context, tableName, provider);

            if (!data.Any())
            {
                return $@"-- 資料表資料: {tableName}
-- 生成時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

-- 此資料表沒有資料
-- INSERT INTO ""{tableName}"" (...) VALUES (...);";
            }

            var insertStatements = new List<string>();

            foreach (var row in data)
            {
                var columns = string.Join(", ", row.Keys.Select(k => provider.ToLower() == "postgresql" ? $"\"{k}\"" : $"[{k}]"));
                var values = string.Join(", ", row.Values.Select(v => FormatValue(v)));
                insertStatements.Add($"INSERT INTO \"{tableName}\" ({columns}) VALUES ({values});");
            }

            var sql = $@"-- 資料表資料: {tableName}
-- 生成時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
-- 共 {data.Count} 筆資料

{string.Join("\n", insertStatements)}";

            return sql;
        }
        catch (Exception ex)
        {
            return $@"-- 生成資料表資料時發生錯誤: {ex.Message}
-- 資料表: {tableName}
-- 生成時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
    }

    /// <summary>
    /// 生成欄位定義
    /// </summary>
    private static string GenerateColumnDefinition(TableColumnDto column, string provider)
    {
        var columnName = provider.ToLower() == "postgresql" ? $"\"{column.ColumnName}\"" : $"[{column.ColumnName}]";
        var dataType = ConvertDataType(column.DataType, provider);
        var nullable = column.IsNullable ? "" : " NOT NULL";
        var defaultValue = !string.IsNullOrEmpty(column.DefaultValue) ? $" DEFAULT {column.DefaultValue}" : "";
        var primaryKey = column.IsPrimaryKey ? " PRIMARY KEY" : "";

        return $"{columnName} {dataType}{nullable}{defaultValue}{primaryKey}";
    }

    /// <summary>
    /// 從連線字串中提取資料庫名稱
    /// </summary>
    private static string ExtractDatabaseName(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "Unknown";

        // 嘗試提取 Database 參數
        var databaseMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"Database=([^;]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (databaseMatch.Success)
        {
            return databaseMatch.Groups[1].Value.Trim();
        }

        // 嘗試提取 Initial Catalog 參數 (SQL Server)
        var catalogMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"Initial Catalog=([^;]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (catalogMatch.Success)
        {
            return catalogMatch.Groups[1].Value.Trim();
        }

        return "Unknown";
    }

    /// <summary>
    /// 查詢資料表的實際資料
    /// </summary>
    private static async Task<List<Dictionary<string, object>>> QueryTableData(DynamicDbContext context, string tableName, string provider)
    {
        var data = new List<Dictionary<string, object>>();

        try
        {
            var sql = provider.ToLower() switch
            {
                "postgresql" => $"SELECT * FROM {tableName} LIMIT 1000",
                "sqlserver" => $"SELECT TOP 1000 * FROM [{tableName}]",
                _ => throw new ArgumentException($"不支援的資料庫提供者: {provider}")
            };

            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            // 使用現有的連線，不要重新打開
            if (context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            {
                await context.Database.GetDbConnection().OpenAsync();
            }

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[columnName] = value;
                }

                data.Add(row);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"查詢資料表 {tableName} 資料失敗: {ex.Message}", ex);
        }

        return data;
    }

    /// <summary>
    /// 格式化資料值
    /// </summary>
    private static string FormatValue(object value)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";

        return value switch
        {
            string str => $"'{str.Replace("'", "''")}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            DateOnly d => $"'{d:yyyy-MM-dd}'",
            TimeOnly t => $"'{t:HH:mm:ss}'",
            bool b => b ? "TRUE" : "FALSE",
            decimal or double or float => value.ToString(),
            int or long or short or byte => value.ToString(),
            Guid guid => $"'{guid}'",
            _ => $"'{value.ToString()?.Replace("'", "''")}'"
        };
    }

    /// <summary>
    /// 轉換資料類型
    /// </summary>
    private static string ConvertDataType(string dataType, string provider)
    {
        if (string.IsNullOrEmpty(dataType))
            return "TEXT";

        var lowerType = dataType.ToLower();

        return provider.ToLower() switch
        {
            "postgresql" => lowerType switch
            {
                "int" or "integer" => "INTEGER",
                "bigint" => "BIGINT",
                "smallint" => "SMALLINT",
                "decimal" or "numeric" => "DECIMAL",
                "real" or "float4" => "REAL",
                "double precision" or "float8" => "DOUBLE PRECISION",
                "boolean" or "bool" => "BOOLEAN",
                "varchar" => "VARCHAR",
                "char" => "CHAR",
                "text" => "TEXT",
                "date" => "DATE",
                "time" => "TIME",
                "timestamp" => "TIMESTAMP",
                "uuid" => "UUID",
                _ => dataType.ToUpper()
            },
            "sqlserver" => lowerType switch
            {
                "int" => "INT",
                "bigint" => "BIGINT",
                "smallint" => "SMALLINT",
                "tinyint" => "TINYINT",
                "decimal" or "numeric" => "DECIMAL",
                "float" => "FLOAT",
                "real" => "REAL",
                "bit" => "BIT",
                "varchar" => "VARCHAR",
                "char" => "CHAR",
                "nvarchar" => "NVARCHAR",
                "nchar" => "NCHAR",
                "text" => "TEXT",
                "ntext" => "NTEXT",
                "date" => "DATE",
                "time" => "TIME",
                "datetime" => "DATETIME",
                "datetime2" => "DATETIME2",
                "uniqueidentifier" => "UNIQUEIDENTIFIER",
                _ => dataType.ToUpper()
            },
            _ => dataType.ToUpper()
        };
    }

    #endregion
}