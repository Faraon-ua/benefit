using System;
using System.Linq;
using Benefit.Common.Exceptions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using System.Data.Entity;

namespace Benefit.Services.Domain
{
    public class TransactionsService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public void AddBonusesOrderAbandonedTransaction(Order order)
        {
            var user = db.Users.Find(order.UserId);
            var transaction =
                db.Transactions.FirstOrDefault(
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
                Bonuses = bonuses + transaction.Commission,
                BonusesBalans = user.BonusAccount + (bonuses + transaction.Commission),
                OrderId = order.Id,
                PayeeId = user.Id,
                Time = DateTime.UtcNow,
                Type = TransactionType.BonusesOrderAbandonedPayment
            };
            user.BonusAccount = bonusesPaymentTransaction.BonusesBalans;

            db.Transactions.Add(bonusesPaymentTransaction);
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();   
        }
        public void AddBonusesOrderTransaction(Order order)
        {
            var user = db.Users.Find(order.UserId);
            var commission = order.Sum * SettingsService.BonusesComissionRate / 100;
            //add transaction for personal purchase
            var bonusesPaymentTransaction = new Transaction()
            {
                Id = Guid.NewGuid().ToString(),
                Bonuses = -(order.Sum),
                Commission = commission,
                BonusesBalans = user.BonusAccount - (order.Sum + commission),
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

        public void AddOrderFinishedTransaction(Order order)
        {
            var user = db.Users.Find(order.UserId);

            //add transaction for personal purchase
            var transaction = new Transaction()
            {
                Id = Guid.NewGuid().ToString(),
                Bonuses = order.PersonalBonusesSum,
                BonusesBalans = user.CurrentBonusAccount + order.PersonalBonusesSum,
                OrderId = order.Id,
                PayeeId = user.Id,
                Time = DateTime.UtcNow,
                Type = TransactionType.PersonalSiteBonus
            };
            user.CurrentBonusAccount = transaction.BonusesBalans;
            user.PointsAccount += order.PointsSum;
            
            if (order.PaymentType == PaymentType.Bonuses)
            {
                var orderTransaction =
                    db.Transactions.FirstOrDefault(
                        entry => entry.OrderId == order.Id && entry.Type == TransactionType.BonusesOrderPayment);
                if (orderTransaction == null)
                {
                    throw new ServiceException("No transaction for order " + order.Id);
                }
                if (order.Sum < Math.Abs(transaction.Bonuses))
                {
                    var difference = (Math.Abs(transaction.Bonuses) - order.Sum);
                    difference = difference + difference * SettingsService.BonusesComissionRate / 100;
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
                    db.Transactions.Add(refundTransaction);
                }
            }

            db.Transactions.Add(transaction);
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
