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
            var orders = db.Orders.Where(entry => entry.Status == OrderStatus.Finished);

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

            orders = orders.Where(entry => entry.Time >= startDate && entry.Time <= endDate);

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
                        orders.Where(entry => entry.SellerId == seller.Id && entry.OrderType == OrderType.BenefitSite)
                            .Select(entry => entry.Sum)
                            .DefaultIfEmpty(0)
                            .Sum(),
                    CardsTurnover =
                        orders.Where(entry => entry.SellerId == seller.Id && entry.OrderType == OrderType.BenefitCard)
                            .Select(entry => entry.Sum)
                            .DefaultIfEmpty(0)
                            .Sum(),
                    BonusesTurnover =
                        orders.Where(entry => entry.SellerId == seller.Id && entry.PaymentType == PaymentType.Bonuses)
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