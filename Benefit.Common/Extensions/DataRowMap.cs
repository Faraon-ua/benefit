using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Benefit.Common.Extensions
{
    public static class DataRowMap
    {
        public static T ToObject<T>(this DataRow dataRow) where T : new()
        {
            T item = new T();

            foreach (DataColumn column in dataRow.Table.Columns)
            {
                PropertyInfo property = GetProperty(typeof(T), column.ColumnName);

                if (property != null)
                {
                    if (dataRow[column] == null || dataRow[column] == DBNull.Value)
                    {
                        property.SetValue(item, null, null);
                    }
                    else
                    {
                        try
                        {
                            property.SetValue(item, dataRow[column], null);
                        }
                        catch(Exception ex)
                        {
                            var enumType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                            if (enumType.IsEnum)
                            {
                                var convertedValue = Enum.ToObject(enumType, dataRow[column]);
                                property.SetValue(item, convertedValue, null);
                            }
                        }
                    }
                }
            }

            return item;
        }

        private static PropertyInfo GetProperty(Type type, string attributeName)
        {
            PropertyInfo property = type.GetProperty(attributeName);

            if (property != null)
            {
                return property;
            }

            return type.GetProperties()
                 .Where(p => p.IsDefined(typeof(DisplayAttribute), false) && p.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().Single().Name == attributeName)
                 .FirstOrDefault();
        }

        public static object ChangeType(object value, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                return Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
            }

            return Convert.ChangeType(value, type);
        }
    }
}
