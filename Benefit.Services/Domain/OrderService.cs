using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Models.Admin;

namespace Benefit.Services.Domain
{
    public class OrderService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //public List<Order> GetOrders(OrdersFilters ordersFilters, int page = 0)
        //{
        //    var takePerPage = 50;

        //    var orders =
        //        db.Orders.Include(o => o.User)
        //            .Where(entry => entry.OrderType == ordersFilters.NavigationType);

        //    if (Seller.CurrentAuthorizedSellerId != null)
        //    {
        //        orders = orders.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId);
        //        var seller = db.Sellers.Find(Seller.CurrentAuthorizedSellerId);
        //        if (seller.RepeatingTransactionInterval != null)
        //        {
        //            orders.Where(
        //                entry =>
        //                    orders.Any(
        //                        o => o.Id != entry.Id &&
        //                            o.UserId == entry.UserId &&
        //                             DbFunctions.DiffHours(o.Time, entry.Time) < seller.RepeatingTransactionInterval)).ToList().ForEach(entry => entry.IsRepeating = true);
        //        }
        //    }
        //    if (!string.IsNullOrEmpty(ordersFilters.ClientName))
        //    {
        //        orders = orders.Where(entry => entry.User.FullName.ToLower().Contains(ordersFilters.ClientName.ToLower()));
        //    }
        //    if (!string.IsNullOrEmpty(ordersFilters.PersonnelName))
        //    {
        //        orders = orders.Where(entry => entry.PersonnelName.ToLower().Contains(ordersFilters.PersonnelName.ToLower()));
        //    }
        //    if (!string.IsNullOrEmpty(ordersFilters.DateRange))
        //    {
        //        var dateRangeValues = ordersFilters.DateRange.Split('-');
        //        var startDate = DateTime.Parse(dateRangeValues.First());
        //        var endDate = DateTime.Parse(dateRangeValues.Last()).AddTicks(-1).AddDays(1);
        //        orders = orders.Where(entry => entry.Time >= startDate && entry.Time <= endDate);
        //    }
        //    if (ordersFilters.SellerId != null)
        //    {
        //        orders = orders.Where(entry => entry.SellerId == ordersFilters.SellerId);
        //    }
        //    if (!string.IsNullOrEmpty(ordersFilters.PaymentType))
        //    {
        //        orders = orders.Where(entry => ordersFilters.PaymentType.Contains(entry.PaymentType.ToString()));
        //    }
        //    else
        //    {
        //        ordersFilters.PaymentType = string.Empty;
        //    }
        //    if (ordersFilters.NavigationType == OrderType.BenefitSite)
        //    {
        //        if (!string.IsNullOrEmpty(ordersFilters.Status))
        //        {
        //            orders = orders.Where(entry => ordersFilters.Status.Contains(entry.Status.ToString()));
        //        }
        //        else
        //        {
        //            ordersFilters.Status = string.Empty;
        //        }
        //        if (ordersFilters.OrderNumber.HasValue)
        //        {
        //            orders = orders.Where(entry => entry.OrderNumber == ordersFilters.OrderNumber);
        //        }
        //    }
        //    if (ordersFilters.ClientGrouping)
        //    {
        //        var groupedOrders =
        //            orders.GroupBy(entry => entry.UserId).OrderByDescending(entry => entry.Count()).ToList();
        //        orders = groupedOrders.SelectMany(entry => entry.OrderByDescending(grp => grp.Time)).AsQueryable();
        //    }
        //    else
        //    {
        //        switch (ordersFilters.Sort)
        //        {
        //            case OrderSortOption.DateAsc:
        //                orders = orders.OrderBy(entry => entry.Time);
        //                break;
        //            case OrderSortOption.DateDesc:
        //                orders = orders.OrderByDescending(entry => entry.Time);
        //                break;
        //            case OrderSortOption.SumAsc:
        //                orders = orders.OrderBy(entry => entry.Sum);
        //                break;
        //            case OrderSortOption.SumDesc:
        //                orders = orders.OrderByDescending(entry => entry.Sum);
        //                break;
        //        }
        //    }
        //    var ordersTotal = orders.Count();
        //    ordersFilters.Sum = orders.ToList().Select(l => l.SumWithDiscount).DefaultIfEmpty(0).Sum();
        //    ordersFilters.Number = orders.Count();
        //    orders = orders.Skip(page * takePerPage).Take(takePerPage);

