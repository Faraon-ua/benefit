using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Benefit.Domain.DataAccess
{
    public class InfoPagesDBContext : AdoDbContext
    {
        private SqlCommand cmd;
        public InfoPagesDBContext()
        {
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
        }
        public List<InfoPage> GetMainPagesNews()
        {
            List<InfoPage> returnList = new List<InfoPage>();
            cmd.CommandText = @"SELECT top 6 p.* FROM InfoPages p
  where (p.IsNews = 1 and p.IsActive = 1) or
  p.UrlName = 'golovna'
  order by case when p.UrlName='golovna' then 0 else 1 end, p.CreatedOn desc";

            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            adapter.Fill(result);

            foreach (DataRow drwRow in result.Tables[0].Rows)
            {
                returnList.Add(new InfoPage
                {
                    Name = (string)drwRow["Name"],
                    UrlName = (string)drwRow["UrlName"],
                    ImageUrl = ConvertFromDBVal<String>(drwRow["ImageUrl"]),
                    Order = (int)drwRow["Order"],
                    CreatedOn = (DateTime)drwRow["CreatedOn"],
                    Title = ConvertFromDBVal<String>(drwRow["Title"]),
                    ShortContent = ConvertFromDBVal<String>(drwRow["ShortContent"]),
                    Content = ConvertFromDBVal<String>(drwRow["Content"]),
                    IsNews = (bool)drwRow["IsNews"]
                });
            }
            return returnList;
        }
    }
}
