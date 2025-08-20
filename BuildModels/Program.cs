// -----------------------------------------------------------------------------
// BuildModels – EF Core Database-First（PostgreSQL 版）
// -----------------------------------------------------------------------------
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
        var auditTag = BuildModels.Internal.InternalAuditVerifier.GetInternalHash();
        Console.WriteLine(auditTag);

        Console.WriteLine("[BuildModels] 開始從 PostgreSQL 產生 EF Core 模型 (Database-First)");

        var cfg = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

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
        foreach (KeyValuePair<string, string[]> kv in rawTablesMap)
        {
            var key = (kv.Key ?? "").Trim();
            var list = (kv.Value ?? Array.Empty<string>())
                        .Select(s => (s ?? "").Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.Ordinal)
                        .ToArray();
            if (list.Length > 0) tablesMap[key] = list;
        }

        var services = new ServiceCollection();
        services.AddEntityFrameworkDesignTimeServices();
        new NpgsqlDesignTimeServices().ConfigureDesignTimeServices(services);

        using var provider = services.BuildServiceProvider();
        var scaffolder = provider.GetRequiredService<IReverseEngineerScaffolder>();

        foreach (KeyValuePair<string, string[]> kv in tablesMap)
        {
            var key = kv.Key;
            var tables = kv.Value;

            if (!connMap.TryGetValue(key, out var conn) || string.IsNullOrWhiteSpace(conn))
            {
                Console.WriteLine($"[Skip] {key} 無對應連線字串");
                continue;
            }

            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "HNB"));
            var outDir = Path.Combine(projectRoot, "Models", key);
            Directory.CreateDirectory(outDir);

            Console.WriteLine($"=> 產生 {key}（{tables.Length} 張表）");
            Console.WriteLine("   表清單: " + string.Join(", ", tables));

            var model = scaffolder.ScaffoldModel(
                connectionString: conn,
                databaseOptions: new DatabaseModelFactoryOptions(
                    tables,
                    Array.Empty<string>()
                ),
                modelOptions: new ModelReverseEngineerOptions
                {
                    UseDatabaseNames = true
                },
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
        }

        Console.WriteLine("[BuildModels] 完成");
    }
}
