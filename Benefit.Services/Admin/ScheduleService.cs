using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls.WebParts;
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
        public ProcessBonusesViewModel CloseQualificationPeriod()
        {
            using (var db = new ApplicationDbContext())
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        //для всех пользователей, у кого есть бонусы за текущий период
                        db.Users.Where(entry => entry.CurrentBonusAccount > 0).ToList().ForEach(entry =>
                        {
                            //бонусы за текущий период - в обработку
                            entry.HangingBonusAccount = entry.CurrentBonusAccount;
                            //обнулить бонусы за текущий период 
                            entry.CurrentBonusAccount = 0;
                            //баллы за текущий период - в обработку
                            entry.HangingPointsAccount = entry.PointsAccount;
                            //обнулить баллы в обработке
                            entry.PointsAccount = 0;
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
            return null;
        }
        public ProcessBonusesViewModel ProcessBonuses()
        {
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
                        var allUsers = db.Users.Include(entry => entry.Referal).ToList();

                        //берем всех партнеров, которые сделали хотя бы какой-то товарооборот за предыдущий период
                        var partners =
                            allUsers.Where(entry => entry.HangingPointsAccount > 0 && entry.ReferalId != null).ToList();
                        //транзакции
                        var partnerReckons = new List<TransactionServiceModel>();

                        //рассчет бонусов за приглашение
                        foreach (var partner in partners)
                        {
                            var currentReferal = partner.Referal;
                            var bonusesForMentor = partner.HangingPointsAccount *
                                                   SettingsService.RewardsPlan.DistributionToPointsPercentageMap[
                                                       DistributionType.Mentor] / 100;
                            do
                            {
                                if (currentReferal.HangingPointsAccount >= SettingsService.RewardsPlan.PointsQualificationAmount)
                                {
                                    //если добавили наставнику - прерываем цикл
                                    var totalBalansBeforeRecon = currentReferal.BonusAccount +
                                                      partnerReckons.Where(entry => entry.Transaction.PayeeId == currentReferal.Id)
                                                          .Sum(entry => entry.Transaction.Bonuses);
                                    var transaction = new TransactionServiceModel()
                                    {
                                        Transaction = new Transaction()
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            Bonuses = bonusesForMentor,
                                            BonusesBalans = totalBalansBeforeRecon + bonusesForMentor,
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
                            result.Partners.Add(partnerRecon);
                            db.Entry(payee).State = EntityState.Modified;
                        }

                        /*Рассчет комиссионных вип партнеров*/

                        var totalStructurePointsAmount = allUsers.Sum(entry => entry.HangingPointsAccount);
                        var vipBonusesStack = totalStructurePointsAmount * SettingsService.RewardsPlan.DistributionToPointsPercentageMap[DistributionType.VIP] / 100;

                        //вычислить випов
                        var vipPartners =
                            result.Partners.Where(
                                entry =>
                                    entry.User.HangingPointsAccount >= SettingsService.RewardsPlan.VIPPointsQualificationAmount &&
                                    entry.QualifiedPartnersNumber >= SettingsService.RewardsPlan.VIPQualifiedPartnersNumber).Select(entry => entry.User).ToList();

                        var vipPortions = new List<VipPortion>();
                        foreach (var vipPartner in vipPartners)
                        {
                            var vipPartnerTotalStructurePointAmount =
                                vipPartner.GetAllStructurePartners().Sum(entry => entry.HangingPointsAccount);
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

                        var vipPortionBonusRate = vipBonusesStack/totalVipPortions;

                        foreach (var vipPortion in vipPortions)
                        {
                            var bonusesRecon = (vipPortion.Portions > SettingsService.RewardsPlan.VIPMaxPortions
                                ? SettingsService.RewardsPlan.VIPMaxPortions
                                : vipPortion.Portions)*vipPortionBonusRate;
                            var totalBalansBeforeRecon = vipPortion.User.BonusAccount +
                                                     partnerReckons.Where(entry => entry.Transaction.PayeeId == vipPortion.User.Id)
                                                         .Sum(entry => entry.Transaction.Bonuses);
                            var transaction = new TransactionServiceModel()
                            {
                                Transaction = new Transaction()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Bonuses = bonusesRecon,
                                    BonusesBalans = totalBalansBeforeRecon + bonusesRecon,
                                    Time = DateTime.UtcNow,
                                    Type = TransactionType.VIPBonus,
                                    PayeeId = vipPortion.User.Id,
                                    Payee = vipPortion.User
                                },
                                PointsAmount = 0
                            };
                            result.VipPartners.Add(new PartnerReckon()
                            {
                                User = vipPortion.User,
                                BonusesReckoned = bonusesRecon,
                                QualifiedPartnersNumber = vipPortion.UserStructurePoints
                            });
                            partnerReckons.Add(transaction);
                        }


                        //для всех пользователей, у кого есть баллы в обработке
                        allUsers.Where(entry => entry.HangingPointsAccount > 0).ToList().ForEach(entry =>
                        {
                            //бонусы в обработке - в доступные
                            entry.BonusAccount += entry.HangingBonusAccount;
                            //обнулить бонусы в обработке 
                            entry.HangingBonusAccount = 0;
                            //обнулить баллы в обработке
                            entry.HangingPointsAccount = 0;

                            db.Entry(entry).State = EntityState.Modified;
                        });

                        db.Transactions.AddRange(partnerReckons.Select(entry => entry.Transaction));
                        //                        db.SaveChanges();
                        //                        dbTransaction.Commit();
                    }
                    catch (Exception)
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