        //    ordersFilters.Orders = new PaginatedList<Order>
        //    {
        //        Items = orders.ToList(),
        //        Pages = ordersTotal / takePerPage + 1,
        //        ActivePage = page
        //    };
        //    ordersFilters.Sorting = (from OrderSortOption sortOption in Enum.GetValues(typeof(OrderSortOption))
        //                             select
        //                                 new SelectListItem()
        //                                 {
        //                                     Text = Enumerations.GetEnumDescription(sortOption),
        //                                     Value = sortOption.ToString(),
        //                                     Selected = sortOption == ordersFilters.Sort
        //                                 }).ToList();
        //    ordersFilters.Sellers =
        //        db.Sellers.OrderBy(entry => entry.Name)
        //            .Select(entry => new SelectListItem { Text = entry.Name, Value = entry.Id });
        //}



        public string AddOrder(CompleteOrder model)
        {
            var seller = db.Sellers.Include(entry => entry.SellerCategories).FirstOrDefault(entry => entry.Id == model.Order.SellerId);
            var order = model.Order;
            order.Id = Guid.NewGuid().ToString();
            var orderNumber = db.Orders.Max(entry => (int?)entry.OrderNumber) ?? SettingsService.OrderMinValue;
            order.OrderNumber = orderNumber + 1;

            order.Sum = order.GetOrderSum();
            order.Description =
                string.Format("{0}<br/>--------------------------------------------<br/> Заказ створено на {1}",
                    model.Comment, HttpContext.Current.Request.Url.Host);
            //order.PersonalBonusesSum = order.SumWithDiscount * seller.UserDiscount / 100;
            order.PersonalBonusesSum = order.OrderProducts.Sum(entry => entry.BonusesAcquired * entry.Amount);

            order.PointsSum = Double.IsInfinity(order.Sum / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount]) ? 0 : order.SumWithDiscount / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount];
            order.SellerName = seller.Name;

            var shipping = db.ShippingMethods.FirstOrDefault(entry => entry.Id == model.ShippingMethodId);
            order.ShippingCost = order.Sum < shipping.FreeStartsFrom ? (shipping.CostBeforeFree ?? default(double)) : 0;
            order.ShippingName = shipping.Name;
            if (string.IsNullOrEmpty(order.ShippingAddress))
            {
                var address = db.Addresses.FirstOrDefault(entry => entry.Id == model.AddressId);
                if (address != null)
                {
                    order.ShippingAddress = string.Format("{0}; {1}; {2}, {3}", address.FullName, address.Phone,
                        address.Region.Name_ua, address.AddressLine);
                }
            }
            order.Time = DateTime.UtcNow;
            order.OrderType = OrderType.BenefitSite;
            order.PaymentType = model.PaymentType.Value;

            //add order to DB
            db.Orders.Add(order);
            var i = 0;
            //handle wholesale prices
            foreach (var product in order.OrderProducts)
            {
                product.Id = Guid.NewGuid().ToString();
                product.ProductPrice = product.ActualPrice;
                product.OrderId = order.Id;
                product.Index = i++;
                db.OrderProducts.Add(product);
                foreach (var orderProductOption in product.OrderProductOptions)
                {
                    orderProductOption.OrderProductId = product.Id;
                    db.OrderProductOptions.Add(orderProductOption);
                }
                //add seller transaction
                var dbProduct = db.Products.Include(entry => entry.Seller.SellerCategories).FirstOrDefault(entry => entry.Id == product.ProductId);
                var sellerCategory =
                   seller.SellerCategories.FirstOrDefault(entry => entry.CategoryId == dbProduct.CategoryId) ??
                   seller.SellerCategories.FirstOrDefault(entry =>
                       entry.CategoryId == dbProduct.Category.MappedParentCategoryId);
                var comissionPercent = sellerCategory == null ? dbProduct.Seller.TotalDiscount : sellerCategory.CustomDiscount ?? dbProduct.Seller.TotalDiscount;
                var sellerTransaction = new SellerTransaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    Number = db.SellerTransactions.Select(entry => entry.Number).DefaultIfEmpty(10000).Max() + 1,
                    SellerId = model.Order.SellerId,
                    Time = DateTime.UtcNow,
                    ProductSKU = product.ProductSku.Value,
                    ProductUrlName = product.ProductUrlName,
                    Amount = product.Amount,
                    Price = product.ActualPrice,
                    TotalPrice = product.ActualPrice * product.Amount,
                    Type = SellerTransactionType.Reserve,
                    OrderNumber = model.Order.OrderNumber,
                    Charge = null,
                    Writeoff = (product.ActualPrice * product.Amount) * comissionPercent / 100,
                    Balance = seller.CurrentBill,
                    GreyZoneBalance = seller.GreyZone + (product.ActualPrice * product.Amount) * comissionPercent / 100
                };
                seller.GreyZone = sellerTransaction.GreyZoneBalance;
                db.SellerTransactions.Add(sellerTransaction);
                if (seller.CurrentBill - seller.GreyZone < 0)
                {
                    seller.BlockOn = DateTime.UtcNow.AddDays(5);
                }
            }
            db.SaveChanges();

            //add transaction to reduce bonuses account
            if (order.PaymentType == PaymentType.Bonuses)
            {
                var TransactionsService = new TransactionsService();
                TransactionsService.AddBonusesOrderTransaction(order);
            }

            Cart.Cart.CurrentInstance.ClearSellerOrder(model.Order.SellerId);
            var cartNumberCookie = HttpContext.Current.Response.Cookies["cartNumber"];
            var cartPriceCookie = HttpContext.Current.Response.Cookies["cartPrice"];
            if (cartNumberCookie != null)
                if (Cart.Cart.CurrentInstance.Orders.Count == 0)
                {
                    cartNumberCookie.Expires = DateTime.UtcNow.AddDays(-1);
                    cartPriceCookie.Expires = DateTime.UtcNow.AddDays(-1);
                }
                else
                {
                    cartNumberCookie.Value = Cart.Cart.CurrentInstance.GetOrderProductsCountAndPrice().ProductsNumber.ToString();
                    cartPriceCookie.Value = Cart.Cart.CurrentInstance.GetOrderProductsCountAndPrice().Price.ToString();
                }
            return order.OrderNumber.ToString();
        }

        public void DeleteOrder(string orderId)
        {
            var order = db.Orders.Include(entry => entry.OrderProducts).Include(entry => entry.User)
                .Include(entry => entry.Transactions).FirstOrDefault(entry => entry.Id == orderId);
            var now = DateTime.UtcNow;
            var seller = db.Sellers.FirstOrDefault(entry => entry.Id == order.SellerId);
            if (seller != null)
            {
                if (order.Time > now.StartOfMonth() && order.Time < now.EndOfMonth())
                {
                    seller.PointsAccount = seller.PointsAccount - order.PointsSum;
                }
                if (order.Time > now.StartOfMonth().AddMonths(-1) && order.Time < now.EndOfMonth().AddMonths(-1))
                {
                    seller.HangingPointsAccount = seller.HangingPointsAccount - order.PointsSum;
                }
                db.Entry(seller).State = EntityState.Modified;
            }
            if (order.Time > now.StartOfMonth() && order.Time < now.EndOfMonth())
            {
                order.User.PointsAccount = order.User.PointsAccount - order.PointsSum;
                order.User.CurrentBonusAccount = order.User.CurrentBonusAccount - order.PersonalBonusesSum;
                if (order.PaymentType == PaymentType.Bonuses)
                {
                    order.User.BonusAccount = order.User.BonusAccount + order.Sum;
                }
            }
            if (order.Time > now.StartOfMonth().AddMonths(-1) && order.Time < now.EndOfMonth().AddMonths(-1))
            {
                order.User.HangingPointsAccount = order.User.HangingPointsAccount - order.PointsSum;
                order.User.HangingBonusAccount = order.User.HangingBonusAccount - order.PersonalBonusesSum;
                if (order.PaymentType == PaymentType.Bonuses)
                {
                    order.User.BonusAccount = order.User.BonusAccount + order.Sum;
                }
            }

            db.Transactions.RemoveRange(order.Transactions);
            db.OrderProductOptions.RemoveRange(order.OrderProducts.SelectMany(op => op.OrderProductOptions));
            db.OrderProducts.RemoveRange(order.OrderProducts);
            db.Entry(order.User).State = EntityState.Modified;
            db.Orders.Remove(order);
            db.SaveChanges();
        }
    }
}
