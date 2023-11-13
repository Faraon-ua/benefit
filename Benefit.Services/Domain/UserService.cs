using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ApiDto.SendPulse;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace Benefit.Services.Domain
{
    public class UserService
    {
        public ApplicationUser GetUser(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Users.Find(id);
            }
        }

        public string GetVipReferalCode()
        {
            using (var db = new ApplicationDbContext())
            {
                var completedVips =
                db.Users.Where(entry => entry.StatusCompletionMonths != null && entry.StatusCompletionMonths > 0)
                    .ToList();
                if (!completedVips.Any())
                {
                    return "53214e23-bfef-11e6-a542-c48508062b41";
                }
                var random = new Random();
                return completedVips[random.Next(completedVips.Count)].Id;
            }
        }

        public async Task SubscribeSendPulse(string email)
        {
            var httpClientService = new HttpClientService();
            var authValues = new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"},
                    {"client_id", "7e27ce1dac21e41636bffd1fda438479"},
                    {"client_secret", "413dec132624858344ba2013976ef3c4"}
                };
            string postBody = JsonConvert.SerializeObject(authValues);
            var content = new StringContent(postBody, Encoding.UTF8, "application/json");

            var authResult = await httpClientService.PostObectToService<SPAuthDto>(SettingsService.SendPulse.SendPulseAuthUrl, content).ConfigureAwait(false);

            var addEmailUrl = SettingsService.SendPulse.SendPulseAddEmailUrl.Replace("{id}",
                SettingsService.SendPulse.SendPulseFinLikbezAddressBookId);

            var strContent = "emails=[{\"email\": \"" + email + "\"}]";
            content = new StringContent(strContent, Encoding.UTF8, "application/x-www-form-urlencoded");
            var result = await httpClientService.PostObectToService<string>(addEmailUrl, content, authResult.access_token);
        }

        public string SetUserPic(string id, string fileExt)
        {
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(id);
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    var ImagesService = new ImagesService(db);
                    ImagesService.DeleteFile(user.Avatar, "", ImageType.UserAvatar);
                }
                if (user != null)
                {
                    user.Avatar = user.Id + DateTime.Now.ToShortTimeString().Replace(" ", "").Replace(":", "") + fileExt;
                    db.Entry(user).State = EntityState.Modified;
                }
                db.SaveChanges();
                return user.Avatar;
            }
        }
        public ApplicationUser GetUserInfoWithPartners(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Include(entry => entry.Region).Include(entry => entry.Partners.Select(part => part.Region)).Include(entry => entry.Referal).FirstOrDefault(entry => entry.Id == id);
                user.Partners = user.Partners.OrderByDescending(entry => entry.RegisteredOn).ToList();
                return user;
            }
        }
        public ApplicationUser GetUserInfoWithRegions(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users
                .Include(entry => entry.Region)
                .Include(entry => entry.Addresses)
                .Include(entry => entry.Addresses.Select(addr => addr.Region))
                .Include(entry => entry.OwnedSellers)
                .Include(entry => entry.ReferedBenefitCardSellers)
                .Include(entry => entry.ReferedWebSiteSellers)
                .Include(entry => entry.OwnedSellers)
                .Include(entry => entry.Personnels)
                .FirstOrDefault(entry => entry.Id == id);
                return user;
            }
        }
        
        public ApplicationUser GetUserInfoWithRegionsAndSellers(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var user =
                db.Users.Include(entry => entry.Region)
                    .Include(entry => entry.Addresses)
                    .Include(entry => entry.Addresses.Select(addr => addr.Region))
                    .Include(entry => entry.OwnedSellers)
                    .Include(entry => entry.ReferedBenefitCardSellers)
                    .Include(entry => entry.ReferedWebSiteSellers)
                    .FirstOrDefault(entry => entry.Id == id);
                var moderatedSellers = db.Sellers.Include(entry => entry.Personnels)
                                        .Where(entry => entry.Personnels.Select(pers => pers.UserId).Contains(id)).ToList();
                foreach (var moderatedSeller in moderatedSellers)
                {
                    user.OwnedSellers.Add(moderatedSeller);
                }
                return user;
            }
        }

        public Dictionary<string, List<ApplicationUser>> GetPartnersByReferalIds(string[] userIds)
        {
            using (var db = new ApplicationDbContext())
            {
                var allPartners = db.Users.Include(entry => entry.Region).Where(entry => userIds.Contains(entry.ReferalId)).ToList();
                return userIds.ToDictionary(userId => userId, userId => allPartners.Where(entry => entry.ReferalId == userId).ToList());
            }
        }
        public List<ApplicationUser> GetPartnersInDepth(string userId, int skip, int take = UserConstants.DefaultPartnersTakeCount)
        {
            using (var db = new ApplicationDbContext())
            {
                var partners =
                db.Users.Include(entry => entry.Region).Where(entry => entry.ReferalId == userId).OrderByDescending(entry => entry.RegisteredOn);
                if (skip > 0)
                    partners = partners.Skip(skip).OrderByDescending(entry => entry.RegisteredOn);
                if (take > 0)
                    partners = partners.Take(take).OrderByDescending(entry => entry.RegisteredOn);
                foreach (var partner in partners)
                {
                    partner.Partners = new List<ApplicationUser>(GetPartnersInDepth(partner.Id, -1, -1));
                }
                return partners.ToList();
            }
        }

        public void RemoveCard(string userId)
        {
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(userId);
                user.CardNumber = null;
                user.NFCCardNumber = null;
                user.IsCardVerified = false;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}
