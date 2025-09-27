using Microsoft.EntityFrameworkCore;
using System.Reflection;
using HNB.Areas.HnbBackoffice.Dtos;

namespace HNB.Areas.HnbBackoffice.Utilities
{
    /// <summary>
    /// 動態資料庫上下文，用於連接任意資料庫
    /// </summary>
    public class DynamicDbContext : DbContext
    {
        private readonly string _provider;
        private readonly string _connectionString;

        public DynamicDbContext(string provider, string connectionString)
        {
            _provider = provider.ToLower();
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            switch (_provider)
            {
                case "postgresql":
                    optionsBuilder.UseNpgsql(_connectionString);
                    break;
                case "sqlserver":
                    optionsBuilder.UseSqlServer(_connectionString);
                    break;
                default:
                    throw new ArgumentException($"不支援的資料庫提供者: {_provider}");
            }
        }

        /// <summary>
        /// 動態獲取資料表清單 - 使用 EF Core API
        /// </summary>
        public async Task<List<string>> QueryTableNamesAsync()
        {
            try
            {
                var connection = Database.GetDbConnection();
                await connection.OpenAsync();
                
                var tableNames = new List<string>();
                
                if (_provider == "postgresql")
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT schemaname || '.' || tablename as table_name
                        FROM pg_tables 
                        WHERE schemaname NOT IN (@schema1, @schema2)
                        AND tablename NOT LIKE @prefix1
                        AND tablename NOT LIKE @prefix2
                        ORDER BY schemaname, tablename";
                    
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("@schema1", "information_schema"));
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("@schema2", "pg_catalog"));
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("@prefix1", "pg_%"));
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("@prefix2", "sql_%"));
                    
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        tableNames.Add(reader.GetString(0));
                    }
                }
                else if (_provider == "sqlserver")
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT TABLE_NAME 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_TYPE = @tableType
                        AND TABLE_NAME NOT LIKE @prefix1
                        AND TABLE_NAME NOT LIKE @prefix2
                        ORDER BY TABLE_NAME";
                    
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@tableType", "BASE TABLE"));
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@prefix1", "sys%"));
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@prefix2", "dt%"));
                    
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        tableNames.Add(reader.GetString(0));
                    }
                }
                else
                {
                    throw new NotSupportedException($"不支援的資料庫提供者: {_provider}");
                }
                
                return tableNames;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"獲取資料表清單失敗: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 動態獲取資料表欄位資訊
        /// </summary>
        public async Task<List<TableColumnDto>> QueryTableColumnsAsync(string tableName)
        {
            try
            {
                var connection = Database.GetDbConnection();
                await connection.OpenAsync();
                
                var columns = new List<TableColumnDto>();
                
                if (_provider == "postgresql")
                {
                    string schemaName = "public";
                    string actualTableName = tableName;
                    
                    if (tableName.Contains('.'))
                    {
                        var parts = tableName.Split('.');
                        schemaName = parts[0];
                        actualTableName = parts[1];
                    }
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT 
                            c.column_name,
                            c.data_type,
                            c.character_maximum_length,
                            c.is_nullable,
                            c.column_default,
                            CASE WHEN pk.column_name IS NOT NULL THEN 'YES' ELSE 'NO' END as is_primary_key
                        FROM information_schema.columns c
                        LEFT JOIN (
                            SELECT ku.column_name
                            FROM information_schema.table_constraints tc
                            JOIN information_schema.key_column_usage ku 
                                ON tc.constraint_name = ku.constraint_name
                            WHERE tc.table_schema = @schemaName
                            AND tc.table_name = @tableName 
                            AND tc.constraint_type = 'PRIMARY KEY'
                        ) pk ON c.column_name = pk.column_name
                        WHERE c.table_schema = @schemaName
                        AND c.table_name = @tableName
                        ORDER BY c.ordinal_position";
                    
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("@schemaName", schemaName));
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("@tableName", actualTableName));
                    
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var column = new TableColumnDto
                        {
                            ColumnName = reader.GetString(0),
                            DataType = reader.GetString(1),
                            Length = reader.IsDBNull(2) ? null : reader.GetInt32(2).ToString(),
                            IsNullable = reader.GetString(3) == "YES",
                            DefaultValue = reader.IsDBNull(4) ? null : reader.GetString(4),
                            IsPrimaryKey = reader.GetString(5) == "YES",
                            Constraints = reader.GetString(5) == "YES" ? "Primary Key" : ""
                        };
                        columns.Add(column);
                    }
                }
                else if (_provider == "sqlserver")
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT 
                            c.COLUMN_NAME,
                            c.DATA_TYPE,
                            c.CHARACTER_MAXIMUM_LENGTH,
                            c.IS_NULLABLE,
                            c.COLUMN_DEFAULT,
                            CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 'YES' ELSE 'NO' END as IS_PRIMARY_KEY
                        FROM INFORMATION_SCHEMA.COLUMNS c
                        LEFT JOIN (
                            SELECT ku.COLUMN_NAME
                            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku 
                                ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                            WHERE tc.TABLE_NAME = @tableName 
                            AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                        ) pk ON c.COLUMN_NAME = pk.COLUMN_NAME
                        WHERE c.TABLE_NAME = @tableName
                        ORDER BY c.ORDINAL_POSITION";
                    
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@tableName", tableName));
                    
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var column = new TableColumnDto
                        {
                            ColumnName = reader.GetString(0),
                            DataType = reader.GetString(1),
                            Length = reader.IsDBNull(2) ? null : reader.GetInt32(2).ToString(),
                            IsNullable = reader.GetString(3) == "YES",
                            DefaultValue = reader.IsDBNull(4) ? null : reader.GetString(4),
                            IsPrimaryKey = reader.GetString(5) == "YES",
                            Constraints = reader.GetString(5) == "YES" ? "Primary Key" : ""
                        };
                        columns.Add(column);
                    }
                }
                else
                {
                    throw new NotSupportedException($"不支援的資料庫提供者: {_provider}");
                }
                
                return columns;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"獲取資料表 {tableName} 欄位資訊失敗: {ex.Message}", ex);
            }
        }
        
    }
}
