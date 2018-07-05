using System.Linq;
using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using System;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using System.Data.Entity;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class StatisticsController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        //
        // GET: /Admin/Statistics/
        public ActionResult SellerTurnover(SellerTurnoverViewModel sellerTurnover)
        {
            var model = new SellerTurnoverViewModel();
            var orders = db.Orders
                .Include(entry => entry.OrderStatusStamps)
                .Include(entry => entry.Transactions)
                .Where(entry => entry.Status == OrderStatus.Finished);

            var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endDate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(sellerTurnover.DateRange))
            {
                var dateRangeValues = sellerTurnover.DateRange.Split('-');
                startDate = DateTime.Parse(dateRangeValues.First());
                endDate = DateTime.Parse(dateRangeValues.Last()).AddTicks(-1).AddDays(1);
            }
            model.DateRange = string.Format("{0} - {1}", startDate.ToShortDateString(),
                endDate.ToShortDateString());

            var sellers = db.Sellers.Include(entry => entry.Addresses).Where(entry => entry.TotalDiscount <= 100);
            if (!string.IsNullOrEmpty(sellerTurnover.RegionId))
            {
                var intRegionId = int.Parse(sellerTurnover.RegionId);
                sellers = sellers.Where(entry => entry.Addresses.Select(addr => addr.RegionId).Contains(intRegionId));
            }
            foreach (var seller in sellers)
            {
                var turnover = new SellerTurnover()
                {
                    SellerName = seller.Name,
                    SellerTotalDiscount = seller.TotalDiscount,
                    SiteTurnover =
                        orders.Where(
                            entry =>
                                entry.SellerId == seller.Id && entry.OrderType == OrderType.BenefitSite &&
                                entry.OrderStatusStamps.FirstOrDefault(
                                    stamp => stamp.OrderStatus == OrderStatus.Finished).Time >= startDate &&
                                entry.OrderStatusStamps.FirstOrDefault(
                                    stamp => stamp.OrderStatus == OrderStatus.Finished).Time <= endDate)
                            .Select(entry => entry.Sum)
                            .DefaultIfEmpty(0)
                            .Sum(),
                    SiteTurnoverWithoutBonuses =
                        orders.Where(
                            entry =>
                                entry.SellerId == seller.Id && entry.OrderType == OrderType.BenefitSite &&
                                entry.PaymentType != PaymentType.Bonuses &&
                                entry.OrderStatusStamps.FirstOrDefault(
                                    stamp => stamp.OrderStatus == OrderStatus.Finished).Time >= startDate &&
                                entry.OrderStatusStamps.FirstOrDefault(
                                    stamp => stamp.OrderStatus == OrderStatus.Finished).Time <= endDate)
                            .Select(entry => entry.Sum)
                            .DefaultIfEmpty(0)
                            .Sum(),
                    CardsTurnover =
                        orders.Where(
                            entry =>
                                entry.SellerId == seller.Id && entry.OrderType == OrderType.BenefitCard &&
                                entry.Time > startDate && entry.Time < endDate)
                            .Select(entry => entry.Sum)
                            .DefaultIfEmpty(0)
                            .Sum(),
                    BonusesOnlineTurnover = Math.Abs(
                        orders.Where(
                            entry =>
                                entry.SellerId == seller.Id 
                                && entry.PaymentType == PaymentType.Bonuses 
                                && entry.OrderType == OrderType.BenefitSite 
                                && entry.OrderStatusStamps.FirstOrDefault(
                                    stamp => stamp.OrderStatus == OrderStatus.Finished).Time >= startDate &&
                                entry.OrderStatusStamps.FirstOrDefault(
                                    stamp => stamp.OrderStatus == OrderStatus.Finished).Time <= endDate)
                            .SelectMany(entry => entry.Transactions.Where(tr=>tr.Type == TransactionType.BonusesOrderPayment || tr.Type == TransactionType.OrderRefund).Select(tr=>tr.Bonuses))
                            .DefaultIfEmpty(0)
                            .Sum()),
                    BonusesOffineTurnover =
                        orders.Where(
                            entry =>
                                entry.SellerId == seller.Id 
                                && entry.PaymentType == PaymentType.Bonuses
                                && entry.OrderType == OrderType.BenefitCard
                                && entry.Time > startDate && entry.Time < endDate)
                            .Select(entry => entry.Sum)
                            .DefaultIfEmpty(0)
                            .Sum()
                };
                turnover.BCcomission = (turnover.SiteTurnover + turnover.CardsTurnover) * seller.TotalDiscount / 100;
                model.SellerTurnovers.Add(turnover);
            }
            model.SellerTurnovers = model.SellerTurnovers.OrderByDescending(entry => entry.SiteTurnover + entry.CardsTurnover).ToList();

            return View(model);
        }
    }
}