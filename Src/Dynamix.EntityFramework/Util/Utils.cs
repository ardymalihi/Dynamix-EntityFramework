using Dynamix.EntityFramework.Model;
using System;

namespace Dynamix.EntityFramework.Util
{
    public static class Utils
    {
        public static string GetFancyLabel(string aLabel)
        {
            string ret = aLabel;
            System.Globalization.TextInfo ti = new System.Globalization.CultureInfo("en-US", false).TextInfo;
            if (ret.IndexOf(" ") >= 0)
            {
                ret = ti.ToTitleCase(ret);
            }
            else
            {
                ret = ret.Replace("_", " ");
                string finalString = "";
                for (int i = 0; i <= ret.Length - 1; i++)
                {
                    if (Char.IsUpper(ret[i]))
                    {
                        finalString = finalString + " " + ret[i];
                    }
                    else
                    {
                        finalString = finalString + ret[i];
                    }
                }
                finalString = finalString.Trim();
                ret = "";
                for (int i = 0; i <= finalString.Length - 1; i++)
                {
                    if (finalString[i] == ' ')
                    {
                        if (i == finalString.Length - 2)
                        {
                            if (Char.IsUpper(finalString[i - 1]) && Char.IsUpper(finalString[i + 1]))
                            {
                                ret = ret + "_";
                            }
                            else
                            {
                                ret = ret + finalString[i];
                            }
                        }
                        else
                        {
                            if (i < finalString.Length - 2)
                            {
                                if (Char.IsUpper(finalString[i - 1]) && Char.IsUpper(finalString[i + 1]) && (finalString[i + 2] == ' '))
                                {
                                    ret = ret + "_";
                                }
                                else
                                {
                                    ret = ret + finalString[i];
                                }
                            }
                            else
                            {
                                ret = ret + finalString[i];
                            }
                        }
                    }
                    else
                    {
                        ret = ret + finalString[i];
                    }
                }
                finalString = ret;
                finalString = finalString.Replace("_", "");
                ret = finalString;
                ret = ti.ToTitleCase(ret);
            }
            return ret;
        }

        public static DataType DBTypeToDataType(string dbType, bool nullable)
        {
            string dataType = dbType.ToLower();
            DataType ret = new DataType();
            ret.DbType = dbType;

            try
            {
                if (nullable)
                {
                    ret.DbType = ret.DbType + "(nullable)";
                }

                if (dataType == "uniqueidentifier")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<Guid>);
                    }
                    else
                    {
                        ret.SystemType = typeof(Guid);
                    }
                }
                else if (dataType == "xml")
                {
                    ret.SystemType = typeof(string);
                }
                else if (dataType == "nvarchar")
                {
                    ret.SystemType = typeof(string);
                }
                else if (dataType == "char")
                {
                    ret.SystemType = typeof(char);
                }
                else if (dataType == "nchar")
                {
                    ret.SystemType = typeof(char);
                }
                else if (dataType == "text")
                {
                    ret.SystemType = typeof(string);
                }
                else if (dataType == "ntext")
                {
                    ret.SystemType = typeof(string);
                }
                else if (dataType == "varchar")
                {
                    ret.SystemType = typeof(string);
                }
                else if (dataType == "money" || dataType == "smallmoney")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<decimal>);
                    }
                    else
                    {
                        ret.SystemType = typeof(decimal);
                    }
                }
                else if (dataType == "numeric")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<decimal>);
                    }
                    else
                    {
                        ret.SystemType = typeof(decimal);
                    }
                }
                else if (dataType == "smallint")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<Int16>);
                    }
                    else
                    {
                        ret.SystemType = typeof(Int16);
                    }
                }
                else if (dataType == "int")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<int>);
                    }
                    else
                    {
                        ret.SystemType = typeof(int);
                    }
                }
                else if (dataType == "bigint")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<Int64>);
                    }
                    else
                    {
                        ret.SystemType = typeof(Int64);
                    }
                }
                else if (dataType == "tinyint")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<byte>);
                    }
                    else
                    {
                        ret.SystemType = typeof(byte);
                    }
                }
                else if (dataType == "float")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<double>);
                    }
                    else
                    {
                        ret.SystemType = typeof(double);
                    }
                }
                else if (dataType == "real")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<Single>);
                    }
                    else
                    {
                        ret.SystemType = typeof(Single);
                    }
                }
                else if (dataType == "decimal")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<decimal>);
                    }
                    else
                    {
                        ret.SystemType = typeof(decimal);
                    }
                }
                else if (dataType == "date")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<DateTime>);
                    }
                    else
                    {
                        ret.SystemType = typeof(DateTime);
                    }
                }
                else if (dataType.ToLower() == "timestamp")
                {
                    ret.SystemType = typeof(byte[]);
                }
                else if (dataType.ToLower() == "time")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<TimeSpan>);
                    }
                    else
                    {
                        ret.SystemType = typeof(TimeSpan);
                    }
                }
                else if (dataType == "smalldatetime")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<DateTime>);
                    }
                    else
                    {
                        ret.SystemType = typeof(DateTime);
                    }
                }
                else if (dataType == "datetime")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<DateTime>);
                    }
                    else
                    {
                        ret.SystemType = typeof(DateTime);
                    }
                }
                else if (dataType == "image")
                {
                    ret.SystemType = typeof(byte[]);
                }
                else if (dataType == "varbinary")
                {
                    ret.SystemType = typeof(byte[]);
                }
                else if (dataType == "binary")
                {
                    ret.SystemType = typeof(byte[]);
                }
                else if (dataType == "bit")
                {
                    if (nullable)
                    {
                        ret.SystemType = typeof(Nullable<bool>);
                    }
                    else
                    {
                        ret.SystemType = typeof(bool);
                    }
                }
                else
                {
                    throw new Exception("Invalid Sql Data Type");
                }
            }
            catch
            {
                ret.SystemType = typeof(object);
            }

            return ret;
        }

        public static string FormatTextToVariableType(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            char[] illegalChars = { '\'', ' ', '?', '-', '+', '*', '/', '\\', '!', '@', '#', '$', '%', '^', '(', ')', '[', ']', '{', '}', ',', ';', ':', '.', '`', '~', '&', '=', '|', '<', '>' };

            while (text.IndexOfAny(illegalChars) >= 0)
            {
                for (int i = 0; i <= illegalChars.Length - 1; i++)
                {
                    text = text.Replace(Convert.ToString(illegalChars[i]), "");
                }
            }

            return text;
        }

        public static object GetPropertyValue(object src, string propName)
        {
            if (src == null) return null;
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static void SetPropertyValue(object src, string propName, object value)
        {
            if (src == null) return;
            var property = src.GetType().GetProperty(propName);
            if (property != null)
            {
                Type t = Nullable.GetUnderlyingType(property.PropertyType)
                 ?? property.PropertyType;

                object safeValue = (value == null) ? null : Convert.ChangeType(value, t);

                property.SetValue(src, safeValue, null);
            }
        }

        public static Type GetPropertyType(object src, string propName)
        {
            return src.GetType().GetProperty(propName).PropertyType;
        }

        public static Type GetListType(dynamic list)
        {
            return list.GetType().GetGenericArguments()[0];
        }
    }
}
