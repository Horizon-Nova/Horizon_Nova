using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Oracle.ManagedDataAccess.Client;

namespace YiSha.Data
{
    public class DbParameterExtension
    {
        /// <summary>
        /// 根据配置文件中所配置的資料庫類型
        /// 來創建相應資料庫的參數對象
        /// </summary>
        /// <returns></returns>
        public static DbParameter CreateDbParameter()
        {
            switch (DbHelper.DbType)
            {
                case DatabaseType.SqlServer:
                    return new SqlParameter();

                case DatabaseType.MySql:
                    return new MySqlParameter();

                case DatabaseType.Oracle:
                    return new OracleParameter();

                default:
                    throw new Exception("資料庫類型目前不支持！");
            }
        }

        /// <summary>
        /// 根据配置文件中所配置的資料庫類型
        /// 來創建相應資料庫的參數對象
        /// </summary>
        /// <returns></returns>
        public static DbParameter CreateDbParameter(string paramName, object value)
        {
            DbParameter param = CreateDbParameter();
            param.ParameterName = paramName;
            param.Value = value;
            return param;
        }

        /// <summary>
        /// 转換對應的資料庫參數
        /// </summary>
        /// <param name="dbParameter">參數</param>
        /// <returns></returns>
        public static DbParameter[] ToDbParameter(DbParameter[] dbParameter)
        {
            int i = 0;
            int size = dbParameter.Length;
            DbParameter[] _dbParameter = null;
            switch (DbHelper.DbType)
            {
                case DatabaseType.SqlServer:
                    _dbParameter = new SqlParameter[size];
                    while (i < size)
                    {
                        _dbParameter[i] = new SqlParameter(dbParameter[i].ParameterName, dbParameter[i].Value);
                        i++;
                    }
                    break;
                case DatabaseType.MySql:
                    _dbParameter = new MySqlParameter[size];
                    while (i < size)
                    {
                        _dbParameter[i] = new MySqlParameter(dbParameter[i].ParameterName, dbParameter[i].Value);
                        i++;
                    }
                    break;
                case DatabaseType.Oracle:
                    _dbParameter = new OracleParameter[size];
                    while (i < size)
                    {
                        _dbParameter[i] = new OracleParameter(dbParameter[i].ParameterName, dbParameter[i].Value);
                        i++;
                    }
                    break;
                default:
                    throw new Exception("資料庫類型目前不支持！");
            }
            return _dbParameter;
        }
    }
}
