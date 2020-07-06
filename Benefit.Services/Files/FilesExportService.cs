using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Benefit.Services.Files
{
    public class FilesExportService
    {
        /// <summary>
        /// Creates the CSV from a generic list.
        /// </summary>;
        /// <typeparam name="T"></typeparam>;
        /// <param name="list">The list.</param>;
        /// <param name="csvNameWithExt">Name of CSV (w/ path) w/ file ext.</param>;
        public byte[] CreateCSVFromGenericList<T>(List<T> list, string delimeter = "$")
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";
            //            if (list == null || list.Count == 0) return new byte[0];

            //get type from 0th member
            var t = typeof(T);
            string newLine = Environment.NewLine;

            var sw = new StringBuilder();
            //make a new instance of the class name we figured out to get its props
            object o = Activator.CreateInstance(t);
            //gets all properties
            var props = o.GetType().GetProperties().Where(p => p.PropertyType == typeof(string) ||
                   !typeof(IEnumerable).IsAssignableFrom(p.PropertyType)).ToList();

            //foreach of the properties in class above, write out properties
            //this is the header row
            var headers = new List<string>();
            foreach(var prop in props)
            {
                var displayAttr = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Single();
                if(displayAttr != null)
                {
                    headers.Add(displayAttr.DisplayName);
                }
                else
                {
                    headers.Add(prop.Name);
                }
            }
            sw.Append(string.Join(delimeter, headers) + newLine);

            //this acts as datarow
            foreach (T item in list)
            {
                //this acts as datacolumn
                var row = string.Join(delimeter, props.Select(d =>
                {
                    var strValue =
                        item.GetType()
                            .GetProperty(d.Name)
                            .GetValue(item, null);
                    var type = item.GetType().GetProperty(d.Name).PropertyType;

                    if (strValue == null)
                        return string.Empty;
                    if(type == typeof(double))
                    {
                        return ((double)strValue).ToString(nfi);
                    }
                    return strValue.ToString();
                }).ToArray());
                sw.Append(row + newLine);
            }
            return Encoding.GetEncoding(1251).GetBytes(sw.ToString());
        }
    }
}
