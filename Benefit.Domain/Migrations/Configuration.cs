using System;
using Benefit.Domain.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Benefit.Domain.DataAccess.ApplicationDbContext>
    {
        private const string DefaultAdminUserName = "admin";
        private const string DefaultAdminPassword = "xD67F_gH367";

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DataAccess.ApplicationDbContext context)
        {
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var adminRole = new IdentityRole { Name = "Admin" };
                var contentManagerRole = new IdentityRole { Name = "ContentManager" };
                var sellerRole = new IdentityRole { Name = "Seller" };

                manager.Create(adminRole);
                manager.Create(contentManagerRole);
                manager.Create(sellerRole);
            }

            if (!(context.Users.Any(u => u.UserName == DefaultAdminUserName)))
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var userToInsert = new ApplicationUser
                {
                    B2BDoubleReward = false,
                    IsActive = true,
                    UserName = DefaultAdminUserName, 
                    ExternalNumber = 007, 
                    ReferalNumber = 007, 
                    FullName = "¿‰Ï≥Ì ¿‰Ï≥ÌË˜",
                    Email = "faraon.ua@gmail.com",
                    CardNumber = "005656",
                    PhoneNumber = "0630517004",
                    RegisteredOn = DateTime.UtcNow
                };
                userManager.Create(userToInsert, DefaultAdminPassword);
                
                userManager.AddToRole(userToInsert.Id, "Admin");
            }
        }
    }
}
