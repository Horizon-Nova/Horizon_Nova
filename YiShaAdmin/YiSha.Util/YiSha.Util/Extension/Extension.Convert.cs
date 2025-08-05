using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Util.Extension
{
    public static partial class Extensions
    {
        #region 转換為long
        /// <summary>
        /// 將object转換為long，若转換失敗，则返回0。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long ParseToLong(this object obj)
        {
            try
            {
                return long.Parse(obj.ToString());
            }
            catch
            {
                return 0L;
            }
        }

        /// <summary>
        /// 將object转換為long，若转換失敗，则返回指定值。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long ParseToLong(this string str, long defaultValue)
        {
            try
            {
                return long.Parse(str);
            }
            catch
            {
                return defaultValue;
            }
        }
        #endregion

        #region 转換為int
        /// <summary>
        /// 將object转換為int，若转換失敗，则返回0。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ParseToInt(this object str)
        {
            try
            {
                return Convert.ToInt32(str);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 將object转換為int，若转換失敗，则返回指定值。不抛出異常。 
        /// null返回默認值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ParseToInt(this object str, int defaultValue)
        {
            if (str == null)
            {
                return defaultValue;
            }
            try
            {
                return Convert.ToInt32(str);
            }
            catch
            {
                return defaultValue;
            }
        }
        #endregion

        #region 转換為short
        /// <summary>
        /// 將object转換為short，若转換失敗，则返回0。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static short ParseToShort(this object obj)
        {
            try
            {
                return short.Parse(obj.ToString());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 將object转換為short，若转換失敗，则返回指定值。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static short ParseToShort(this object str, short defaultValue)
        {
            try
            {
                return short.Parse(str.ToString());
            }
            catch
            {
                return defaultValue;
            }
        }
        #endregion

        #region 转換為demical
        /// <summary>
        /// 將object转換為demical，若转換失敗，则返回指定值。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static decimal ParseToDecimal(this object str, decimal defaultValue)
        {
            try
            {
                return decimal.Parse(str.ToString());
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 將object转換為demical，若转換失敗，则返回0。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static decimal ParseToDecimal(this object str)
        {
            try
            {
                return decimal.Parse(str.ToString());
            }
            catch
            {
                return 0;
            }
        }
        #endregion

        #region 转化為bool
        /// <summary>
        /// 將object转換為bool，若转換失敗，则返回false。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ParseToBool(this object str)
        {
            try
            {
                return bool.Parse(str.ToString());
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 將object转換為bool，若转換失敗，则返回指定值。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ParseToBool(this object str, bool result)
        {
            try
            {
                return bool.Parse(str.ToString());
            }
            catch
            {
                return result;
            }
        }
        #endregion

        #region 转換為float
        /// <summary>
        /// 將object转換為float，若转換失敗，则返回0。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static float ParseToFloat(this object str)
        {
            try
            {
                return float.Parse(str.ToString());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 將object转換為float，若转換失敗，则返回指定值。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static float ParseToFloat(this object str, float result)
        {
            try
            {
                return float.Parse(str.ToString());
            }
            catch
            {
                return result;
            }
        }
        #endregion

        #region 转換為Guid
        /// <summary>
        /// 將string转換為Guid，若转換失敗，则返回Guid.Empty。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Guid ParseToGuid(this string str)
        {
            try
            {
                return new Guid(str);
            }
            catch
            {
                return Guid.Empty;
            }
        }
        #endregion

        #region 转換為DateTime
        /// <summary>
        /// 將string转換為DateTime，若转換失敗，则返回日期最小值。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime ParseToDateTime(this string str)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return DateTime.MinValue;
                }
                if (str.Contains("-") || str.Contains("/"))
                {
                    return DateTime.Parse(str);
                }
                else
                {
                    int length = str.Length;
                    switch (length)
                    {
                        case 4:
                            return DateTime.ParseExact(str, "yyyy", System.Globalization.CultureInfo.CurrentCulture);
                        case 6:
                            return DateTime.ParseExact(str, "yyyyMM", System.Globalization.CultureInfo.CurrentCulture);
                        case 8:
                            return DateTime.ParseExact(str, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        case 10:
                            return DateTime.ParseExact(str, "yyyyMMddHH", System.Globalization.CultureInfo.CurrentCulture);
                        case 12:
                            return DateTime.ParseExact(str, "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture);
                        case 14:
                            return DateTime.ParseExact(str, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                        default:
                            return DateTime.ParseExact(str, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                    }
                }
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// 將string转換為DateTime，若转換失敗，则返回默認值。  
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTime ParseToDateTime(this string str, DateTime? defaultValue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return defaultValue.GetValueOrDefault();
                }
                if (str.Contains("-") || str.Contains("/"))
                {
                    return DateTime.Parse(str);
                }
                else
                {
                    int length = str.Length;
                    switch (length)
                    {
                        case 4:
                            return DateTime.ParseExact(str, "yyyy", System.Globalization.CultureInfo.CurrentCulture);
                        case 6:
                            return DateTime.ParseExact(str, "yyyyMM", System.Globalization.CultureInfo.CurrentCulture);
                        case 8:
                            return DateTime.ParseExact(str, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        case 10:
                            return DateTime.ParseExact(str, "yyyyMMddHH", System.Globalization.CultureInfo.CurrentCulture);
                        case 12:
                            return DateTime.ParseExact(str, "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture);
                        case 14:
                            return DateTime.ParseExact(str, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                        default:
                            return DateTime.ParseExact(str, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                    }
                }
            }
            catch
            {
                return defaultValue.GetValueOrDefault();
            }
        }
        #endregion

        #region 转換為string
        /// <summary>
        /// 將object转換為string，若转換失敗，则返回""。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ParseToString(this object obj)
        {
            try
            {
                if (obj == null)
                {
                    return string.Empty;
                }
                else
                {
                    return obj.ToString();
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        public static string ParseToStrings<T>(this object obj)
        {
            try
            {
                var list = obj as IEnumerable<T>;
                if (list != null)
                {
                    return string.Join(",", list);
                }
                else
                {
                    return obj.ToString();
                }
            }
            catch
            {
                return string.Empty;
            }

        }
        #endregion

        #region 转換為double
        /// <summary>
        /// 將object转換為double，若转換失敗，则返回0。不抛出異常。  
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double ParseToDouble(this object obj)
        {
            try
            {
                return double.Parse(obj.ToString());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 將object转換為double，若转換失敗，则返回指定值。不抛出異常。  
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double ParseToDouble(this object str, double defaultValue)
        {
            try
            {
                return double.Parse(str.ToString());
            }
            catch
            {
                return defaultValue;
            }
        }
        #endregion

        #region 强制转換類型
        /// <summary>
        /// 强制转換類型
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> CastSuper<TResult>(this IEnumerable source)
        {
            foreach (object item in source)
            {
                yield return (TResult)Convert.ChangeType(item, typeof(TResult));
            }
        }
        #endregion
    }
}
