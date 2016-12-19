using System;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using System.Data.Entity;

namespace Benefit.Services.Domain
{
    public class TransactionsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public void AddOrderTransaction(Order order)
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
                Time = DateTime.UtcNow
            };
            user.CurrentBonusAccount = transaction.BonusesBalans.Value;
            user.PointsAccount += order.PointsSum;

            if (order.PaymentType == PaymentType.Bonuses)
            {
                //add transaction for personal purchase
                var bonusesPaymentTransaction = new Transaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    Bonuses = -order.Sum,
                    BonusesBalans = user.BonusAccount - order.Sum,
                    OrderId = order.Id,
                    PayeeId = user.Id,
                    Time = DateTime.UtcNow
                };
                user.BonusAccount = bonusesPaymentTransaction.BonusesBalans.Value;
                db.Transactions.Add(bonusesPaymentTransaction);
            }
            db.Transactions.Add(transaction);
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
