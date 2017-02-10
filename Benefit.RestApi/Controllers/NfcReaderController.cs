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
        [Route("AuthCashier")]
        public IHttpActionResult AuthCashier(string nfc)
        {
            var cashier = db.Personnels.Include(entry => entry.Seller).FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == nfc.ToLower());
            if (cashier != null && cashier.Seller.Id == User.Identity.Name)
            {
                return Ok(new SellerCashierAuthDto()
                {
                    SellerName = cashier.Seller.Name,
                    CashierName = cashier.Name
                });
            }
            return NotFound();
        }

        [HttpGet]
        [Route("AuthUser")]
        public IHttpActionResult AuthUser(string nfc)
        {
            var user = db.Users.FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == nfc.ToLower());
            if (user != null)
            {
                return Ok(new BenefitCardUserAuthDto
                {
                    Name = user.FullName,
                    CardNumber = user.CardNumber
                });
            }
            return NotFound();
        }

        [HttpGet]
        [Route("UserInfo")]
        public IHttpActionResult UserInfo(string nfc)
        {
            var user = db.Users.Include(entry => entry.Orders).FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == nfc.ToLower());
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

        [HttpPost]
        [Route("ProcessPayment")]
        public HttpResponseMessage ProcessPayment(PaymentIngest paymentIngest)
        {
            try
            {
                var cashier =
                    db.Personnels.Include(entry => entry.Seller)
                        .FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == paymentIngest.CashierNfc.ToLower());
                if (cashier == null || cashier.Seller.Id != User.Identity.Name)
                {
                    _logger.Error("cashier not found, nfc: " + paymentIngest.CashierNfc);
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "cashier not found");
                }
                var seller = cashier.Seller;

                var user =
                    db.Users.FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == paymentIngest.UserNfc.ToLower());
                if (user == null)
                {
                    _logger.Error("User not found, nfc: " + paymentIngest.UserNfc);
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found");
                }
                var points = paymentIngest.Sum/SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount];
                var bonuses = paymentIngest.Sum*seller.UserDiscount/100;
                //add finished order with benefit card type
                var orderNumber = db.Orders.Max(entry => (int?) entry.OrderNumber) ?? SettingsService.OrderMinValue;
                var order = new Order()
                {
                    Id = Guid.NewGuid().ToString(),
                    CardNumber = user.CardNumber,
                    OrderNumber = orderNumber + 1,
                    OrderType = OrderType.BenefitCard,
                    PaymentType = PaymentType.Cash,
                    SellerId = seller.Id,
                    SellerName = seller.Name,
                    PersonnelName = cashier.Name,
                    Time = DateTime.UtcNow,
                    Status = OrderStatus.Finished,
                    UserId = user.Id,
                    Description = null,
                    ShippingAddress = null,
                    ShippingCost = 0,
                    ShippingName = null,
                    Sum = paymentIngest.Sum,
                    PointsSum = points,
                    PersonalBonusesSum = bonuses
                };
                db.Orders.Add(order);

                //add transaction with personal bonuses with type benefit card
                var transaction = new Transaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    Bonuses = bonuses,
                    BonusesBalans = user.CurrentBonusAccount + bonuses,
                    OrderId = order.Id,
                    PayeeId = user.Id,
                    Time = DateTime.UtcNow,
                    Type = TransactionType.PersonalBenefitCardBonus
                };
                db.Transactions.Add(transaction);

                //add points to seller account
                seller.PointsAccount += points;
                db.Entry(seller).State = EntityState.Modified;

                //add points and bonuses to personal user account
                user.PointsAccount += points;
                user.CurrentBonusAccount += bonuses;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChangesAsync();

                return Request.CreateResponse(HttpStatusCode.OK,
                    new PaymentResultDto()
                    {
                        BonusesAcquired = bonuses,
                        BonusesAccount = user.BonusAccount
                    });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "BenefitCard Payment exception:");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }
    }
}
