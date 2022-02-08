using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Benefit.DataTransfer.ApiDto.PrivatBank;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Domain.Models.ModelExtensions;
using Benefit.Domain.Models.Service;
using Benefit.Services.ExternalApi;
using Benefit.Services.Files;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Benefit.Services.Admin
{
    public class ScheduleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ScheduleService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public ScheduleService()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public async Task UpdateCurrencies()
        {
            using (var db = new ApplicationDbContext())
            {
                var client = new HttpClientService();
                var currencies = await client.GetObectFromService<List<PrivatBankCurrency>>(SettingsService.PrivatBank.CurrenciesApiUrl).ConfigureAwait(false);
                foreach (var currency in currencies)
                {
                    var dbCurrency =
                        db.Currencies.FirstOrDefault(
                            entry => entry.Name == currency.ccy && entry.Provider == CurrencyProvider.PrivatBank);
                    if (dbCurrency != null)
                    {
                        dbCurrency.Rate = currency.sale;
                        db.Entry(dbCurrency).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
            }
        }
    }
}