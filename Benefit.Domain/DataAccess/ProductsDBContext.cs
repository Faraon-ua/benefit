using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Benefit.Domain.DataAccess
{
    public class ProductsDBContext : AdoDbContext
    {
        private SqlCommand cmd;
        public ProductsDBContext(string connectionString)
        {
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
        }
        public List<Product> GetMainPageProducts()
        {
            List<Product> returnList = new List<Product>();
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

            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            adapter.Fill(result);

            //fill product images
            var imgResult = new DataSet();
            var productIds = result.Tables[0].AsEnumerable().Select(dataRow => string.Format("'{0}'", dataRow["Id"].ToString())).ToList();
            cmd.CommandText = string.Format("Select * from Images where ProductId in ({0})", string.Join(",", productIds));
            adapter.Fill(imgResult);

            foreach (DataRow drwRow in result.Tables[0].Rows)
            {
                returnList.Add(new Product
                {
                    Id = (String)drwRow["Id"],
                    Price = (double)drwRow["Price"],
                    Name = (String)drwRow["Name"],
                    SellerId = (String)drwRow["SellerId"],
                    UrlName = (String)drwRow["UrlName"],
                    Title = ConvertFromDBVal<String>(drwRow["Title"]),
                    SKU = (int)drwRow["SKU"],
                    IsFeatured = (bool)drwRow["IsFeatured"],
                    IsNewProduct = (bool)drwRow["IsNewProduct"],
                    AvailabilityState = (ProductAvailabilityState)(int)drwRow["AvailabilityState"],
                    Seller = new Seller
                    {
                        Name = (String)drwRow["SellerName"],
                        SafePurchase = (bool)drwRow["SellerSafePurchase"],
                        UrlName = (String)drwRow["SellerUrlName"],
                        UserDiscount = (double)drwRow["SellerUserDiscount"]
                    },
                    ReviewsCount = (int)drwRow["ReviewsCount"],
                    Images = imgResult.Tables[0].AsEnumerable().Where(entry=>entry["ProductId"].ToString() == (String)drwRow["Id"]).Select(img =>
                        new Image
                        {
                            ImageUrl = (string)img["ImageUrl"],
                            IsAbsoluteUrl = (bool)img["IsAbsoluteUrl"],
                            Order = (int)img["Order"],
                        }).ToList()
                });
            }
            return returnList;
        }
    }
}
