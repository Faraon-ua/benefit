﻿using Benefit.Common.Extensions;
using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Benefit.Domain.DataAccess
{
    public class CatalogParams
    {
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public string ProductParameters { get; set; }
        public string Vendors { get; set; }
        public string SellerNames { get; set; }
        public string SellerUrls { get; set; }
        public string OriginCountries { get; set; }
    }
    public class ProductsDBContext : AdoDbContext
    {
        private SqlCommand cmd;
        public ProductsDBContext()
        {
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
        }

        public CatalogParams GetCatalogParams(string categoryId, string sellerId)
        {
            var sellerWhere = sellerId == null ? string.Empty : "p.SellerId = @sellerId and";
            cmd.CommandText = string.Format(@"
            with result as
                (SELECT p.Id, 
	                (Select STRING_AGG(cast(StartValue as NVARCHAR(MAX)), ',') from ProductParameterProducts pp where pp.ProductId = p.Id) as ProductParameters,
	                p.Vendor,
	                (p.Price * ISNULL(c.Rate, 1)) as Price,
	                s.Name as SellerName,
	                s.UrlName as SellerUrl,
	                p.OriginCountry
                FROM Products p
	                join Sellers s on s.Id = p.SellerId
	                join Categories cat on cat.Id = p.CategoryId
	                left join Categories pCat on cat.MappedParentCategoryId = pCat.Id
	                left join Currencies c on p.CurrencyId = c.Id
                  WHERE
	                p.IsActive = 1 and
	                p.ModerationStatus = 0 and
	                {0}
	                (cat.IsActive = 1 or pCat.IsActive = 1) and
	                (cat.Id = @categoryId or pCat.Id = @categoryId)
                ) 
                select 
                (select STRING_AGG(cast(sub.Vendor as NVARCHAR(MAX)), ',') from (select distinct(Vendor) from result) as sub) as Vendors
                --,(select STRING_AGG(cast(sub.ProductParameters as NVARCHAR(MAX)), ',') from (select distinct(ProductParameters) from result) as sub) as ProductParameters
                ,(select STRING_AGG(cast(sub.SellerName as NVARCHAR(MAX)), ',') from (select distinct(SellerName) from result) as sub) as SellerNames
                ,(select STRING_AGG(cast(sub.SellerUrl as NVARCHAR(MAX)), ',') from (select distinct(SellerUrl) from result) as sub) as SellerUrls
                ,(select STRING_AGG(cast(sub.OriginCountry as NVARCHAR(MAX)), ',') from (select distinct(OriginCountry) from result) as sub) as OriginCountries
                ,Min(result.Price) as MinPrice
                ,Max(result.Price) as MaxPrice
                from result", sellerWhere);
            if (sellerId != null)
            {
                var sellerIdParam = new SqlParameter
                {
                    ParameterName = "@sellerId",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Value = sellerId
                };
                cmd.Parameters.Add(sellerIdParam);
            }
            var categoryIdParam = new SqlParameter
            {
                ParameterName = "@categoryId",
                SqlDbType = SqlDbType.NVarChar,
                Direction = ParameterDirection.Input,
                Value = categoryId
            };
            cmd.Parameters.Add(categoryIdParam);
            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            adapter.Fill(result);
            var drwRow = result.Tables[0].Rows[0];
            var catalog = drwRow.ToObject<CatalogParams>();
            if (catalog.ProductParameters == null)
            {
                catalog.ProductParameters = string.Empty;
            }
            return catalog;
        }
        public int GetCatalogCount(string categoryId, string sellerId, string where, IEnumerable<SqlParameter> sqlParameters)
        {
            var sellerWhere = sellerId == null ? string.Empty : "p.SellerId = @sellerId and";
            //products in mapped categories
            cmd.CommandText = string.Format(@"select count(Id) from
              (SELECT p.Id,
                (p.Price * ISNULL(c.Rate, 1)) as Price
              FROM Products p
	            left join Currencies c on c.Id = p.CurrencyId
	            join Sellers s on s.Id = p.SellerId
	            join Categories cat on cat.Id = p.CategoryId
	            left join Categories pCat on cat.MappedParentCategoryId = pCat.Id
              WHERE
	            p.IsActive = 1 and
	            p.ModerationStatus = 0 and
	            {0}
	            (cat.IsActive = 1 or pCat.IsActive = 1) and
	            (cat.Id = @categoryId or pCat.Id = @categoryId) {1}) as result", sellerWhere, where);
            if (sellerId != null)
            {
                var sellerIdParam = new SqlParameter
                {
                    ParameterName = "@sellerId",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Value = sellerId
                };
                cmd.Parameters.Add(sellerIdParam);
            }
            var categoryIdParam = new SqlParameter
            {
                ParameterName = "@categoryId",
                SqlDbType = SqlDbType.NVarChar,
                Direction = ParameterDirection.Input,
                Value = categoryId
            };
            cmd.Parameters.Add(categoryIdParam);
            cmd.Parameters.AddRange(sqlParameters.ToArray());
            int count = 0;
            cmd.Connection.Open();
            count = (int)cmd.ExecuteScalar();
            cmd.Connection.Close();
            return count;
        }
        public List<Product> GetCatalog(string categoryId, string sellerId, string userId, string where, string orderBy, int skip, int take, IEnumerable<SqlParameter> sqlParams)
        {
            var sellerWhere = sellerId == null ? string.Empty : "p.SellerId = @sellerId and";
            cmd.CommandText = string.Format(@"SELECT p.Id
                  ,p.SellerId
                    ,s.IsActive as SellerIsActive
                    ,s.Name as SellerName
	                ,s.SafePurchase as SellerSafePurchase
	                ,s.UrlName as SellerUrlName
	                ,s.UserDiscount as SellerUserDiscount
                    /*=Default Image=*/
                    ,img.ImageUrl as ImageUrl
                    ,img.IsAbsoluteUrl as ImageIsAbsoluteUrl
                    ,cat.IsActive as CategoryIsActive
                    ,cat.Name as CategoryName
                  ,p.IsActive
                  ,p.Name
                  ,p.UrlName
                  ,p.SKU
                  ,p.IsWeightProduct
                  ,p.AvailabilityState
                  ,(p.Price * ISNULL(c.Rate, 1)) as Price
                  ,(SELECT CASE WHEN EXISTS (SELECT * FROM Favorites WHERE ProductId = p.Id and UserId = '{1}')
                        THEN CAST(1 AS BIT)
                        ELSE CAST(0 AS BIT) END) as IsFavorite
                  ,(SELECT CASE WHEN EXISTS (SELECT * FROM Images WHERE ProductId = p.Id)
                          THEN CAST(1 AS BIT)
                          ELSE CAST(0 AS BIT) END) as HasImages
              FROM Products p
	            left join Currencies c on c.Id = p.CurrencyId
	            join Sellers s on s.Id = p.SellerId
	            join Categories cat on cat.Id = p.CategoryId
	            left join Categories pCat on cat.MappedParentCategoryId = pCat.Id
                left join Images img on img.Id = p.DefaultImageId
              WHERE
	            p.IsActive = 1 and
	            p.ModerationStatus = 0 and
	            {0}
	            (cat.IsActive = 1 or pCat.IsActive = 1) and
	            (cat.Id = @categoryId or pcat.Id = @categoryId) {2}
                order by {3}
	            OFFSET {4} ROWS FETCH NEXT {5} ROWS ONLY", sellerWhere, userId, where, orderBy, skip, take);
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@categoryId",
                SqlDbType = SqlDbType.NVarChar,
                Direction = ParameterDirection.Input,
                Value = categoryId
            });
            if (sellerId != null)
            {
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@sellerId",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Value = sellerId
                });
            }

            cmd.Parameters.AddRange(sqlParams.ToArray());
            return FetchProducts();
        }

        public List<Product> GetMainPageProducts()
        {
            cmd.CommandText = @"select distinct(p.Id), p.Name, p.UrlName, p.IsActive, p.SKU, p.Title, p.AvailabilityState, (p.Price * c.Rate) as Price, p.SellerId, p.IsNewProduct, p.IsFeatured,
                /*=Seller=*/
	            s.IsActive as SellerIsActive, 
	            s.Name as SellerName, 
	            s.SafePurchase as SellerSafePurchase, 
	            s.UrlName as SellerUrlName, 
	            s.UserDiscount as SellerUserDiscount, 
                /*=Default Image=*/
                img.ImageUrl as ImageUrl,
                img.IsAbsoluteUrl as ImageIsAbsoluteUrl,
	            (SELECT count(Id) FROM Reviews where ProductId = p.Id) as ReviewsCount,
                cat.IsActive as CategoryIsActive,
                cat.Name as CategoryName
	            FROM Products p
            left join Currencies c on c.Id = p.CurrencyId
            left join Reviews r on r.ProductId = p.Id
            join Sellers s on p.SellerId = s.Id
            left join Images img on img.Id = p.DefaultImageId
            join Categories cat on p.CategoryId = cat.Id
            where p.SellerId = s.Id and
	              p.CategoryId = cat.Id and
                  (p.AvailabilityState = 0 or p.AvailabilityState = 1) and
	              p.IsActive = 1 and
	              s.IsActive = 1 and
	              cat.IsActive = 1 and
                  (p.IsFeatured = 1 or p.IsNewProduct = 1) and
	              s.AreProductsFeatured = 1";
            return FetchProducts();
        }
        private List<Product> FetchProducts()
        {
            List<Product> returnList = new List<Product>();
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var result = new DataSet();
                adapter.Fill(result);
                foreach (DataRow drwRow in result.Tables[0].Rows)
                {
                    var product = drwRow.ToObject<Product>();
                    product.Seller = new Seller
                    {
                        Id = (String)drwRow["SellerId"],
                        IsActive = (bool)drwRow["SellerIsActive"],
                        Name = (String)drwRow["SellerName"],
                        SafePurchase = (bool)drwRow["SellerSafePurchase"],
                        UrlName = (String)drwRow["SellerUrlName"],
                        UserDiscount = (double)drwRow["SellerUserDiscount"]
                    };
                    product.Category = new Category
                    {
                        IsActive = (bool)drwRow["CategoryIsActive"],
                        Name = (String)drwRow["CategoryName"],
                    };
                    if (drwRow["ImageUrl"] != DBNull.Value)
                    {
                        product.DefaultImage = new Image
                        {
                            ImageUrl = (string)drwRow["ImageUrl"],
                            IsAbsoluteUrl = (bool)drwRow["ImageIsAbsoluteUrl"],
                        };
                    }

                    returnList.Add(product);
                }
                result.Dispose();
                cmd.Dispose();
                return returnList;
            }
        }
    }
}
