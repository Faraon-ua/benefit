using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Collections.Generic;

namespace Benefit.Domain.DataAccess
{
    public class DeleteInModel
    {
        public string ColumnName { get; set; }
        public bool IncludeIn { get; set; }
        public List<string> Ids { get; set; }
    }
    public abstract class AdoDbContext
    {
        protected readonly string connectionString;
        public AdoDbContext()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
        protected T ConvertFromDBVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default(T);
            }
            else
            {
                return (T)obj;
            }
        }

        protected void BatchDeleteIn(string tableName, string whereParams, params DeleteInModel[] list)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
            var query = new StringBuilder();
            var whereInQuery = new StringBuilder();
            for (var i = 0; i < list.Length; i++)
            {
                whereInQuery.AppendFormat(" and {0} {1} in (select id from #DeleteList{2})", list[i].ColumnName, list[i].IncludeIn ? "" : "not", i);
                query.AppendFormat("\nCREATE TABLE #DeleteList{0} (id VARCHAR(36))\n ", i);
                var insertCount = 0;
                for (var j = 0; j < list[i].Ids.Count; j++, insertCount++)
                {
                    if (insertCount == 1000)
                    {
                        insertCount = 0;
                    }
                    if (insertCount == 0)
                    {
                        query.AppendFormat("\nINSERT INTO #DeleteList{0} VALUES ", i);
                    }
                    query.AppendFormat("('{1}')", i, list[i].Ids[j]);
                    if (insertCount < 999 && j < list[i].Ids.Count - 1)
                    {
                        query.Append(", ");
                    }
                }
            }
            query.Append("\n");
            query.AppendFormat("delete from {0} where id in (select id FROM {0} where {1}", tableName, whereParams);
            query.Append(whereInQuery);
            query.Append(")");
            for (var i = 0; i < list.Length; i++)
            {
                query.AppendFormat("\nDROP TABLE #DeleteList{0}", i);
            }
            cmd.CommandText = query.ToString();
            cmd.CommandTimeout = 240;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }
    }
}
