﻿using System;
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
        private ApplicationDbContext db = new ApplicationDbContext();

        public void AddCustomBonusesPayment(string userId, double sum, string comment, TransactionType transactionType = TransactionType.Custom)
        {
            var user = db.Users.Find(userId);
            var transaction = new Transaction()
            {
                Id = Guid.NewGuid().ToString(),
                Bonuses = sum,
                BonusesBalans = user.BonusAccount + sum,
                Description = comment,
                PayeeId = userId,
                Time = DateTime.UtcNow,
                Type = transactionType
            };
            db.Transactions.Add(transaction);
            user.BonusAccount = transaction.BonusesBalans;
            user.TotalBonusAccount += sum;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void AddPromotionBonusesPayment(string userId, Promotion promotion, ApplicationDbContext transactionDb)
        {
            var user = transactionDb.Users.Find(userId);
            if (promotion.IsMentorPromotion)
            {
                user = transactionDb.Users.Find(user.ReferalId);
            }
            if (!user.IsCardVerified) return;
            var bonuses = promotion.DiscountValue.GetValueOrDefault(0);
            var transaction = new Transaction()
            {
                Id = Guid.NewGuid().ToString(),
                Bonuses = bonuses,
                BonusesBalans = (promotion.IsCurrentAccountBonusPromotion ? user.CurrentBonusAccount : user.BonusAccount) + bonuses,
                Description = promotion.Name,
                PayeeId = user.Id,
                Time = DateTime.UtcNow,
                Type = TransactionType.Promotion
            };
            transactionDb.Transactions.Add(transaction);
            if (promotion.IsCurrentAccountBonusPromotion)
            {
                user.CurrentBonusAccount = transaction.BonusesBalans;
            }
            else
            {
                user.BonusAccount = transaction.BonusesBalans;
                user.TotalBonusAccount += bonuses;    
            }
            transactionDb.Entry(user).State = EntityState.Modified;
        }

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

        public void AddOrderFinishedTransaction(Order order, ApplicationDbContext transactionDb)
        {
            var user = transactionDb.Users.Find(order.UserId);
            var seller = transactionDb.Sellers.Find(order.SellerId);
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
            seller.PointsAccount += order.PointsSum;

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
            transactionDb.Entry(seller).State = EntityState.Modified;
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
                        entry.Type == TransactionType.Custom ||
                        entry.Type == TransactionType.Comission ||
                        entry.Type == TransactionType.Promotion ||
                        entry.Type == TransactionType.BenefitCardBonusesPayment ||
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
                var lastMentorTransaction =
                    user.Transactions.Where(
                        entry =>
                            entry.Time.Year == date.Year && entry.Time.Month == date.Month &&
                            entry.Type == TransactionType.MentorBonus)
                        .OrderByDescending(entry => entry.BonusesBalans)
                        .FirstOrDefault();
                if (lastMentorTransaction != null)
                {
                    var mentorBonusesAgregate = user.Transactions.Where(
                        entry => (entry.Type == TransactionType.MentorBonus || entry.Type == TransactionType.BusinessLevel)
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
                            Time = lastMentorTransaction.Time,
                            Type = TransactionType.MentorBonus,
                            Payee = user,
                            Bonuses = mentorBonusesAgregate,
                            BonusesBalans = lastMentorTransaction.BonusesBalans
                        });
                }
            }

            model.General =
                model.General.Where(entry => entry.Time > start && entry.Time < end)
                    .OrderByDescending(entry => entry.Time)
                    .ToList();

            model.Personal =
                user.Transactions.Where(
                    entry =>
                        entry.Type == TransactionType.Promotion ||
                        entry.Type == TransactionType.PersonalSiteBonus ||
                        entry.Type == TransactionType.PersonalBenefitCardBonus)
                    .Where(entry => entry.Time >= start && entry.Time <= end)
                    .OrderByDescending(entry => entry.Time)
                    .ToList();

            model.Referals = user.Transactions.Where(entry => entry.Type == TransactionType.MentorBonus || entry.Type == TransactionType.BusinessLevel)
                .Where(entry => entry.Time >= start && entry.Time <= end)
                .OrderByDescending(entry => entry.Time)
                .ToList();

            return model;
        }
    }
}