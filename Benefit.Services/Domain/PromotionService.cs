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
                var user =
                      _transactionDb.Users.Include(entry => entry.Partners)
                          .FirstOrDefault(entry => entry.Id == order.UserId);
                for (var i = 0; i < promotions.Max(entry => entry.Level) + 1; i++)
                {
                    //lvl 0
                    //add accomplishment reward
                    var promotion = promotions.FirstOrDefault(entry => entry.Level == i);
                    if (i > 0)
                    {
                        var prevPromotion = promotions.FirstOrDefault(entry => entry.Level == i - 1);
                        if (prevPromotion == null) return;
                        var partnerAccomplishments =
                            _transactionDb.PromotionAccomplishments.Where(
                                entry =>
                                    entry.PromotionId == prevPromotion.Id &&
                                    user.Partners.Select(p => p.Id).Contains(entry.UserId));

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

                            if (((int)(totalpartnerAccomplishments / promotion.DiscountFrom) -
                                 userAccomplishment.AccomplishmentsNumber) > 0)
                            {
                                userAccomplishment.AccomplishmentsNumber++;

                                if (!_transactionDb.PromotionAccomplishments.Any(
                                    entry => entry.PromotionId == promotion.Id && entry.UserId == user.Id))
                                {
                                    _transactionDb.PromotionAccomplishments.Add(userAccomplishment);
                                }
                                else
                                {
                                    _transactionDb.Entry(userAccomplishment).State = EntityState.Modified;
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

        //private KeyValuePair<int, int> CheckUserForPromotionCompletement(ApplicationUser user, List<Promotion> promotions, int level)
        //{
        //    var promotion = promotions.FirstOrDefault(entry => entry.Level == level);
        //    if (promotion == null) return new KeyValuePair<int, int>(0, 0);
        //    var orders = _transactionDb.Orders.Include(entry => entry.OrderProducts)
        //        .Count(entry => entry.UserId == user.Id
        //                        &&
        //                        entry.OrderProducts
        //                            .Select(
        //                                pr =>
        //                                    pr.ProductId)
        //                            .Contains(
        //                                promotion
        //                                    .ProductId)
        //                        &&
        //                        entry.Time >
        //                        promotion.Start.Value &&
        //                        entry.Time <
        //                        promotion.End.Value &&
        //                        entry.Status ==
        //                        OrderStatus.Finished);

        //    switch (promotion.Level)
        //    {
        //        case 0:
        //            return orders;
        //        case 1:
        //            return user.Partners
        //                .Sum(entry => CheckUserForPromotionCompletement(entry, promotions, 0));
        //        case 2:
        //            return user.Partners
        //                .Sum(entry => CheckUserForPromotionCompletement(entry, promotions, 1));
        //    }
        //    return 0;
        //}
    }
}
