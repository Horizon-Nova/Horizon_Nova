
using HNB.Areas.HnbBackoffice.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace HNB.Areas.HNBBackoffice.Utilities;

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
    /// 從資料庫生成 Entity Framework 模型
    /// </summary>
    /// <param name="provider">資料庫提供者</param>
    /// <param name="connectionString">連線字串</param>
    /// <param name="contextName">DbContext 名稱</param>
    /// <param name="outputDirectory">輸出目錄</param>
    /// <returns>生成結果</returns>
    public static async Task<(bool Success, string Message)> GenerateModelsFromDatabase(GenerateModelsRequestDto request)
    {
        try
        {
            var fullOutputPath = Path.Combine(Directory.GetCurrentDirectory(), request.OutputDirectory);
            Directory.CreateDirectory(fullOutputPath);

            using var context = new DynamicDbContext(request.Provider, request.ConnectionString);
            var tables = await context.QueryTableNamesAsync();

            if (!tables.Any())
            {
                return (false, "資料庫中沒有找到任何資料表");
            }

            var services = new ServiceCollection();
            services.AddEntityFrameworkDesignTimeServices();

            if (request.Provider.ToLower() == "postgresql")
            {
                new Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal.NpgsqlDesignTimeServices().ConfigureDesignTimeServices(services);
            }
            else if (request.Provider.ToLower() == "sqlserver")
            {
                new Microsoft.EntityFrameworkCore.SqlServer.Design.Internal.SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);
            }

            using var provider = services.BuildServiceProvider();
            var scaffolder = provider.GetRequiredService<IReverseEngineerScaffolder>();

            var model = scaffolder.ScaffoldModel(
                connectionString: request.ConnectionString,
                databaseOptions: new DatabaseModelFactoryOptions(tables.ToArray(), Array.Empty<string>()),
                modelOptions: new ModelReverseEngineerOptions { UseDatabaseNames = true },
                codeOptions: new ModelCodeGenerationOptions
                {
                    ContextName = request.ContextName,
                    ContextDir = fullOutputPath,
                    ModelNamespace = $"Models.{request.ContextName.Replace("DbContext", "")}",
                    ContextNamespace = $"Models.{request.ContextName.Replace("DbContext", "")}",
                    SuppressOnConfiguring = true,
                    SuppressConnectionStringWarning = true,
                    UseDataAnnotations = true,
                    UseNullableReferenceTypes = true
                });

            scaffolder.Save(model, fullOutputPath, overwriteFiles: true);

            return (true, $"模型生成成功！\n找到 {tables.Count} 個資料表: {string.Join(", ", tables)}\n輸出目錄: {request.OutputDirectory}\nContext: {request.ContextName}");
        }
        catch (Exception ex)
        {
            return (false, $"模型生成過程中發生錯誤: {ex.Message}");
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

    #endregion
}