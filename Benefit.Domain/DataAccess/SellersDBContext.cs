﻿using Benefit.Common.Extensions;
using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Benefit.Domain.DataAccess
{
    public class SellersDBContext : AdoDbContext
    {
        private SqlCommand cmd;
        public SellersDBContext()
        {
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
        }
        public List<Seller> Get(string where)
        {
            List<Seller> returnList = new List<Seller>();
            cmd.CommandText = string.Format(@"SELECT s.* 
                                              FROM Sellers s
                                              where {0}", where);

            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            adapter.Fill(result);

            foreach (DataRow drwRow in result.Tables[0].Rows)
            {
                returnList.Add(drwRow.ToObject<Seller>());
            }
            return returnList;
        }
        public List<Seller> GetBrands()
        {
            List<Seller> returnList = new List<Seller>();
            cmd.CommandText = @"SELECT s.Name, s.UrlName, 
                (select top 1 ImageUrl from Images where SellerId = s.Id and ImageType = 0) as ImageUrl
                FROM Sellers s
                where s.IsActive = 1 and s.IsFeatured = 1";

            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            adapter.Fill(result);

            foreach (DataRow drwRow in result.Tables[0].Rows)
            {
                returnList.Add(new Seller
                {
                    Name = (string)drwRow["Name"],
                    UrlName = (string)drwRow["UrlName"],
                    Images = new List<Image>()
                    {
                        new Image
                        {
                            ImageUrl = (string)drwRow["ImageUrl"]
                        }
                    }
                });
            }
            return returnList;
        }
    }
}
