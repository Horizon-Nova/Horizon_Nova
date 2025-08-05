using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YiSha.Data.EF
{
    /// <summary>
    /// 主鍵约定，把属性Id當做資料庫主鍵
    /// </summary>
    public class PrimaryKeyConvention
    {
        public static void SetPrimaryKey(ModelBuilder modelBuilder, string entityName)
        {
            modelBuilder.Entity(entityName).HasKey("Id");
        }
    }

    /// <summary>
    /// 列名约定，比如属性ParentId，映射到資料庫字段parent_id
    /// </summary>
    [Obsolete]
    public class ColumnConvention
    {
        public static void SetColumnName(ModelBuilder modelBuilder, string entityName, string propertyName)
        {
            StringBuilder sbField = new StringBuilder();
            char[] charArr = propertyName.ToCharArray();

            int iCapital = 0; // 把属性第一個開始的大寫字母转成小寫，直到遇到了第1個小寫字母，因為資料庫里面是小寫的
            while (iCapital < charArr.Length)
            {
                if (charArr[iCapital] >= 'A' && charArr[iCapital] <= 'Z')
                {
                    charArr[iCapital] = (char)(charArr[iCapital] + 32);
                }
                else
                {
                    break;
                }
                iCapital++;
            }

            for (int i = 0; i < charArr.Length; i++)
            {
                if (charArr[i] >= 'A' && charArr[i] <= 'Z')
                {
                    charArr[i] = (char)(charArr[i] + 32);
                    sbField.Append("_" + charArr[i]);
                }
                else
                {
                    sbField.Append(charArr[i]);
                }
            }
            modelBuilder.Entity(entityName).Property(propertyName).HasColumnName(sbField.ToString());
        }
    }
}
