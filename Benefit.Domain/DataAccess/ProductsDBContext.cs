using Benefit.Common.Extensions;
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
        public ProductsDBContext(string connectionString)
        {
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
        }

        public CatalogParams GetCatalogParams(string categoryId, string sellerId)
        {
            cmd.CommandText = string.Format(@"
            with result as
                (SELECT p.Id, 
	                (Select STRING_AGG(StartValue, ', ') from ProductParameterProducts pp where pp.ProductId = p.Id) as ProductParameters,
	                p.Vendor,
	                (p.Price *  ISNULL(c.Rate, 1)) as Price,
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
	                p.SellerId = '{0}' and
	                (cat.IsActive = 1 or pCat.IsActive = 1) and
	                (cat.Id = '{1}' or pCat.Id = '{1}')
                ) 
                select 
                (select STRING_AGG(cast(sub.Vendor as NVARCHAR(MAX)), ',') from (select distinct(Vendor) from result) as sub) as Vendors
                ,(select STRING_AGG(cast(sub.ProductParameters as NVARCHAR(MAX)), ',') from (select distinct(ProductParameters) from result) as sub) as ProductParameters
                ,(select STRING_AGG(cast(sub.SellerName as NVARCHAR(MAX)), ',') from (select distinct(SellerName) from result) as sub) as SellerNames
                ,(select STRING_AGG(cast(sub.SellerUrl as NVARCHAR(MAX)), ',') from (select distinct(SellerUrl) from result) as sub) as SellerUrls
                ,(select STRING_AGG(cast(sub.OriginCountry as NVARCHAR(MAX)), ',') from (select distinct(OriginCountry) from result) as sub) as OriginCountries
                ,Min(result.Price) as MinPrice
                ,Max(result.Price) as MaxPrice
                from result", sellerId, categoryId);
            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            adapter.Fill(result);
            var drwRow = result.Tables[0].Rows[0];
            return drwRow.ToObject<CatalogParams>();
        }
        public int GetCatalogCount(string categoryId, string sellerId, string where)
        {
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
	            p.SellerId = '{0}' and
	            (cat.IsActive = 1 or pCat.IsActive = 1) and
	            (cat.Id = '{1}' or pCat.Id = '{1}') {2}) as result", sellerId, categoryId, where);
            int count = 0;
            cmd.Connection.Open();
            count = (int)cmd.ExecuteScalar();
            cmd.Connection.Close();
            return count;
        }
        public List<Product> GetCatalog(string categoryId, string sellerId, string userId, string where, string orderBy, int skip, int take)
        {
            cmd.CommandText = string.Format(@"SELECT p.Id
                  ,p.Name
                  ,p.UrlName
                  ,p.SKU
                  ,(p.Price * ISNULL(c.Rate, 1)) as Price
                  ,(SELECT CASE WHEN EXISTS (SELECT * FROM Favorites WHERE ProductId = p.Id and UserId = '{0}')
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
              WHERE
	            p.IsActive = 1 and
	            p.ModerationStatus = 0 and
	            p.SellerId = '{1}' and
	            (cat.IsActive = 1 or pCat.IsActive = 1) and
	            (cat.Id = '{2}' or pcat.Id = '{2}') {3}
                order by {4}
	            OFFSET {5} ROWS FETCH NEXT {6} ROWS ONLY", userId, sellerId, categoryId, where, orderBy, skip, take);
            return FetchProducts(false, true);
        }

        public List<Product> GetMainPageProducts()
        {
            cmd.CommandText = @"select distinct(p.Id), p.Name, p.UrlName, p.SKU, p.Title, p.AvailabilityState, (p.Price * c.Rate) as Price, p.SellerId, p.IsNewProduct, p.IsFeatured,
	            s.Name as SellerName, 
	            s.SafePurchase as SellerSafePurchase, 
	            s.UrlName as SellerUrlName, 
	            s.UserDiscount as SellerUserDiscount, 
	            (SELECT count(Id) FROM Reviews where ProductId = p.Id) as ReviewsCount
	            FROM Products p
            left join Currencies c on c.Id = p.CurrencyId
            left join Reviews r on r.ProductId = p.Id
            join Sellers s on p.SellerId = s.Id
            join Categories cat on p.CategoryId = cat.Id
            where p.SellerId = s.Id and
	              p.CategoryId = cat.Id and
                  (p.AvailabilityState = 0 or p.AvailabilityState = 1) and
	              p.IsActive = 1 and
	              s.IsActive = 1 and
	              cat.IsActive = 1 and
                  (p.IsFeatured = 1 or p.IsNewProduct = 1) and
	              s.AreProductsFeatured = 1";
            return FetchProducts(true, true);
        }
        private List<Product> FetchProducts(bool fetchSeller, bool fetchImages)
        {
            List<Product> returnList = new List<Product>();
            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            var imgResult = new DataSet();
            adapter.Fill(result);

            if (fetchImages)
            {
                var productIds = result.Tables[0].AsEnumerable().Select(dataRow => string.Format("'{0}'", dataRow["Id"].ToString())).ToList();
                if (!productIds.Any()) productIds.Add("'0'");
                cmd.CommandText = string.Format("Select * from Images where ProductId in ({0})", string.Join(",", productIds));
                adapter.Fill(imgResult);
            }
            foreach (DataRow drwRow in result.Tables[0].Rows)
            {
                var product = drwRow.ToObject<Product>();
                if (fetchSeller)
                {
                    product.Seller = new Seller
                    {
                        Name = (String)drwRow["SellerName"],
                        SafePurchase = (bool)drwRow["SellerSafePurchase"],
                        UrlName = (String)drwRow["SellerUrlName"],
                        UserDiscount = (double)drwRow["SellerUserDiscount"]
                    };
                }
                if (fetchImages)
                {
                    product.Images = imgResult.Tables[0].AsEnumerable().Where(entry => entry["ProductId"].ToString() == (String)drwRow["Id"]).Select(img =>
                              img.ToObject<Image>()).ToList();
                }
                //returnList.Add(new Product
                //{
                //    Id = (String)drwRow["Id"],
                //    Price = (double)drwRow["Price"],
                //    Name = (String)drwRow["Name"],
                //    SellerId = (String)drwRow["SellerId"],
                //    UrlName = (String)drwRow["UrlName"],
                //    Title = ConvertFromDBVal<String>(drwRow["Title"]),
                //    SKU = (int)drwRow["SKU"],
                //    IsFeatured = (bool)drwRow["IsFeatured"],
                //    IsNewProduct = (bool)drwRow["IsNewProduct"],
                //    AvailabilityState = (ProductAvailabilityState)(int)drwRow["AvailabilityState"],
                //    Seller = new Seller
                //    {
                //        Name = (String)drwRow["SellerName"],
                //        SafePurchase = (bool)drwRow["SellerSafePurchase"],
                //        UrlName = (String)drwRow["SellerUrlName"],
                //        UserDiscount = (double)drwRow["SellerUserDiscount"]
                //    },
                //    ReviewsCount = (int)drwRow["ReviewsCount"],
                //    Images = imgResult.Tables[0].AsEnumerable().Where(entry=>entry["ProductId"].ToString() == (String)drwRow["Id"]).Select(img =>
                //        new Image
                //        {
                //            ImageUrl = (string)img["ImageUrl"],
                //            IsAbsoluteUrl = (bool)img["IsAbsoluteUrl"],
                //            Order = (int)img["Order"],
                //        }).ToList()
                //});
                returnList.Add(product);
            }
            return returnList;
        }
    }
}
