using Benefit.Common.Extensions;
using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

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
        public Category Get(string urlName)
        {
            var sql = new StringBuilder(@"SELECT c.*, pc.UrlName as ParentUrl, pc.Name as ParentName,
                                                     (SELECT CASE WHEN EXISTS (SELECT Id FROM Categories WHERE ParentCategoryId = c.Id)
                                                                        THEN CAST(1 AS BIT)
                                                                        ELSE CAST(0 AS BIT) END) as HasChildCategories
                                FROM Categories c
								left join Categories pc on c.ParentCategoryId = pc.Id");
            if (string.IsNullOrEmpty(urlName))
            {
                return null;
            }
            else
            {
                sql.Append(" where c.UrlName = @categoryUrl");
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@categoryUrl",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Value = urlName
                });
            }
            cmd.CommandText = sql.ToString();

            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            adapter.Fill(result);

            if (result.Tables[0].Rows.Count == 0) return null;
            var category = result.Tables[0].Rows[0].ToObject<Category>();
            if (result.Tables[0].Rows[0]["ParentUrl"] != null)
            {
                category.ParentCategory = new Category
                {
                    UrlName = (String)result.Tables[0].Rows[0]["ParentUrl"],
                    Name  = (String)result.Tables[0].Rows[0]["ParentName"]
                };
            }
            return category;
        }
    }
}
