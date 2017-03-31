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
        ApplicationDbContext db = new ApplicationDbContext();

        public ApplicationUser GetUser(string id)
        {
            return db.Users.Find(id);
        }
        public async Task SubscribeSendPulse(string email)
        {
            var httpClientService = new HttpClientService();
            var authValues = new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"},
                    {"client_id", "81cbe7e753aee8266585f7aa4d112bcf"},
                    {"client_secret", "f459f5679fe6c3e8dc0000079dc6a5c2"}
                };
            string postBody = JsonConvert.SerializeObject(authValues);
            var content = new StringContent(postBody, Encoding.UTF8, "application/json");

            var authResult = await httpClientService.GetObectFromService<SPAuthDto>(SettingsService.SendPulse.SendPulseAuthUrl, content);

            var addEmailUrl = SettingsService.SendPulse.SendPulseAddEmailUrl.Replace("{id}",
                SettingsService.SendPulse.SendPulseFinLikbezAddressBookId);

            var strContent = "emails=[{\"email\": \"" + email + "\"}]";
            content = new StringContent(strContent, Encoding.UTF8, "application/x-www-form-urlencoded");
            var result = await httpClientService.GetObectFromService<string>(addEmailUrl, content, authResult.access_token);
        }

        public string SetUserPic(string id, string fileExt)
        {
            var user = db.Users.Find(id);
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                var ImagesService = new ImagesService();
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
        public ApplicationUser GetUserInfoWithPartners(string id)
        {
            var user = db.Users.Include(entry => entry.Region).Include(entry => entry.Partners.Select(part => part.Region)).Include(entry => entry.Referal).FirstOrDefault(entry => entry.Id == id);
            user.Partners = user.Partners.OrderByDescending(entry => entry.RegisteredOn).ToList();
            return user;
        }
        public ApplicationUser GetUserInfoWithRegions(string id)
        {
            var user = db.Users.Include(entry => entry.Region).Include(entry => entry.Addresses).Include(entry => entry.Addresses.Select(addr => addr.Region)).FirstOrDefault(entry => entry.Id == id);
            return user;
        }
        
        public ApplicationUser GetUserInfoWithRegionsAndSellers(string id)
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

        public Dictionary<string, List<ApplicationUser>> GetPartnersByReferalIds(string[] userIds)
        {
            var allPartners = db.Users.Include(entry => entry.Region).Where(entry => userIds.Contains(entry.ReferalId)).ToList();
            return userIds.ToDictionary(userId => userId, userId => allPartners.Where(entry => entry.ReferalId == userId).ToList());
        }
        public List<ApplicationUser> GetPartnersInDepth(string userId, int skip, int take = UserConstants.DefaultPartnersTakeCount)
        {
            var partners =
                db.Users.Include(entry => entry.Region).Where(entry => entry.ReferalId == userId).OrderByDescending(entry => entry.RegisteredOn);
            if (skip > 0)
                partners = partners.Skip(skip).OrderByDescending(entry => entry.RegisteredOn);
            if (take > 0)
                partners = partners.Take(take).OrderByDescending(entry => entry.RegisteredOn); ;
            var temp = partners.ToList();
            foreach (var partner in partners)
            {
                partner.Partners = new List<ApplicationUser>(GetPartnersInDepth(partner.Id, -1, -1));
            }
            return partners.ToList();
        }

        public void RemoveCard(string userId)
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
