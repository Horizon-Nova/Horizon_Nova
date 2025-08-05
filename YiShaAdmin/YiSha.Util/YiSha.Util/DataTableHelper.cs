using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Util
{
    public static class DataTableHelper
    {
        public static DataTable ListToDataTable<T>(List<T> entitys)
        {
            //检查實體集合不能為空
            if (entitys == null || entitys.Count < 1)
            {
                throw new Exception("需转換的集合為空");
            }
            //取出第一個實體的所有Propertie
            Type entityType = entitys[0].GetType();
            PropertyInfo[] entityProperties = entityType.GetProperties();

            //生成DataTable的structure
            //生產代碼中，應將生成的DataTable結構Cache起來，此處略
            DataTable dt = new DataTable();
            for (int i = 0; i < entityProperties.Length; i++)
            {
                dt.Columns.Add(entityProperties[i].Name);
            }
            //將所有entity添加到DataTable中
            foreach (object entity in entitys)
            {
                //检查所有的的實體都為同一類型
                if (entity.GetType() != entityType)
                {
                    throw new Exception("要转換的集合元素類型不一致");
                }
                object[] entityValues = new object[entityProperties.Length];
                for (int i = 0; i < entityProperties.Length; i++)
                {
                    entityValues[i] = entityProperties[i].GetValue(entity, null);
                }
                dt.Rows.Add(entityValues);
            }
            return dt;
        }
    }
}
