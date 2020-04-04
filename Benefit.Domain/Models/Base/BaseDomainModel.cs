using Benefit.Common.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Benefit.Domain.Models.Base
{
    public class BaseDomainModel
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }

        public BaseDomainModel Include<T>(string prop) where T : new()
        {
            var parentType = this.GetType().Name;
            var cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            cmd.CommandText = string.Format(@"SELECT * 
                                              FROM {0}
                                              where {1}Id = '{2}'", prop, parentType, this.Id);
            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            adapter.Fill(result);

            PropertyInfo property = this.GetType().GetProperty(prop);
            var children = result.Tables[0].AsEnumerable().Select(entry =>entry.ToObject<T>()).ToList();
            property.SetValue(this, children, null);
            return this;
        }
    }
}
