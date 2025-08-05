using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

namespace HNB.Areas.HNB_WEB.Utilities;

public static partial class Extensions
{
    #region 將列舉成員轉換為 Dictionary 型別
    /// <summary>
    /// 轉換為 Dictionary 型別
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    public static Dictionary<int, string> EnumToDictionary(this Type enumType)
    {
        Dictionary<int, string> dictionary = new Dictionary<int, string>();
        Type typeDescription = typeof(DescriptionAttribute);
        FieldInfo[] fields = enumType.GetFields();
        int sValue = 0;
        string sText = string.Empty;
        foreach (FieldInfo field in fields)
        {
            if (field.FieldType.IsEnum)
            {
                sValue = ((int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null));
                object[] arr = field.GetCustomAttributes(typeDescription, true);
                if (arr.Length > 0)
                {
                    DescriptionAttribute da = (DescriptionAttribute)arr[0];
                    sText = da.Description;
                }
                else
                {
                    sText = field.Name;
                }
                dictionary.Add(sValue, sText);
            }
        }
        return dictionary;
    }

    /// <summary>
    /// 將列舉成員轉換為鍵值對的 JSON 字串
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    public static string EnumToDictionaryString(this Type enumType)
    {
        List<KeyValuePair<int, string>> dictionaryList = EnumToDictionary(enumType).ToList();
        var sJson = JsonConvert.SerializeObject(dictionaryList);
        return sJson;
    }
    #endregion

    #region 取得列舉值對應的描述
    /// <summary>
    /// 取得列舉值的描述
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    public static string GetDescription(this System.Enum enumType)
    {
        FieldInfo EnumInfo = enumType.GetType().GetField(enumType.ToString());
        if (EnumInfo != null)
        {
            DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])EnumInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (EnumAttributes.Length > 0)
            {
                return EnumAttributes[0].Description;
            }
        }
        return enumType.ToString();
    }
    #endregion

    #region 根據值取得列舉的描述
    /// <summary>
    /// 根據數值取得列舉描述
    /// </summary>
    public static string GetDescriptionByEnum<T>(this object obj)
    {
        var tEnum = System.Enum.Parse(typeof(T), obj.ParseToString()) as System.Enum;
        var description = tEnum.GetDescription();
        return description;
    }
    #endregion
}
