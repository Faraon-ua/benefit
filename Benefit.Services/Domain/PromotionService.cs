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
        private ApplicationDbContext _transactionDb;

        public PromotionService(ApplicationDbContext transactionDb)
        {
            this._transactionDb = transactionDb;
        }

        public void ProcessPromotions(Order order)
        {
            ProcessBonusesPromotions(order);
        }

        private void ProcessBonusesPromotions(Order order)
        {
            var orderProductIds = order.OrderProducts.Select(entry => entry.ProductId).ToList();

            foreach (var productId in orderProductIds)
            {
                var localTime = DateTime.UtcNow.ToLocalTime();
                var promotions =
                    _transactionDb.Promotions.Where(
                        entry =>
                            entry.IsActive && entry.Start < localTime &&
                            entry.End > localTime && entry.IsBonusDiscount &&
                            entry.SellerId == order.SellerId && entry.ProductId == productId).ToList();
                if (!promotions.Any()) return;

                var user =
                      _transactionDb.Users.Include(entry => entry.Partners)
                          .FirstOrDefault(entry => entry.Id == order.UserId);
                for (var i = 0; i < promotions.Max(entry => entry.Level) + 1; i++)
                {
                    if(user == null) continue;
                    //lvl 0
                    //add accomplishment reward
                    var promotion = promotions.FirstOrDefault(entry => entry.Level == i);
                    if (i > 0)
                    {
                        var prevPromotion = promotions.FirstOrDefault(entry => entry.Level == i - 1);
                        if (prevPromotion == null) return;
                        var partnerIds = user.Partners.Select(p => p.Id).ToList();
                        var partnerAccomplishments =
                            _transactionDb.PromotionAccomplishments.Where(
                                entry =>
                                    entry.PromotionId == prevPromotion.Id &&
                                    partnerIds.Contains(entry.UserId)).ToList();

                        if (partnerAccomplishments.Count() >= promotion.DiscountFrom.Value)
                        {
                            var userAccomplishment =
                                _transactionDb.PromotionAccomplishments.FirstOrDefault(
                                    entry => entry.PromotionId == promotion.Id && entry.UserId == user.Id) ??
                                new PromotionAccomplishment()
                                {
                                    PromotionId = promotion.Id,
                                    UserId = user.Id,
                                    AccomplishmentsNumber = 0
                                };

                            var totalpartnerAccomplishments =
                                partnerAccomplishments.Sum(entry => entry.AccomplishmentsNumber);

                            var accomplishmentsNumber = (int) (totalpartnerAccomplishments/promotion.DiscountFrom) -
                                                        userAccomplishment.AccomplishmentsNumber;
                            if (accomplishmentsNumber > 0)
                            {
                                userAccomplishment.AccomplishmentsNumber+=accomplishmentsNumber;

                                if (!_transactionDb.PromotionAccomplishments.Any(
                                    entry => entry.PromotionId == promotion.Id && entry.UserId == user.Id))
                                {
                                    _transactionDb.PromotionAccomplishments.Add(userAccomplishment);
                                }
                                else
                                {
                                    _transactionDb.Entry(userAccomplishment).State = EntityState.Modified;
                                }
                                for (var j = 0; j < accomplishmentsNumber; j++)
                                {
                                    _transactionsService.AddPromotionBonusesPayment(user.Id, promotion, _transactionDb);
                                }
                            }
                        }
                    }
                    else
                    {
                        var accomplishmentsNumber = (int)order.OrderProducts.First(entry => entry.ProductId == productId).Amount;
                        for (var j = 0; j < accomplishmentsNumber; j++)
                        {
                            _transactionsService.AddPromotionBonusesPayment(user.Id, promotion, _transactionDb);
                        }

                        //add accomplishment stamp
                        var promotionAccomplishment = _transactionDb.PromotionAccomplishments.FirstOrDefault(
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
                            _transactionDb.PromotionAccomplishments.Add(promotionAccomplishment);
                        }
                    }
                    _transactionDb.SaveChanges();
                    user = _transactionDb.Users.Include(entry => entry.Partners).FirstOrDefault(entry => entry.Id == user.ReferalId);
                }
            }
        }
    }
}
