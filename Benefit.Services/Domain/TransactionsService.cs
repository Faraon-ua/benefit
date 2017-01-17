using System;
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
        private ApplicationDbContext db = new ApplicationDbContext();

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
                if (order.Sum < Math.Abs(orderTransaction.Bonuses))
                {
                    var difference = (Math.Abs(orderTransaction.Bonuses) - order.Sum);
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

        public PartnerTransactionsViewModel GetPartnerTransactions(string id, DateTime start, DateTime end)
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
            model.General.AddRange(
                user.Transactions.Where(
                    entry =>
                        entry.Type == TransactionType.BonusesOrderPayment ||
                        entry.Type == TransactionType.OrderRefund ||
                        entry.Type == TransactionType.BonusesOrderAbandonedPayment ||
                        entry.Type == TransactionType.PersonalMonthAggregate ||
                        entry.Type == TransactionType.VIPBonus ||
                        entry.Type == TransactionType.VIPSellerBonus));

            foreach (
                var date in
                    user.Transactions.Where(entry => entry.Type == TransactionType.MentorBonus)
                        .Select(entry => new DateTime(entry.Time.Year, entry.Time.Month, 1))
                        .Distinct())
            {
                var personalTransaction =
                    user.Transactions.FirstOrDefault(
                        entry =>
                            entry.Time.Year == date.Year && entry.Time.Month == date.Month &&
                            entry.Type == TransactionType.PersonalMonthAggregate);
                if (personalTransaction != null)
                {
                    var mentorBonusesAgregate = user.Transactions.Where(
                        entry => entry.Type == TransactionType.MentorBonus
                                 &&
                                 entry.Time >=
                                 new DateTime(date.Year, date.Month, 1)
                                 &&
                                 entry.Time <=
                                 new DateTime(date.Year, date.Month,
                                     DateTime.DaysInMonth(date.Year,
                                         date.Month), 23, 59, 59))
                        .Sum(entry => entry.Bonuses);
                    model.General.Add(
                        new Transaction()
                        {
                            Time = personalTransaction.Time.AddHours(1),
                            Type = TransactionType.MentorBonus,
                            Payee = user,
                            Bonuses = mentorBonusesAgregate,
                            BonusesBalans = personalTransaction.BonusesBalans + mentorBonusesAgregate
                        });
                }
            }

            model.General = model.General.Where(entry => entry.Time > start && entry.Time < end).OrderByDescending(entry => entry.Time).ToList();

            model.Personal =
                user.Transactions.Where(
                    entry =>
                        entry.Type == TransactionType.PersonalSiteBonus ||
                        entry.Type == TransactionType.PersonalBenefitCardBonus)
                    .Where(entry => entry.Time >= start && entry.Time <= end)
                    .OrderByDescending(entry => entry.Time)
                    .ToList();

            model.Referals = user.Transactions.Where(entry => entry.Type == TransactionType.MentorBonus)
                .Where(entry => entry.Time >= start && entry.Time <= end)
                .OrderByDescending(entry => entry.Time)
                .ToList();

            return model;
        }
    }
}