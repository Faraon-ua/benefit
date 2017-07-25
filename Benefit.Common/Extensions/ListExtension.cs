using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Benefit.Common.Extensions
{
    public static class ListExtension
    {
        public static DataTable ToDataTable<T>(this IList<T> list)
        {
            var props = typeof(T).GetProperties().Where(p => p.PropertyType.IsSimpleType()).ToList();
            var table = new DataTable();
            foreach (var prop in props)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            var values = new object[props.Count];
            foreach (T item in list)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = props[i].GetValue(item, null) ?? DBNull.Value;
                table.Rows.Add(values);
            }
            return table;
        }
    }
}
