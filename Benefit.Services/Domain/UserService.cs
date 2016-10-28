using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Services.Domain
{
    public class UserService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public ApplicationUser GetUserInfo(string id)
        {
            var user = db.Users.FirstOrDefault(entry => entry.Id == id);
            return user;
        }
    }
}
