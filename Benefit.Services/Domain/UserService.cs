using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Services.Domain
{
    public class UserService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public ApplicationUser GetUserInfoWithPartners(string id)
        {
            var user = db.Users.Include(entry=>entry.Region).Include(entry => entry.Partners).Include(entry => entry.Referal).FirstOrDefault(entry => entry.Id == id);
            user.Partners = user.Partners.OrderByDescending(entry => entry.RegisteredOn).ToList();
            return user;
        }
        public ApplicationUser GetUserInfoWithRegions(string id)
        {
            var user = db.Users.Include(entry => entry.Region).Include(entry => entry.Addresses).Include(entry => entry.Addresses.Select(addr=>addr.Region)).FirstOrDefault(entry => entry.Id == id);
            return user;
        }

        public Dictionary<string, List<ApplicationUser>> GetPartnersByReferalIds(string[] userIds)
        {
            var allPartners = db.Users.Include(entry=>entry.Region).Where(entry => userIds.Contains(entry.ReferalId)).ToList();
            return userIds.ToDictionary(userId => userId, userId => allPartners.Where(entry => entry.ReferalId == userId).ToList());
        }
        public List<ApplicationUser> GetPartnersInDepth(string userId, int skip, int take = UserConstants.DefaultPartnersTakeCount)
        {
            var partners =
                db.Users.Include(entry => entry.Region).Where(entry => entry.ReferalId == userId).OrderByDescending(entry=>entry.RegisteredOn);
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
    }
}
