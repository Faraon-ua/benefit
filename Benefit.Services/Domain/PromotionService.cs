using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Services.Domain
{
    public class PromotionService
    {
        private readonly TransactionsService _transactionsService = new TransactionsService();

        public void ProcessPromotions(Order order)
        {
            ProcessBonusesPromotions(order);
        }

        private void ProcessBonusesPromotions(Order order)
        {
            using (var db = new ApplicationDbContext())
            {
                var orderProductIds = order.OrderProducts.Select(entry => entry.ProductId).ToList();

                foreach (var productId in orderProductIds)
                {
                    var localTime = DateTime.UtcNow.ToLocalTime();
                    var promotions =
                        db.Promotions.Where(
                            entry =>
                                entry.IsActive && entry.Start < localTime &&
                                entry.End > localTime && entry.IsBonusDiscount &&
                                entry.SellerId == order.SellerId && entry.ProductId == productId).ToList();
                    if (!promotions.Any()) return;

                    var user =
                          db.Users.Include(entry => entry.Partners)
                              .FirstOrDefault(entry => entry.Id == order.UserId);
                    for (var i = 0; i < promotions.Max(entry => entry.Level) + 1; i++)
                    {
                        if (user == null) continue;
                        //lvl 0
                        //add accomplishment reward
                        var promotion = promotions.FirstOrDefault(entry => entry.Level == i);
                        if (i > 0)
                        {
                            var prevPromotion = promotions.FirstOrDefault(entry => entry.Level == i - 1);
                            if (prevPromotion == null) return;
                            var partnerIds = user.Partners.Select(p => p.Id).ToList();
                            var partnerAccomplishments =
                                db.PromotionAccomplishments.Where(
                                    entry =>
                                        entry.PromotionId == prevPromotion.Id &&
                                        partnerIds.Contains(entry.UserId)).ToList();

                            if (partnerAccomplishments.Count() >= promotion.DiscountFrom.Value)
                            {
                                var userAccomplishment =
                                    db.PromotionAccomplishments.FirstOrDefault(
                                        entry => entry.PromotionId == promotion.Id && entry.UserId == user.Id) ??
                                    new PromotionAccomplishment()
                                    {
                                        PromotionId = promotion.Id,
                                        UserId = user.Id,
                                        AccomplishmentsNumber = 0
                                    };

                                var totalpartnerAccomplishments =
                                    partnerAccomplishments.Sum(entry => entry.AccomplishmentsNumber);

                                var accomplishmentsNumber = (int)(totalpartnerAccomplishments / promotion.DiscountFrom) -
                                                            userAccomplishment.AccomplishmentsNumber;
                                if (accomplishmentsNumber > 0)
                                {
                                    userAccomplishment.AccomplishmentsNumber += accomplishmentsNumber;

                                    if (!db.PromotionAccomplishments.Any(
                                        entry => entry.PromotionId == promotion.Id && entry.UserId == user.Id))
                                    {
                                        db.PromotionAccomplishments.Add(userAccomplishment);
                                    }
                                    else
                                    {
                                        db.Entry(userAccomplishment).State = EntityState.Modified;
                                    }
                                    for (var j = 0; j < accomplishmentsNumber; j++)
                                    {
                                        _transactionsService.AddPromotionBonusesPayment(user.Id, promotion, db);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var accomplishmentsNumber = (int)order.OrderProducts.First(entry => entry.ProductId == productId).Amount;
                            for (var j = 0; j < accomplishmentsNumber; j++)
                            {
                                _transactionsService.AddPromotionBonusesPayment(user.Id, promotion, db);
                            }

                            //add accomplishment stamp
                            var promotionAccomplishment = db.PromotionAccomplishments.FirstOrDefault(
                                entry => entry.UserId == user.Id && entry.PromotionId == promotion.Id);
                            if (promotionAccomplishment != null)
                            {
                                promotionAccomplishment.AccomplishmentsNumber += accomplishmentsNumber;
                            }
                            else
                            {
                                promotionAccomplishment = new PromotionAccomplishment()
                                {
                                    PromotionId = promotion.Id,
                                    UserId = user.Id,
                                    AccomplishmentsNumber = accomplishmentsNumber
                                };
                                db.PromotionAccomplishments.Add(promotionAccomplishment);
                            }
                        }
                        db.SaveChanges();
                        user = db.Users.Include(entry => entry.Partners).FirstOrDefault(entry => entry.Id == user.ReferalId);
                    }
                }
            }
        }
    }
}
