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
        TransactionsService transactionsService = new TransactionsService();
        public void ProcessPromotions(Order order, ApplicationDbContext transactionDb)
        {
            ProcessBonusesPromotions(order, transactionDb);
        }

        private void ProcessBonusesPromotions(Order order, ApplicationDbContext transactionDb)
        {
            var orderProductIds = order.OrderProducts.Select(entry => entry.ProductId).ToList();

            foreach (var productId in orderProductIds)
            {
                var localTime = DateTime.UtcNow.ToLocalTime();
                var promotions =
                    transactionDb.Promotions.Where(
                        entry =>
                            entry.IsActive && entry.Start < localTime &&
                            entry.End > localTime && entry.IsBonusDiscount &&
                            entry.SellerId == order.SellerId && entry.ProductId == productId).ToList();
                foreach (
                    var promotion in promotions.Where(entry => entry.Level == 0))
                {
                    //todo:refactor with loop
                    //lvl 0
                    //add accomplishment reward
                    transactionsService.AddPromotionBonusesPayment(order.UserId, promotion, transactionDb);
                    var promotionAccomplishment = transactionDb.PromotionAccomplishments.FirstOrDefault(
                        entry => entry.UserId == order.UserId && entry.PromotionId == promotion.Id);
                    //add accomplishment stamp
                    if (promotionAccomplishment != null)
                    {
                        promotionAccomplishment.AccomplishmentsNumber++;
                    }
                    else
                    {
                        promotionAccomplishment = new PromotionAccomplishment()
                        {
                            PromotionId = promotion.Id,
                            UserId = order.UserId,
                            AccomplishmentsNumber = 1
                        };
                        transactionDb.PromotionAccomplishments.Add(promotionAccomplishment);
                    }
                    transactionDb.SaveChanges();

                    /* var user =
                        db.Users.Include(entry => entry.Referal.Referal)
                            .FirstOrDefault(entry => entry.Id == order.UserId);
                     //lvl 1

                     promotion = promotions.FirstOrDefault(entry => entry.Level == 1);
                         ;                    if (CheckUserForPromotionCompletement(user.Referal, promotions, 1))
                     {
                         transactionsService.AddPromotionBonusesPayment(order.UserId, promotions.FirstOrDefault(entry => entry.Level == 1));
                     }
                     if (CheckUserForPromotionCompletement(user.Referal.Referal, promotions, 2))
                     {
                         transactionsService.AddPromotionBonusesPayment(order.UserId, promotions.FirstOrDefault(entry => entry.Level == 2));
                     }*/
                }
            }
        }

//        private int CheckUserForPromotionCompletement(ApplicationUser user, List<Promotion> promotions, int level)
//        {
//            var promotion = promotions.FirstOrDefault(entry => entry.Level == level);
//            if (promotion == null) return 0;
//            var orders = db.Orders.Include(entry => entry.OrderProducts)
//                .Count(entry => entry.UserId == user.Id
//                                &&
//                                entry.OrderProducts
//                                    .Select(
//                                        pr =>
//                                            pr.ProductId)
//                                    .Contains(
//                                        promotion
//                                            .ProductId)
//                                &&
//                                entry.Time >
//                                promotion.Start.Value &&
//                                entry.Time <
//                                promotion.End.Value &&
//                                entry.Status ==
//                                OrderStatus.Finished);
//            switch (promotion.Level)
//            {
//                case 0:
//                    return orders;
//                case 1:
//                    return user.Partners
//                        .Sum(entry => CheckUserForPromotionCompletement(entry, promotions, 0));
//                case 2:
//                    return user.Partners
//                        .Sum(entry => CheckUserForPromotionCompletement(entry, promotions, 1));
//            }
//            return 0;
//        }
    }
}
