using System;
using System.Linq;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Services.Domain
{
    public class PromotionService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        TransactionsService transactionsService = new TransactionsService();
        public void ProcessPromotions(Order order)
        {
            ProcessBonusesPromotions(order);
        }

        private void ProcessBonusesPromotions(Order order)
        {
            var orderProductIds = order.OrderProducts.Select(entry => entry.ProductId).ToList();
            var promotions =
                db.Promotions.Where(
                    entry =>
                        entry.IsActive && entry.Start < DateTime.UtcNow.ToLocalTime() &&
                        entry.End > DateTime.UtcNow.ToLocalTime() && !entry.IsInstantDiscount &&
                        (entry.SellerId == order.SellerId || orderProductIds.Contains(entry.ProductId)));

            foreach (var promotion in promotions)
            {
                //lvl 0
                transactionsService.AddPromotionBonusesPayment(order.UserId, promotion);
                //lvl 0 and 1
            }
        }
    }
}
