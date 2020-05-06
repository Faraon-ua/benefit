using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Benefit.Domain.DataAccess
{
    public class BannersDBContext : AdoDbContext
    {
        private SqlCommand cmd;
        public BannersDBContext()
        {
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
        }
        public List<Banner> Get(string whereParams)
        {
            List<Banner> returnList = new List<Banner>();
            cmd.CommandText = string.Format(@"SELECT * FROM [Benefit.com].[dbo].[Banners] where {0}", whereParams);

            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            adapter.Fill(result);

            foreach (DataRow drwRow in result.Tables[0].Rows)
            {
                returnList.Add(new Banner
                {
                    BannerType = (BannerType)drwRow["BannerType"],
                    ImageUrl = (string)drwRow["ImageUrl"],
                    Order = (int)drwRow["Order"]
                });
            }
            return returnList;
        }
    }
}
