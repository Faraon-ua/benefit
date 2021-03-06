﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Benefit.Common.Helpers;
using Benefit.DataTransfer.ApiDto.OldApi;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;

namespace Benefit.RestApi.Controllers
{
    public class OldApiController : ApiController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private const string ApiVersion = "V2";

        [Route(ApiVersion + "/ProgFetchUserData" + ApiVersion + ".php")]
        //        [Route("ProgFetchUserData.php")]
        public SellerAuthDto SellerLogin(SellerAuthIngest auth)
        {
            var seller = GetSellerByUsernameAndPassword(auth.username, auth.password);
            SellerAuthDto response = null;
            if (seller == null)
            {
                response = new SellerAuthDto()
                {
                    result = Enumerations.GetEnumDescription(SellerAuthResult.Error),
                    name = Enumerations.GetEnumDescription(SellerAuthResult.Error),
                    phone = Enumerations.GetEnumDescription(SellerAuthResult.Error),
                    number_check = 0,
                    card = Enumerations.GetEnumDescription(SellerAuthResult.Error),
                    paybals = 0,
                    paybonus = 0,
                    bonus = 0,
                    typeuser = "partner"
                };
            }
            else
            {
                response = new SellerAuthDto()
                {
                    result = Enumerations.GetEnumDescription(SellerAuthResult.Success),
                    name = seller.Name,
                    phone = "",
                    number_check = seller.TerminalBillEnabled ? 1 : 0,
                    card = string.Empty,
                    paybals = 0,
                    paybonus = 0,
                    bonus = 0,
                    typeuser = "partner"
                };
            }
            return response;
        }

