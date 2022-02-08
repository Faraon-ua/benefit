using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Benefit.CardReader.DataTransfer;
using Benefit.CardReader.DataTransfer.Dto;
using Benefit.CardReader.DataTransfer.Ingest;
using Benefit.Domain.DataAccess;
using System.Data.Entity;
using System.Threading;
using Benefit.Domain.Models;
using NLog;
using Benefit.Services;

namespace Benefit.RestApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/NfcReader/v1")]
    public class NfcReaderController : ApiController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        static Logger _logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("GetSellerName")]
        public IHttpActionResult GetSellerName(string licenseKey)
        {
            var seller = db.Sellers.FirstOrDefault(entry => entry.TerminalLicense.ToLower() == licenseKey.ToLower());
            if (seller == null) return NotFound();
            return Ok(seller.Name);
        }

        [HttpGet]
        [Route("AuthCashier")]
        public IHttpActionResult AuthCashier(string nfc)
        {
            var cashier = db.Personnels.Include(entry => entry.Seller).FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == nfc.ToLower());
            if (cashier != null && cashier.Seller.Id == User.Identity.Name)
            {
                return Ok(new SellerCashierAuthDto()
                {
                    SellerName = cashier.Seller.Name,
                    CashierName = cashier.Name,
                    ShowBill = cashier.Seller.TerminalBillEnabled,
                    ShowBonusesPayment = cashier.Seller.TerminalBonusesPaymentActive,
                    ShowKeyboard = cashier.Seller.TerminalKeyboardEnabled,
                    LogRequests = cashier.Seller.LogRequests
                });
            }
            return NotFound();
        }

        [HttpGet]
        [Route("AuthUser")]
        public IHttpActionResult AuthUser(string nfc)
        {
            var user = db.Users.FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == nfc.ToLower());
            if (user == null)
            {
                var benefitCard =
                      db.BenefitCards.Include(entry => entry.User).FirstOrDefault(
                          entry => entry.NfcCode.ToLower() == nfc.ToLower());
                if (benefitCard != null)
                {
                    user = benefitCard.User;
                }
            }
            if (user != null)
            {
                return Ok(new BenefitCardUserAuthDto
                {
                    Name = user.FullName,
                    AvailableBonuses = user.BonusAccount.ToString("F")
                });
            }
            return NotFound();
        }

        [HttpGet]
        [Route("UserInfo")]
        public IHttpActionResult UserInfo(string nfc)
        {
            var user = db.Users.Include(entry => entry.Orders).FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == nfc.ToLower());
            if (user == null)
            {
                var benefitCard =
                      db.BenefitCards.Include(entry => entry.User.Orders).FirstOrDefault(
                          entry => entry.NfcCode.ToLower() == nfc.ToLower());
                if (benefitCard != null)
                {
                    user = benefitCard.User;
                }
            }

            if (user != null)
            {
                var lastCardOrder =
                    user.Orders.Where(entry => entry.OrderType == OrderType.BenefitCard)
                        .OrderByDescending(entry => entry.Time)
                        .FirstOrDefault();
                return Ok(new BenefitCardUserDto()
                {
                    Name = user.FullName,
                    CardNumber = user.CardNumber,
                    BonusesAccount = user.BonusAccount,
                    LastCardOrderInfo = new CardOrderDto()
                    {
                        SellerName = lastCardOrder.SellerName,
                        Sum = lastCardOrder.Sum,
                        Time = lastCardOrder.Time.ToLocalTime()
                    }
                });
            }
            return NotFound();
        }

        //[HttpPost]
        //[Route("ProcessPayment")]
        //public HttpResponseMessage ProcessPayment(PaymentIngest paymentIngest)
        //{
        //    double sumToPay = 0;
        //    BenefitCard benefitCard = null;
        //    try
        //    {
        //        var cashier =
        //            db.Personnels.Include(entry => entry.Seller)
        //                .FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == paymentIngest.CashierNfc.ToLower());
        //        if (cashier == null || cashier.Seller.Id != User.Identity.Name)
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "cashier not found");
        //        }
        //        var seller = cashier.Seller;

        //        var user =
        //            db.Users.FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == paymentIngest.UserNfc.ToLower());
        //        if (user == null)
        //        {
        //            benefitCard =
        //                db.BenefitCards.Include(entry => entry.User).FirstOrDefault(
        //                    entry => entry.NfcCode.ToLower() == paymentIngest.UserNfc.ToLower());
        //            if (benefitCard == null)
        //            {
        //                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found");
        //            }
        //            user = benefitCard.User;
        //        }
        //        var points = paymentIngest.Sum / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount];
        //        var bonuses = paymentIngest.Sum * seller.UserDiscount / 100;
        //        //add finished order with benefit card type
        //        var orderNumber = db.Orders.Max(entry => (int?)entry.OrderNumber) ?? SettingsService.OrderMinValue;
        //        var order = new Order()
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            CardNumber = benefitCard == null ? user.CardNumber : benefitCard.Id,
        //            OrderNumber = orderNumber + 1,
        //            OrderType = OrderType.BenefitCard,
        //            PaymentType = paymentIngest.ChargeBonuses ? PaymentType.Bonuses : PaymentType.Cash,
        //            SellerId = seller.Id,
        //            SellerName = seller.Name,
        //            PersonnelName = cashier.Name,
        //            Time = DateTime.UtcNow,
        //            Status = OrderStatus.Finished,
        //            UserId = user.Id,
        //            Description = paymentIngest.BillNumber,
        //            ShippingAddress = "kassa",
        //            ShippingCost = 0,
        //            ShippingName = null,
        //            Sum = paymentIngest.Sum,
        //            PointsSum = points,
        //            PersonalBonusesSum = bonuses
        //        };
        //        db.Orders.Add(order);

        //        //add transaction with personal bonuses with type benefit card
        //        var transaction = new Transaction()
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            Bonuses = bonuses,
        //            BonusesBalans = user.HangingBonusAccount + bonuses,
        //            OrderId = order.Id,
        //            PayeeId = user.Id,
        //            Time = DateTime.UtcNow,
        //            Type = TransactionType.CashbackBonus
        //        };
        //        db.Transactions.Add(transaction);

        //        //add points to seller account
        //        seller.PointsAccount += points;
        //        db.Entry(seller).State = EntityState.Modified;

        //        //add points and bonuses to personal user account
        //        user.PointsAccount += points;
        //        user.CurrentBonusAccount += bonuses;

        //        //charge bonuses
        //        if (paymentIngest.ChargeBonuses)
        //        {
        //            sumToPay = order.Sum - order.SellerDiscount.GetValueOrDefault(0);
        //            if (user.BonusAccount < sumToPay)
        //            {
        //                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Недостатньо бонусів на рахунку");
        //            }
        //            //add transaction for personal purchase
        //            var bonusesPaymentTransaction = new Transaction()
        //            {
        //                Id = Guid.NewGuid().ToString(),
        //                Bonuses = -(sumToPay),
        //                BonusesBalans = user.BonusAccount - sumToPay,
        //                OrderId = order.Id,
        //                PayeeId = user.Id,
        //                Time = DateTime.UtcNow,
        //                Type = TransactionType.BenefitCardBonusesPayment,
        //                Description = seller.Name
        //            };
        //            user.BonusAccount = bonusesPaymentTransaction.BonusesBalans;
        //            db.Transactions.Add(bonusesPaymentTransaction);
        //        }

        //        db.Entry(user).State = EntityState.Modified;
        //        db.SaveChangesAsync();

        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new PaymentResultDto()
        //            {
        //                BonusesCharged = sumToPay,
        //                BonusesAcquired = bonuses,
        //                BonusesAccount = user.BonusAccount
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex, "BenefitCard Payment exception:");
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
        //    }
        //}

        [Route("PingOnline")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult PingOnline(string licenseKey)
        {
            Seller seller = null;
            if (licenseKey != null)
            {
                seller = db.Sellers.FirstOrDefault(entry => entry.TerminalLicense == licenseKey);
            }
            if (seller == null)
            {
                seller = db.Sellers.Find(User.Identity.Name);
            }
            if (seller == null)
                return NotFound();
            seller.TerminalLastOnline = DateTime.UtcNow;
            db.Entry(seller).State = EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
    }
}
