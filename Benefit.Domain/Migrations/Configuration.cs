using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Text;
using Benefit.Common.Constants;
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
            AutomaticMigrationDataLossAllowed = true;
            CommandTimeout = 2500;
        }

        protected override void Seed(ApplicationDbContext context)
        {
           /* if (System.Diagnostics.Debugger.IsAttached == false)
            {
                System.Diagnostics.Debugger.Launch();
            }*/
            if (!context.Roles.Any(r => r.Name == DomainConstants.AdminRoleName))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var adminRole = new IdentityRole { Name = DomainConstants.AdminRoleName };
                var contentManagerRole = new IdentityRole { Name = DomainConstants.ContentManagerName };
                var sellerRole = new IdentityRole { Name = DomainConstants.SellerRoleName };

                manager.Create(adminRole);
                manager.Create(contentManagerRole);
                manager.Create(sellerRole);
            }

            if (!(context.Users.Any(u => u.UserName == DomainConstants.DefaultAdminUserName)))
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var userToInsert = new ApplicationUser
                {
                    IsActive = true,
                    UserName = DomainConstants.DefaultAdminUserName,
                    ExternalNumber = 1007,
                    ReferalId = null,
                    FullName = "Àäì³í Àäì³íè÷",
                    RegionId = 400000,
                    Email = "benefit.admin@gmail.com",
                    CardNumber = "000000",
                    PhoneNumber = "0630000000",
                    RegisteredOn = DateTime.UtcNow
                };
                userManager.Create(userToInsert, DomainConstants.DefaultAdminPassword);

                userManager.AddToRole(userToInsert.Id, DomainConstants.AdminRoleName);
            }

            if (!(context.Currencies.Any(u => u.Provider == DomainConstants.DefaultUSDCurrencyProvider)))
            {
                var defaultCurrencies = new List<Currency>()
                {
                    new Currency()
                    {
                        Id = "125c1e97-f765-45e7-b47a-36a99c28947a",
                        Name = "UAH",
                        Provider = DomainConstants.DefaultUSDCurrencyProvider,
                        Rate = 1
                    },
                    new Currency()
                    {
                        Id = "225c1e97-f765-45e7-b47a-36a99c28947a",
                        Name = "USD",
                        Provider = DomainConstants.DefaultUSDCurrencyProvider,
                        Rate = 1
                    },
                    new Currency()
                    {
                        Id = "325c1e97-f765-45e7-b47a-36a99c28947a",
                        Name = "EUR",
                        Provider = DomainConstants.DefaultUSDCurrencyProvider,
                        Rate = 1
                    },
                    new Currency()
                    {
                        Id = "425c1e97-f765-45e7-b47a-36a99c28947a",
                        Name = "RUR",
                        Provider = DomainConstants.DefaultUSDCurrencyProvider,
                        Rate = 1
                    }
                };
                context.Currencies.AddRange(defaultCurrencies);
                context.SaveChanges();
            }

            if (!context.Regions.Any())
            {
                var regionsSql = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty) + "Migrations/SqlFiles/UkraineRegions.sql", Encoding.UTF8);
                context.Database.ExecuteSqlCommand(regionsSql);
            }
        }
    }
}