        [Route(ApiVersion + "/ProgUserKassir" + ApiVersion + ".php")]
        //        [Route("ProgUserKassir.php")]
        public PartnerAuthDto KassirLogin(PartnerAuthIngest authIngest)
        {
            var dto = new PartnerAuthDto();
            var seller = GetSellerByUsernameAndPassword(authIngest.username, authIngest.password);
            //if no seller - return error
            if (seller == null)
            {
                dto.result = "error";
                return dto;
            }

            //if cardKassir is emty - then it's kassir auth request
            if (string.IsNullOrEmpty(authIngest.cardKassir))
            {
                var kassir = db.Personnels.FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == authIngest.cardId.ToLower());
                //if no kassir - return no card
                if (kassir == null)
                {
                    dto.result = "block kassir";
                }
                else
                {
                    //if kassir is not in seller
                    if (kassir.SellerId == seller.Id)
                    {
                        dto.result = "success";
                        dto.cardKassir = kassir.NFCCardNumber;
                        dto.fioKassir = kassir.Name;
                    }
                    else
                    {
                        dto.result = "block kassir";
                    }
                }
            }
            //if cardkassir is not empty - it's user auth request
            else
            {
                //if the same kassir - log off him
                if (authIngest.cardKassir.ToLower() == authIngest.cardId.ToLower())
                {
                    dto.result = "success";
                }
                else
                {
                    //if new kassir - send kassir
                    var kassir = db.Personnels.FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == authIngest.cardId.ToLower());
                    if (kassir != null)
                    {
                        if (kassir.SellerId == seller.Id)
                        {
                            dto.result = "success";
                            dto.cardKassir = kassir.NFCCardNumber;
                            dto.fioKassir = kassir.Name;
                        }
                        else
                        {
                            dto.result = "block kassir";
                        }
                    }
                    //if old kassir - send user
                    else
                    {
                        var user = db.Users.FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == authIngest.cardId.ToLower());
                        kassir =
                            db.Personnels.FirstOrDefault(
                                entry => entry.NFCCardNumber.ToLower() == authIngest.cardKassir.ToLower());
                        if (user != null)
                        {
                            //если пользователь заблокирован
                            if (!user.IsActive)
                            {
                                dto.result = "block client";
                            }
                            else
                            {
                                dto.result = "success client";
                                dto.cardKassir = kassir.NFCCardNumber;
                                dto.fioKassir = kassir.Name;
                                dto.cardClient = user.CardNumber;
                                dto.nameClient = user.FullName;
                            }
                        }
                        else
                        {
                            //проверить Benefit Family
                            var familyUser =
                                db.BenefitCards.Include(entry => entry.User).FirstOrDefault(
                                    entry => entry.NfcCode.ToLower() == authIngest.cardId.ToLower());
                            if (familyUser != null && familyUser.User != null)
                            {
                                dto.result = "success client";
                                dto.cardKassir = kassir.NFCCardNumber;
                                dto.fioKassir = kassir.Name;
                                dto.cardClient = familyUser.Id;
                                dto.nameClient = familyUser.HolderName;
                            }
                            else
                            {
                                //не найден партнер
                                dto.cardKassir = kassir.NFCCardNumber;
                                dto.fioKassir = kassir.Name;
                                dto.result = "no active";
                            }
                        }
                    }
                }
            }
            return dto;
        }

        [Route(ApiVersion + "/ProgFetchUserInfo" + ApiVersion + ".php")]
        //        [Route("ProgFetchUserInfo.php")]
        public UserAuthDto UserInfo(UserAuthIngest userAuth)
        {
            var seller = GetSellerByUsernameAndPassword(userAuth.username, userAuth.password);
            if (seller != null)
            {
                var user = db.Users.FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == userAuth.cardnfc);
                if (user != null)
                {
                    var dto = new UserAuthDto()
                    {
                        typeuser = "client",
                        name = user.FullName,
                        phone = string.Empty,
                        card = user.CardNumber,
                        paybals = 0,
                        paybonus = 0,
                        bonus = 0,
                        charged = (float)user.CurrentBonusAccount,
                        //в обробці
                        balance = (float)user.HangingBonusAccount,
                        //доступно
                        hold = (float)user.BonusAccount
                    };
                    return dto;
                }
                else
                {
                    //check for family user
                    var familyUser =
                        db.BenefitCards.Include(entry => entry.User)
                            .FirstOrDefault(entry => entry.NfcCode.ToLower() == userAuth.cardnfc.ToLower());
                    if (familyUser != null && familyUser.User != null)
                    {
                        var dto = new UserAuthDto()
                        {
                            typeuser = "client",
                            name = familyUser.HolderName,
                            phone = string.Empty,
                            card = familyUser.Id,
                            paybals = 0,
                            paybonus = 0,
                            bonus = 0,
                            charged = (float)familyUser.User.CurrentBonusAccount,
                            //в обробці
                            balance = (float)familyUser.User.HangingBonusAccount,
                            //доступно
                            hold = (float)familyUser.User.BonusAccount
                        };
                        return dto;
                    }
                }
            }
            return null;
        }

        [Route(ApiVersion + "/ProgGetOrders" + ApiVersion + ".php")]
        //        [Route("ProgGetOrders.php")]
        public GetOrdersDto CheckOrder(GetOrdersIngest ordersIngest)
        {
            var seller = GetSellerByUsernameAndPassword(ordersIngest.username, ordersIngest.password);
            if (seller != null && seller.TerminalOrderNotification)
            {
                var newOrders = db.Orders.Count(entry => entry.SellerId == seller.Id && entry.Status == OrderStatus.Created);
                return new GetOrdersDto()
                {
                    order = newOrders
                };
            }
            return null;
        }

        [Route(ApiVersion + "/ProgUserPayment" + ApiVersion + ".php")]
        //        [Route("ProgUserPayment.php")]
        public async Task<PaymentDto> ProcessPayment(PaymentIngest payment)
        {
            ApplicationUser user = null;
            BenefitCard familyUser = null;
            var seller = GetSellerByUsernameAndPassword(payment.username, payment.password);
            if (seller == null)
            {
                return new PaymentDto { result = "error" };
            }
            else
            {
                var kassir =
                    db.Personnels.FirstOrDefault(entry => entry.NFCCardNumber.ToLower() == payment.cardKassir.ToLower());
                if (kassir == null)
                    return new PaymentDto { result = "no active" };
                user = db.Users.FirstOrDefault(entry => entry.CardNumber.ToLower() == payment.paymentcard.ToLower());
                if (user == null)
                {
                    familyUser =
                        db.BenefitCards.Include(entry => entry.User)
                            .FirstOrDefault(entry => entry.Id.ToLower() == payment.paymentcard.ToLower());
                    if (familyUser != null && familyUser.User != null)
                    {
                        user = familyUser.User;
                    }
                    else
                    {
                        return new PaymentDto { result = "no active" };
                    }
                }
                var points = payment.summachek / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount];
                var bonuses = payment.summachek * seller.UserDiscount / 100;
                //add finished order with benefit card type
                var orderNumber = db.Orders.Max(entry => (int?)entry.OrderNumber) ?? SettingsService.OrderMinValue;
                var order = new Order()
                {
                    Id = Guid.NewGuid().ToString(),
                    CardNumber = familyUser == null ? user.CardNumber : familyUser.Id,
                    OrderNumber = orderNumber + 1,
                    OrderType = OrderType.BenefitCard,
                    PaymentType = PaymentType.Cash,
                    SellerId = seller.Id,
                    SellerName = seller.Name,
                    PersonnelName = kassir.Name,
                    Time = DateTime.UtcNow,
                    Status = OrderStatus.Finished,
                    UserId = user.Id,
                    Description = payment.numberchek,
                    ShippingAddress = "kassa",
                    ShippingCost = 0,
                    ShippingName = null,
                    Sum = payment.summachek,
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
                await db.SaveChangesAsync();

                return new PaymentDto()
                {
                    result = "success",
                    //скільки нараховано
                    charged = (float)bonuses,
                    //в обробці
                    balance = (float)user.HangingBonusAccount,
                    //доступно
                    hold = (float)user.BonusAccount
                };
            }
        }

        private Seller GetSellerByUsernameAndPassword(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return null;
            }
            return db.Sellers.FirstOrDefault(
                    entry => entry.TerminalLogin == userName && entry.TerminalPassword == password);
        }
    }
}
