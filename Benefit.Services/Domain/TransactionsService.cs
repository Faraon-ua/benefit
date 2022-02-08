using System;
using System.Data.Common;
using System.Linq;
using Benefit.Common.Exceptions;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using System.Data.Entity;

namespace Benefit.Services.Domain
{
    public class TransactionsService
    {
        public void AddBonusesOrderAbandonedTransaction(Order order, ApplicationDbContext transactionDb)
        {
            var user = transactionDb.Users.Find(order.UserId);
            var transaction =
                transactionDb.Transactions.FirstOrDefault(
                    entry => entry.OrderId == order.Id && entry.Type == TransactionType.BonusesOrderPayment);
            if (transaction == null)
            {
                throw new ServiceException("No transaction for order " + order.Id);
            }

            //add transaction for personal purchase
            var bonuses = Math.Abs(transaction.Bonuses);
            var bonusesPaymentTransaction = new Transaction()
            {
                Id = Guid.NewGuid().ToString(),
                Bonuses = bonuses,
                BonusesBalans = user.BonusAccount + bonuses,
                OrderId = order.Id,
                PayeeId = user.Id,
                Time = DateTime.UtcNow,
                Type = TransactionType.BonusesOrderAbandonedPayment
            };
            user.BonusAccount = bonusesPaymentTransaction.BonusesBalans;

            transactionDb.Transactions.Add(bonusesPaymentTransaction);
            transactionDb.Entry(user).State = EntityState.Modified;
        }

        public void AddBonusesOrderTransaction(Order order)
        {
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(order.UserId);
                var sumToPay = order.Sum - order.SellerDiscount.GetValueOrDefault(0);
                //add transaction for personal purchase
                var bonusesPaymentTransaction = new Transaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    Bonuses = -(sumToPay),
                    BonusesBalans = user.BonusAccount - sumToPay,
                    OrderId = order.Id,
                    PayeeId = user.Id,
                    Time = DateTime.UtcNow,
                    Type = TransactionType.BonusesOrderPayment
                };
                user.BonusAccount = bonusesPaymentTransaction.BonusesBalans;

                db.Transactions.Add(bonusesPaymentTransaction);
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void AddOrderFinishedSellerTransaction(Order order, ApplicationDbContext transactionDb)
        {
            var seller = transactionDb.Sellers.Include(entry => entry.SellerCategories).FirstOrDefault(entry => entry.Id == order.SellerId);
            foreach (var orderProduct in order.OrderProducts)
            {
                var product = transactionDb.Products.FirstOrDefault(entry => entry.Id == orderProduct.ProductId) ??
                    new Product
                    {
                        UrlName = orderProduct.ProductName,
                        Seller = seller,
                        Category = new Category
                        {
                            MappedParentCategoryId = null
                        }
                    };
                var sellerCategory =
                   seller.SellerCategories.FirstOrDefault(entry => entry.CategoryId == product.CategoryId) ??
                   seller.SellerCategories.FirstOrDefault(entry =>
                       entry.CategoryId == product.Category.MappedParentCategoryId);
                var comissionPercent = sellerCategory == null ? product.Seller.TotalDiscount : sellerCategory.CustomDiscount ?? product.Seller.TotalDiscount;
                var sellerTransaction = new SellerTransaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    Number = transactionDb.SellerTransactions.Select(entry => entry.Number).DefaultIfEmpty(10000).Max() + 1,
                    SellerId = order.SellerId,
                    Time = DateTime.UtcNow,
                    ProductSKU = orderProduct.ProductSku.GetValueOrDefault(default(int)),
                    ProductUrlName = product.UrlName,
                    Amount = orderProduct.Amount,
                    Price = orderProduct.ActualPrice,
                    TotalPrice = orderProduct.ActualPrice * orderProduct.Amount,
                    OrderNumber = order.OrderNumber,
                    Charge = (orderProduct.ActualPrice * orderProduct.Amount) * comissionPercent / 100,
                    Writeoff = (orderProduct.ActualPrice * orderProduct.Amount) * comissionPercent / 100,
                    FeePercent = comissionPercent
                };
                if (order.PaymentType == PaymentType.Bonuses)
                {
                    sellerTransaction.Type = SellerTransactionType.Bonuses;
                    sellerTransaction.Charge = orderProduct.ActualPrice * orderProduct.Amount - (orderProduct.ActualPrice * orderProduct.Amount * comissionPercent / 100);
                    sellerTransaction.Writeoff = null;
                    sellerTransaction.Balance = seller.CurrentBill + sellerTransaction.Charge.Value;
                    sellerTransaction.GreyZoneBalance = seller.GreyZone - (orderProduct.ActualPrice * orderProduct.Amount * comissionPercent / 100);
                }
                else
                {
                    sellerTransaction.Type = SellerTransactionType.SalesComission;
                    sellerTransaction.Balance = seller.CurrentBill - sellerTransaction.Writeoff.Value;
                    sellerTransaction.GreyZoneBalance = seller.GreyZone - sellerTransaction.Charge.Value;
                }
                seller.GreyZone = sellerTransaction.GreyZoneBalance;
                seller.CurrentBill = sellerTransaction.Balance;
                transactionDb.SellerTransactions.Add(sellerTransaction);
            }
            transactionDb.SaveChanges();
        }

