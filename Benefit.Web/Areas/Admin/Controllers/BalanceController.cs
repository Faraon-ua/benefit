using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using System.Linq;
using System.Web.Mvc;
using Benefit.Services;
using Newtonsoft.Json;
using Benefit.Common.Extensions;
using Benefit.DataTransfer.ViewModels;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Data.Entity;
using Benefit.Common.Constants;
using System.IO;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class BalanceController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult GetBankForm(int invoiceNumber)
        {
            var invoice = db.PaymentBills.Include(entry => entry.Seller).FirstOrDefault(entry => entry.InnerNumber == invoiceNumber);
            if (invoice == null)
            {
                throw new HttpException(404, "Not found");
            }
            return View(invoice);
        }
        public ActionResult RemoveSellerReport(string id)
        {
            var report = db.SellerReports.Find(id);
            if (report != null)
            {
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var reportPath = Path.Combine(originalDirectory, "Reports", report.SellerId,report.FileUrl);
                if (System.IO.File.Exists(reportPath))
                {
                    System.IO.File.Delete(reportPath);
                }
                db.SellerReports.Remove(report);
                db.SaveChanges();
                return new HttpStatusCodeResult(200);
            }
            return new HttpStatusCodeResult(404);
        }
        public ActionResult Index(BalanceViewModel model, int page = 0)
        {
            model = model ?? new BalanceViewModel();
            model.Seller = db.Sellers.Find(Seller.CurrentAuthorizedSellerId);
            var sellerTransactions = db.SellerTransactions
                .Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId);
            ViewBag.NotPaidCount = db.PaymentBills.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId && entry.Status == BillStatus.AwaitingPayment).Count();

            if (!string.IsNullOrEmpty(model.OrderNumber))
            {
                sellerTransactions = sellerTransactions.Where(entry => entry.OrderNumber.ToString() == model.OrderNumber);
            }
            if (!string.IsNullOrEmpty(model.ProductSKU))
            {
                sellerTransactions = sellerTransactions.Where(entry => entry.ProductSKU.ToString() == model.ProductSKU);
            }
            if (model.TransactionType.HasValue)
            {
                sellerTransactions = sellerTransactions.Where(entry => entry.Type == model.TransactionType.Value);
            }
            if (!string.IsNullOrEmpty(model.DateRange))
            {
                var dateRangeValues = model.DateRange.Split('-');
                var startDate = DateTime.Parse(dateRangeValues.First());
                var endDate = DateTime.Parse(dateRangeValues.Last()).AddTicks(-1).AddDays(1);
                sellerTransactions = sellerTransactions.Where(entry => entry.Time >= startDate && entry.Time <= endDate);
            }
            var transactionsTotal = sellerTransactions.Count();
            model.SellerTransactions = new PaginatedList<SellerTransaction>
            {
                Items = sellerTransactions.OrderByDescending(entry => entry.Time)
                    .Skip(ListConstants.AdminTakePerPage * page)
                    .Take(ListConstants.AdminTakePerPage)
                    .ToList(),
                Pages = transactionsTotal / ListConstants.AdminTakePerPage + 1,
                ActivePage = page
            };
            model.SellerReports = db.SellerReports.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId).OrderBy(entry => entry.Year).ThenBy(entry=>entry.Month).ToList();
            return View(model);
        }
        // GET: Admin/Balance
        public ActionResult Invoices()
        {
            var invoices = db.PaymentBills.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId).ToList();
            return PartialView("_Invoices", invoices);
        }

        public ActionResult GetLiqpayForm(double amount, string description, string order_id)
        {
            var liqpayParams = new
            {
                version = 3,
                public_key = SettingsService.Liqpay.PublicKey,
                action = "pay",
                amount,
                currency = "UAH",
                description,
                order_id
            };
            var jsonString = JsonConvert.SerializeObject(liqpayParams);
            var base64String = Encoding.UTF8.EncodeBase64(jsonString);
            var signatureStr = string.Format("{0}{1}{2}", SettingsService.Liqpay.PrivateKey, base64String, SettingsService.Liqpay.PrivateKey);
            var signature = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(signatureStr)));
            var model = new LiqpayViewModel()
            {
                Base64Data = base64String,
                Signature = signature
            };
            return PartialView("_LiqpayForm", model);
        }
    }
}