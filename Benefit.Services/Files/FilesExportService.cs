using System;
using System.Collections;
using System.Collections.Generic;
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
        public byte[] CreateCSVFromGenericList<T>(List<T> list)
        {
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
            sw.Append(string.Join("$", props.Select(d => d.Name).ToArray()) + newLine);

            //this acts as datarow
            foreach (T item in list)
            {
                //this acts as datacolumn
                var row = string.Join("$", props.Select(d =>
                {
                    var strValue =
                        item.GetType()
                            .GetProperty(d.Name)
                            .GetValue(item, null);
                    return strValue == null ? string.Empty : strValue.ToString();
                }).ToArray());
                sw.Append(row + newLine);

            }
            return Encoding.UTF8.GetBytes(sw.ToString());
        }
    }
}
