using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Benefit.Domain.DataAccess;

namespace Benefit.Domain.Models.ModelExtensions
{
    public static class UserExt
    {
        /* public static IEnumerable<ApplicationUser> GetAllStructurePartners(this ApplicationUser user)
         {
             using (var db = new ApplicationDbContext())
             {
                 foreach (var usr in db.Users.Include(entry=>entry.Partners).Where(entry => entry.ReferalId == user.Id))
                 {
                     yield return usr;

                     if (usr.Partners.Count > 0)
                     {
                         foreach (var childUsr in usr.GetAllStructurePartners())
                         {
                             yield return childUsr;
                         }
                     }
                 }
             }
         } */

        public static List<ApplicationUser> GetAllStructurePartners(this ApplicationUser user, ApplicationDbContext db, List<ApplicationUser> resultList = null)
        {
            if (resultList == null) resultList = new List<ApplicationUser>();
            var partners = db.Users.AsNoTracking().Where(entry => entry.ReferalId == user.Id);
            resultList.AddRange(partners);
            foreach (var partner in partners)
            {
                partner.GetAllStructurePartners(db, resultList);
            }
            return resultList;
        }
    }
}
