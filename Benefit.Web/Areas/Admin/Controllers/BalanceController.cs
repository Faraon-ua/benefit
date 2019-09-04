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

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class BalanceController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.NotPaidCount = db.PaymentBills.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId && entry.Status == BillStatus.AwaitingPayment).Count();
            return View();
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