using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Domain.Models.ModelExtensions;
using Benefit.Domain.Models.Service;

namespace Benefit.Services.Admin
{
    public class ScheduleService
    {
        public void CloseQualificationPeriod()
        {
            using (var db = new ApplicationDbContext())
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        //для всех пользователей, у кого есть бонусы за текущий период
                        db.Users.Where(entry => entry.PointsAccount > 0).ToList().ForEach(entry =>
                        {
                            //бонусы за текущий период - в обработку
                            entry.HangingBonusAccount = entry.CurrentBonusAccount;
                            //обнулить бонусы за текущий период 
                            entry.CurrentBonusAccount = 0;
                            //баллы за текущий период - в обработку
                            entry.HangingPointsAccount = entry.PointsAccount;
                            //обнулить баллы в обработке
                            entry.PointsAccount = 0;
                            db.Entry(entry).State = EntityState.Modified;
                        });

                        db.Sellers.Where(entry => entry.PointsAccount > 0).ToList().ForEach(entry =>
                        {
                            //баллы поставщика за текущий период - в обработку
                            entry.HangingPointsAccount = entry.PointsAccount;
                            //обнулить баллы поставщика в обработке
                            entry.PointsAccount = 0;
                            db.Entry(entry).State = EntityState.Modified;
                        });

                        db.SaveChanges();
                        dbTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        dbTransaction.Rollback();
                    }
                }
            }
        }
        public ProcessBonusesViewModel ProcessBonuses()
        {
            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            var firstDayOfLastMonth = month.AddMonths(-1);
            var lastDayOfLastMonth = month.AddTicks(-1);

            var result = new ProcessBonusesViewModel()
            {
                Partners = new List<PartnerReckon>(),
                VipPartners = new List<PartnerReckon>()
            };
            using (var db = new ApplicationDbContext())
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var promotionSellerIndexes =
                                db.SellerBusinessLevelIndexes.Where(entry => entry.Index > 1).ToList();

                        var allUsers = db.Users.Include(entry => entry.Referal).ToList();

                        var partners =
                            allUsers.Where(entry => entry.HangingPointsAccount > 0 && entry.ReferalId != null).ToList();
                        result.ActiveBuyersCount = partners.Count;
                        //транзакции
                        var partnerReckons = new List<TransactionServiceModel>();

                        //рассчет бонусов за приглашение
                        foreach (var partner in partners)
                        {
                            var currentReferal = partner.Referal;

                            do
                            {
                                //берем реферала, который сделал 500 баллов за предыдущий период и карта верифицирована
                                if (currentReferal.HangingPointsAccount >= SettingsService.RewardsPlan.PointsQualificationAmount && currentReferal.IsCardVerified)
                                {
                                    //если добавили наставнику - прерываем цикл
                                    var totalBalansBeforeRecon = currentReferal.BonusAccount +
                                                      partnerReckons.Where(entry => entry.Transaction.PayeeId == currentReferal.Id)
                                                          .Sum(entry => entry.Transaction.Bonuses);
                                    var promotionPurchasesPoints = 0.0;
                                    var businessLevelBonuses = 0.0;

                                    //if referal has business level that has index more than 1
                                    if (promotionSellerIndexes.Select(entry => entry.BusinessLevel)
                                            .Contains(currentReferal.BusinessLevel))
                                    {
                                        var promotionSellersWithReferalBusinessLevel =
                                            promotionSellerIndexes.Where(
                                                entry => entry.BusinessLevel == currentReferal.BusinessLevel).Select(entry => entry.SellerId).ToList();

                                        promotionPurchasesPoints =
                                            db.Orders.Include(entry => entry.OrderStatusStamps).Where(
                                                entry =>
                                                    entry.UserId == partner.Id &&
                                                    promotionSellersWithReferalBusinessLevel.Contains(entry.SellerId) &&
                                                    entry.Status == OrderStatus.Finished &&
                                                    entry.OrderStatusStamps.FirstOrDefault(stamp => stamp.OrderStatus == OrderStatus.Finished).Time > firstDayOfLastMonth &&
                                                    entry.OrderStatusStamps.FirstOrDefault(stamp => stamp.OrderStatus == OrderStatus.Finished).Time < lastDayOfLastMonth)
                                                .Select(entry => entry.PointsSum)
                                                .DefaultIfEmpty(0)
                                                .Sum();

                                        if (promotionPurchasesPoints > 0)
                                        {
                                            businessLevelBonuses = promotionPurchasesPoints *
                                                                   SettingsService.RewardsPlan
                                                                       .DistributionToPointsPercentageMap[
                                                                           DistributionType.Mentor] / 100;

                                            var businessLevelTransaction = new TransactionServiceModel()
                                            {
                                                Transaction = new Transaction()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    Bonuses = businessLevelBonuses,
                                                    BonusesBalans = totalBalansBeforeRecon + businessLevelBonuses,
                                                    Time = DateTime.UtcNow,
                                                    Type = TransactionType.BusinessLevel,
                                                    PayerId = partner.Id,
                                                    Payer = partner,
                                                    PayeeId = currentReferal.Id,
                                                    Payee = currentReferal
                                                },
                                                PointsAmount = promotionPurchasesPoints
                                            };
                                            partnerReckons.Add(businessLevelTransaction);
                                        }
                                    }

                                    var bonusesForMentor = (partner.HangingPointsAccount + promotionPurchasesPoints) *
                                                   SettingsService.RewardsPlan.DistributionToPointsPercentageMap[
                                                       DistributionType.Mentor] / 100;
                                    var transaction = new TransactionServiceModel()
                                    {
                                        Transaction = new Transaction()
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            Bonuses = bonusesForMentor,
                                            BonusesBalans = totalBalansBeforeRecon + businessLevelBonuses + bonusesForMentor,
                                            Time = DateTime.UtcNow,
                                            Type = TransactionType.MentorBonus,
                                            PayerId = partner.Id,
                                            Payer = partner,
                                            PayeeId = currentReferal.Id,
                                            Payee = currentReferal
                                        },
                                        PointsAmount = partner.HangingPointsAccount
                                    };
                                    partnerReckons.Add(transaction);
                                    break;
                                }
                                else
                                {
                                    currentReferal = currentReferal.Referal;
                                }
                            } while (currentReferal != null);
                        }

                        var disctinctPayees =
                            partnerReckons.Select(entry => entry.Transaction.Payee).Distinct(new ApplicationUserComparer());
                        foreach (var payee in disctinctPayees)
                        {
                            var partnerRecon = new PartnerReckon()
                            {
                                User = payee,
                                BonusesReckoned =
                                    (double)
                                        partnerReckons.Where(entry => entry.Transaction.PayeeId == payee.Id)
                                            .Sum(entry => entry.Transaction.Bonuses),
                                QualifiedPartnersNumber = partnerReckons.Count(entry => entry.PointsAmount >= SettingsService.RewardsPlan.PointsQualificationAmount && entry.Transaction.PayeeId == payee.Id)
                            };

                            payee.BonusAccount += partnerRecon.BonusesReckoned;
                            payee.TotalBonusAccount += partnerRecon.BonusesReckoned;

                            //во вьюшку
                            result.Partners.Add(partnerRecon);
                            db.Entry(payee).State = EntityState.Modified;
                        }

                        /*Рассчет комиссионных вип партнеров*/

                        var totalStructurePointsAmount = allUsers.Sum(entry => entry.HangingPointsAccount);
                        var vipBonusesStack = totalStructurePointsAmount * SettingsService.RewardsPlan.DistributionToPointsPercentageMap[DistributionType.VIP] / 100;

                        //вычислить випов
                        var vipPartnerIds =
                            result.Partners.Where(
                                entry =>
                                    entry.User.HangingPointsAccount >= SettingsService.RewardsPlan.VIPPointsQualificationAmount &&
                                    entry.QualifiedPartnersNumber >= SettingsService.RewardsPlan.VIPQualifiedPartnersNumber).Select(entry => entry.User.Id).ToList();
                        var vipPartners =
                            db.Users.Include(entry => entry.ReferedBenefitCardSellers)
                                .Include(entry => entry.ReferedWebSiteSellers)
                                .Where(entry => vipPartnerIds.Contains(entry.Id))
                                .ToList();

                        var vipPortions = new List<VipPortion>();
                        foreach (var vipPartner in vipPartners)
                        {
                            if (vipPartner.Status.HasValue && vipPartner.Status.Value == Status.Partner)
                            {
                                vipPartner.Status = Status.VIP;
                            }
                            var vipPartnerTotalStructurePointAmount =
                                vipPartner.GetAllStructurePartners(db).Sum(entry => entry.HangingPointsAccount) + vipPartner.HangingPointsAccount; //вместе с ним

                            var vipPortion = new VipPortion()
                            {
                                User = vipPartner,
                                UserStructurePoints = vipPartnerTotalStructurePointAmount,
                                Portions = vipPartnerTotalStructurePointAmount / SettingsService.RewardsPlan.VIPPortionPointsAmount
                            };
                            vipPortions.Add(vipPortion);
                        }

                        var totalVipPortions =
                            vipPortions.Sum(
                                entry =>
                                    (entry.Portions > SettingsService.RewardsPlan.VIPMaxPortions
                                        ? SettingsService.RewardsPlan.VIPMaxPortions
                                        : entry.Portions));

                        var vipPortionBonusRate = vipBonusesStack / totalVipPortions;

                        foreach (var vipPortion in vipPortions)
                        {
                            var bonusesRecon = (vipPortion.Portions > SettingsService.RewardsPlan.VIPMaxPortions
                                ? SettingsService.RewardsPlan.VIPMaxPortions
                                : vipPortion.Portions) * vipPortionBonusRate;
                            var transaction = new TransactionServiceModel()
                            {
                                Transaction = new Transaction()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Bonuses = bonusesRecon,
                                    BonusesBalans = vipPortion.User.BonusAccount + bonusesRecon,
                                    Time = DateTime.UtcNow,
                                    Type = TransactionType.VIPBonus,
                                    PayeeId = vipPortion.User.Id,
                                    Payee = vipPortion.User
                                },
                                PointsAmount = 0
                            };

                            partnerReckons.Add(transaction);

                            //1% от поставщиков випа
                            double webSellerBonus = 0;
                            foreach (var webSeller in vipPortion.User.ReferedWebSiteSellers)
                            {
                                webSellerBonus = webSeller.HangingPointsAccount * SettingsService.RewardsPlan.DistributionToPointsPercentageMap[DistributionType.SellerInvolvement] / 100;
                                if (webSellerBonus > 0)
                                {
                                    var sellerTransaction = new TransactionServiceModel()
                                    {
                                        Transaction = new Transaction()
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            Bonuses = webSellerBonus,
                                            BonusesBalans = vipPortion.User.BonusAccount + webSellerBonus,
                                            Time = DateTime.UtcNow,
                                            Type = TransactionType.VIPSellerBonus,
                                            PayeeId = vipPortion.User.Id,
                                            Payee = vipPortion.User,
                                            Description = webSeller.Name
                                        },
                                        PointsAmount = 0
                                    };
                                    partnerReckons.Add(sellerTransaction);
                                }
                            }

                            double benefitCardSellerBonus = 0;
                            foreach (var cardSeller in vipPortion.User.ReferedBenefitCardSellers)
                            {
                                benefitCardSellerBonus = cardSeller.HangingPointsAccount * SettingsService.RewardsPlan.DistributionToPointsPercentageMap[DistributionType.SellerInvolvement] / 100;
                                if (benefitCardSellerBonus > 0)
                                {
                                    var sellerTransaction = new TransactionServiceModel()
                                    {
                                        Transaction = new Transaction()
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            Bonuses = benefitCardSellerBonus,
                                            BonusesBalans = vipPortion.User.BonusAccount + benefitCardSellerBonus,
                                            Time = DateTime.UtcNow,
                                            Type = TransactionType.VIPSellerBonus,
                                            PayeeId = vipPortion.User.Id,
                                            Payee = vipPortion.User,
                                            Description = cardSeller.Name
                                        },
                                        PointsAmount = 0
                                    };
                                    partnerReckons.Add(sellerTransaction);
                                }
                            }

                            vipPortion.User.BonusAccount += bonusesRecon + webSellerBonus + benefitCardSellerBonus;
                            vipPortion.User.TotalBonusAccount += bonusesRecon + webSellerBonus + benefitCardSellerBonus;
                            db.Entry(vipPortion.User).State = EntityState.Modified;

                            //во вьюшку
                            result.VipPartners.Add(new PartnerReckon()
                            {
                                User = vipPortion.User,
                                BonusesReckoned = bonusesRecon + webSellerBonus + benefitCardSellerBonus,
                                QualifiedPartnersNumber = vipPortion.UserStructurePoints
                            });
                        }

                        //для всех пользователей, у кого есть баллы в обработке
                        allUsers.Where(entry => entry.HangingPointsAccount > 0).ToList().ForEach(entry =>
                        {
                            var hangingTransaction = new Transaction()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Bonuses = entry.HangingBonusAccount,
                                BonusesBalans = entry.BonusAccount + entry.HangingBonusAccount,
                                Time = DateTime.UtcNow,
                                Type = TransactionType.PersonalMonthAggregate,
                                PayeeId = entry.Id
                            };
                            db.Transactions.Add(hangingTransaction);

                            //бонусы в обработке - в доступные
                            entry.BonusAccount += entry.HangingBonusAccount;
                            //бонусы в обработке - в заработанные
                            entry.TotalBonusAccount += entry.HangingBonusAccount;
                            //обнулить бонусы в обработке 
                            entry.HangingBonusAccount = 0;
                            //обнулить баллы в обработке
                            entry.HangingPointsAccount = 0;

                            db.Entry(entry).State = EntityState.Modified;
                        });

                        db.Transactions.AddRange(partnerReckons.Select(entry => entry.Transaction));
                        db.SaveChanges();
                        dbTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbTransaction.Rollback();
                        return null;
                    }
                }
            }
            return result;
        }
    }
}
