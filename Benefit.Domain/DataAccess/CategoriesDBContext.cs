using Benefit.Common.Extensions;
using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Benefit.Domain.DataAccess
{
    public class CategoiesDBContext : AdoDbContext
    {
        private SqlCommand cmd;
        public CategoiesDBContext()
        {
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
        }
        public List<Category> Get(string whereParams, IEnumerable<SqlParameter> sqlParameters)
        {
            List<Category> returnList = new List<Category>();
            cmd.CommandText = string.Format(@"SELECT c.*,
                                                     (SELECT CASE WHEN EXISTS (SELECT Id FROM Categories WHERE ParentCategoryId = c.Id)
                                                                        THEN CAST(1 AS BIT)
                                                                        ELSE CAST(0 AS BIT) END) as HasChildCategories
                                FROM Categories c
                                where {0}", whereParams);
            cmd.Parameters.AddRange(sqlParameters.ToArray());
            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            adapter.Fill(result);

            foreach (DataRow drwRow in result.Tables[0].Rows)
            {
                returnList.Add(drwRow.ToObject<Category>());
            }
            return returnList;
        }
    }
}
