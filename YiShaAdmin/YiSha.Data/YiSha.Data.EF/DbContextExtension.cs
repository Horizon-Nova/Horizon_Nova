using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using YiSha.Util;

namespace YiSha.Data.EF
{
    public static class DbContextExtension
    {
        /// <summary>
        /// 拼接删除SQL語句
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public static string DeleteSql(string tableName)
        {
            StringBuilder strSql = new StringBuilder("DELETE FROM " + tableName + "");
            return strSql.ToString();
        }

        /// <summary>
        /// 拼接删除SQL語句
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="propertyName">實體属性名稱</param>
        /// <param name="propertyValue">字段值：數組1,2,3,4,5,6.....</param>
        /// <returns></returns>
        public static string DeleteSql(string tableName, string propertyName, long propertyValue)
        {
            StringBuilder strSql = new StringBuilder("DELETE FROM " + tableName + " WHERE " + propertyName + " = " + propertyValue + "");
            return strSql.ToString();
        }

        /// <summary>
        /// 拼接批量删除SQL語句
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="propertyName">實體属性名稱</param>
        /// <param name="propertyValue">字段值：數組1,2,3,4,5,6.....</param>
        /// <returns></returns>
        public static string DeleteSql(string tableName, string propertyName, long[] propertyValue)
        {
            string strSql = "DELETE FROM " + tableName + " WHERE " + propertyName + " IN (" + string.Join(",", propertyValue) + ")";
            return strSql.ToString();
        }

        /// <summary>
        /// 獲取實體映射對象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbcontext"></param>
        /// <returns></returns>
        public static IEntityType GetEntityType<T>(DbContext dbcontext) where T : class
        {
            return dbcontext.Model.FindEntityType(typeof(T));
        }

        /// <summary>
        /// 存储過程語句
        /// </summary>
        /// <param name="procName">存储過程名稱</param>
        /// <param name="dbParameter">執行命令所需的sql語句對應參數</param>
        /// <returns></returns>
        public static string BuilderProc(string procName, params DbParameter[] dbParameter)
        {
            StringBuilder strSql = new StringBuilder("exec " + procName);
            if (dbParameter != null)
            {
                foreach (var item in dbParameter)
                {
                    strSql.Append(" " + item + ",");
                }
                strSql = strSql.Remove(strSql.Length - 1, 1);
            }
            return strSql.ToString();
        }

        public static void SetEntityDefaultValue(DbContext dbcontext)
        {
            foreach (EntityEntry entry in dbcontext.ChangeTracker.Entries().Where(p => p.State == EntityState.Added))
            {
                #region 把null設置成對應属性類型的默認值
                Type type = entry.Entity.GetType();
                PropertyInfo[] props = ReflectionHelper.GetProperties(type);
                foreach (PropertyInfo prop in props)
                {
                    object value = prop.GetValue(entry.Entity, null);
                    if (value == null)
                    {
                        string sType = string.Empty;
                        if (prop.PropertyType.GenericTypeArguments.Length > 0)
                        {
                            sType = prop.PropertyType.GenericTypeArguments[0].Name;
                        }
                        else
                        {
                            sType = prop.PropertyType.Name;
                        }
                        switch (sType)
                        {
                            case "Boolean":
                                prop.SetValue(entry.Entity, false);
                                break;
                            case "Char":
                                prop.SetValue(entry.Entity, 0);
                                break;
                            case "SByte":
                                prop.SetValue(entry.Entity, 0);
                                break;
                            case "Byte":
                                prop.SetValue(entry.Entity, 0);
                                break;
                            case "Int16":
                                prop.SetValue(entry.Entity, 0);
                                break;
                            case "UInt16":
                                prop.SetValue(entry.Entity, 0);
                                break;
                            case "Int32":
                                prop.SetValue(entry.Entity, 0);
                                break;
                            case "UInt32":
                                prop.SetValue(entry.Entity, 0);
                                break;
                            case "Int64":
                                prop.SetValue(entry.Entity, (Int64)0);
                                break;
                            case "UInt64":
                                prop.SetValue(entry.Entity, 0);
                                break;
                            case "Single":
                                prop.SetValue(entry.Entity, 0);
                                break;
                            case "Double":
                                prop.SetValue(entry.Entity, 0);
                                break;
                            case "Decimal":
                                prop.SetValue(entry.Entity, (decimal)0);
                                break;
                            case "DateTime":
                                prop.SetValue(entry.Entity, GlobalConstant.DefaultTime);
                                break;
                            case "String":
                                prop.SetValue(entry.Entity, string.Empty);
                                break;
                            default: break;
                        }
                    }
                    else if (value.ToString() == DateTime.MinValue.ToString())
                    {
                        // sql server datetime類型的的範圍不到0001-01-01，所以转成1970-01-01
                        prop.SetValue(entry.Entity, GlobalConstant.DefaultTime);
                    }
                }
                #endregion
            }
        }
    }
}
