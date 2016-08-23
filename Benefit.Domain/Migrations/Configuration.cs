using System;
using System.Collections.Generic;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
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

            if (!(context.Users.Any(u => u.UserName == Common.Constants.DomainConstants.DefaultAdminUserName)))
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var userToInsert = new ApplicationUser
                {
                    IsActive = true,
                    UserName = Common.Constants.DomainConstants.DefaultAdminUserName, 
                    ExternalNumber = 1007, 
                    ReferalId = null, 
                    FullName = "Àäì³í Àäì³íè÷",
                    Email = "faraon.ua@gmail.com",
                    CardNumber = "005656",
                    PhoneNumber = "0630517004",
                    RegisteredOn = DateTime.UtcNow
                };
                userManager.Create(userToInsert, Common.Constants.DomainConstants.DefaultAdminPassword);
                
                userManager.AddToRole(userToInsert.Id, "Admin");
            }

            if (!(context.Currencies.Any(u => u.Provider == Common.Constants.DomainConstants.DefaultUSDCurrencyProvider)))
            {
                var defaultCurrencies = new List<Currency>()
                {
                    new Currency()
                    {
                        Id = "125c1e97-f765-45e7-b47a-36a99c28947a",
                        Name = "UAH",
                        Provider = Common.Constants.DomainConstants.DefaultUSDCurrencyProvider,
                        Rate = 1
                    },
                    new Currency()
                    {
                        Id = "225c1e97-f765-45e7-b47a-36a99c28947a",
                        Name = "USD",
                        Provider = Common.Constants.DomainConstants.DefaultUSDCurrencyProvider,
                        Rate = 1
                    },
                    new Currency()
                    {
                        Id = "325c1e97-f765-45e7-b47a-36a99c28947a",
                        Name = "EUR",
                        Provider = Common.Constants.DomainConstants.DefaultUSDCurrencyProvider,
                        Rate = 1
                    },
                    new Currency()
                    {
                        Id = "425c1e97-f765-45e7-b47a-36a99c28947a",
                        Name = "RUR",
                        Provider = Common.Constants.DomainConstants.DefaultUSDCurrencyProvider,
                        Rate = 1
                    }
                };
                context.Currencies.AddRange(defaultCurrencies);
                context.SaveChanges();
            }
        }
    }
}