        public void AddOrderAbandonedSellerTransaction(Order order, ApplicationDbContext transactionDb)
        {
            var seller = transactionDb.Sellers.Include(entry => entry.SellerCategories).FirstOrDefault(entry => entry.Id == order.SellerId);
            foreach (var orderProduct in order.OrderProducts)
            {
                var product = transactionDb.Products.FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                var sellerCategory =
                   seller.SellerCategories.FirstOrDefault(entry => entry.CategoryId == product.CategoryId) ??
                   seller.SellerCategories.FirstOrDefault(entry =>
                       entry.CategoryId == product.Category.MappedParentCategoryId);
                var comissionPercent = sellerCategory == null ? product.Seller.TotalDiscount : sellerCategory.CustomDiscount ?? product.Seller.TotalDiscount;
                var sellerTransaction = new SellerTransaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    Number = transactionDb.SellerTransactions.Select(entry => entry.Number).DefaultIfEmpty(10000).Max() + 1,
                    SellerId = order.SellerId,
                    Time = DateTime.UtcNow,
                    ProductSKU = orderProduct.ProductSku.Value,
                    ProductUrlName = orderProduct.ProductUrlName,
                    Amount = orderProduct.Amount,
                    Price = orderProduct.ActualPrice,
                    TotalPrice = orderProduct.ActualPrice * orderProduct.Amount,
                    Type = SellerTransactionType.FailOrderReserveReturn,
                    OrderNumber = order.OrderNumber,
                    Charge = (orderProduct.ActualPrice * orderProduct.Amount) * comissionPercent / 100,
                    Writeoff = null,
                };
                sellerTransaction.Balance = seller.CurrentBill;
                sellerTransaction.GreyZoneBalance = seller.GreyZone - sellerTransaction.Charge.Value;
                seller.GreyZone = sellerTransaction.GreyZoneBalance;
                seller.CurrentBill = sellerTransaction.Balance;
                transactionDb.SellerTransactions.Add(sellerTransaction);
            }
            transactionDb.SaveChanges();
        }
        public void AddOrderFinishedTransaction(Order order, ApplicationDbContext transactionDb)
        {
            var user = transactionDb.Users.Find(order.UserId);
            //add transaction for personal purchase
            var transaction = new Transaction()
            {
                Id = Guid.NewGuid().ToString(),
                Bonuses = order.PersonalBonusesSum,
                BonusesBalans = user.HangingBonusAccount + order.PersonalBonusesSum,
                OrderId = order.Id,
                PayeeId = user.Id,
                Time = DateTime.UtcNow,
                Type = TransactionType.CashbackBonus
            };
            user.HangingBonusAccount = transaction.BonusesBalans;

            if (order.PaymentType == PaymentType.Bonuses)
            {
                var orderTransaction =
                    transactionDb.Transactions.FirstOrDefault(
                        entry => entry.OrderId == order.Id && entry.Type == TransactionType.BonusesOrderPayment);
                if (orderTransaction == null)
                {
                    throw new ServiceException("No transaction for order " + order.Id);
                }
                if (order.Sum < Math.Abs(orderTransaction.Bonuses))
                {
                    var difference = (Math.Abs(orderTransaction.Bonuses) - order.Sum);
                    var refundTransaction = new Transaction()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Bonuses = difference,
                        BonusesBalans = user.BonusAccount + difference,
                        OrderId = order.Id,
                        PayeeId = user.Id,
                        Time = DateTime.UtcNow,
                        Type = TransactionType.OrderRefund
                    };
                    user.BonusAccount = refundTransaction.BonusesBalans;
                    transactionDb.Transactions.Add(refundTransaction);
                }
            }
            transactionDb.Transactions.Add(transaction);
            transactionDb.Entry(user).State = EntityState.Modified;
        }

        public PartnerTransactionsViewModel GetPartnerTransactions(string id, DateTime start, DateTime end)
        {
            using (var db = new ApplicationDbContext())
            {
                var user =
                db.Users.Include(entry => entry.Region)
                    .Include(entry => entry.Transactions)
                    .Include(entry => entry.Transactions.Select(tr => tr.Order))
                    .Include(entry => entry.Transactions.Select(tr => tr.Payer))
                    .FirstOrDefault(entry => entry.Id == id);
                var model = new PartnerTransactionsViewModel
                {
                    User = user
                };
                model.Transactions= user.Transactions
                    .Where(entry => entry.Time > start && entry.Time < end)
                    .OrderByDescending(entry => entry.Time).ToList();
                return model;
            }
        }
    }
}