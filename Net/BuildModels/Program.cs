using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

namespace BuildModels;

internal class Program
{
    static void Main()
    {
        #region Audit
        var auditTag = BuildModels.Internal.InternalAuditVerifier.GetInternalHash();
        Console.WriteLine(auditTag);
        Console.WriteLine("[BuildModels] 開始從 PostgreSQL 產生 EF Core 模型 (Database-First)");
        #endregion

        #region 讀設定
        var cfg = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        #endregion

        #region 解析連線與資料表
        var rawTablesMap = cfg.GetSection("ScaffoldSettings:Tables")
                              .Get<Dictionary<string, string[]>>() ?? new();
        var connMap = cfg.GetSection("ConnectionStrings")
                         .GetChildren()
                         .ToDictionary(c => c.Key.Trim(), c => c.Value ?? string.Empty,
                                       StringComparer.OrdinalIgnoreCase);
        if (rawTablesMap.Count == 0 || connMap.Count == 0)
        {
            Console.WriteLine("缺少 Tables 或 ConnectionStrings，流程結束。");
            return;
        }
        var tablesMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in rawTablesMap)
        {
            var key = (kv.Key ?? "").Trim();
            var list = (kv.Value ?? Array.Empty<string>())
                        .Select(s => (s ?? "").Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.Ordinal)
                        .ToArray();
            if (list.Length > 0) tablesMap[key] = list;
        }
        #endregion

        #region 專案根與輸出根解析
        static string FindProjectRootFromBin(string start, string csproj)
        {
            for (var d = new DirectoryInfo(start); d != null; d = d.Parent)
                if (File.Exists(Path.Combine(d.FullName, csproj))) return d.FullName;
            throw new DirectoryNotFoundException($"找不到 {csproj}");
        }

        var exeName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "BuildModels";
        var buildModelsRoot = FindProjectRootFromBin(AppContext.BaseDirectory, $"{exeName}.csproj");

        var baseDir = Directory.GetParent(buildModelsRoot)?.FullName ?? buildModelsRoot;

        var outputRootSetting = cfg["ScaffoldSettings:ModelOutputRoot"];
        var outputRoot = string.IsNullOrWhiteSpace(outputRootSetting)
            ? Path.Combine(baseDir, "Models")
            : (Path.IsPathRooted(outputRootSetting)
                ? outputRootSetting
                : Path.GetFullPath(Path.Combine(baseDir, outputRootSetting)));
        #endregion

        #region DI & Scaffolder
        var services = new ServiceCollection();
        services.AddEntityFrameworkDesignTimeServices();
        new NpgsqlDesignTimeServices().ConfigureDesignTimeServices(services);
        using var provider = services.BuildServiceProvider();
        var scaffolder = provider.GetRequiredService<IReverseEngineerScaffolder>();
        #endregion

        #region Scaffold
        foreach (var kv in tablesMap)
        {
            var key = kv.Key;
            var tables = kv.Value;

            if (!connMap.TryGetValue(key, out var conn) || string.IsNullOrWhiteSpace(conn))
            {
                Console.WriteLine($"[Skip] {key} 無對應連線字串");
                continue;
            }

            var outDir = Path.Combine(outputRoot, key);
            Directory.CreateDirectory(outDir);

            Console.WriteLine($"=> 產生 {key}（{tables.Length} 張表）");
            Console.WriteLine("   表清單: " + string.Join(", ", tables));

            var model = scaffolder.ScaffoldModel(
                connectionString: conn,
                databaseOptions: new DatabaseModelFactoryOptions(tables, Array.Empty<string>()),
                modelOptions: new ModelReverseEngineerOptions { UseDatabaseNames = true },
                codeOptions: new ModelCodeGenerationOptions
                {
                    ContextName = $"{key}DbContext",
                    ContextDir = outDir,
                    ModelNamespace = $"Models.{key}",
                    ContextNamespace = $"Models.{key}",
                    SuppressOnConfiguring = true,
                    SuppressConnectionStringWarning = true,
                    UseDataAnnotations = true,
                    UseNullableReferenceTypes = true
                });

            scaffolder.Save(model, outDir, overwriteFiles: true);

            // 為生成的檔案添加警告註解
            AddWarningCommentsToGeneratedFiles(outDir);
        }
        #endregion

        Console.WriteLine("[BuildModels] 完成");
    }

    /// <summary>
    /// 為指定目錄下的所有 .cs 檔案添加警告註解
    /// </summary>
    private static void AddWarningCommentsToGeneratedFiles(string directory)
    {
        var warningComment = @"// ╔════════════════════════════════════════════════════════════════════════╗
// ║                     嚴重警告 / CRITICAL WARNING                        ║
// ╠════════════════════════════════════════════════════════════════════════╣
// ║  此檔案為系統自動生成，禁止手動修改！                                    ║
// ║  This file is AUTO-GENERATED by the system. DO NOT MODIFY manually!    ║
// ║                                                                          ║
// ║  任何手動修改將在下次重新生成時被覆蓋且遺失！                            ║
// ║  Any manual changes will be OVERWRITTEN and LOST on next generation!   ║
// ║                                                                          ║
// ║  如需修改，請調整資料庫結構後重新執行 BuildModels 工具                   ║
// ║  To make changes, modify the database schema and re-run BuildModels     ║
// ╚════════════════════════════════════════════════════════════════════════╝
";

        foreach (var filePath in Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var content = File.ReadAllText(filePath);
                
                // 檢查是否已經有警告註解（避免重複添加）
                if (content.Contains("此模型為系統自動導出") || content.Contains("auto-generated"))
                {
                    continue;
                }

                // 在檔案開頭插入警告註解
                var newContent = warningComment + content;
                File.WriteAllText(filePath, newContent);
                }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ 無法處理檔案 {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }
    }
}
