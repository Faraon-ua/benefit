using Benefit.Common.Extensions;
using Benefit.DataTransfer.ApiDto.Allo;
using Benefit.DataTransfer.ApiDto.Marketplace.Epicentr;
using Benefit.Domain;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.HttpClient;
using Benefit.Services.Files;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Benefit.Services.ExternalApi
{
    public class EpicentrApiService : BaseMarketPlaceApi
    {
        private BenefitHttpClient _httpClient = new BenefitHttpClient();
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private NotificationsService _notificationService = new NotificationsService();

        public byte[] CategoriesToCSV()
        {
            var url = "https://api.epicentrm.com.ua/v2/pim/categories";
            var result = new List<CategoryDto>();
            var httpClient = new BenefitHttpClient();
            var catsDto = httpClient.Get<CategoriesDto>(url, "5a6489d1a5c48c9d174bd31f2a0a8fd0");
            result.AddRange(catsDto.Data.items);
            var page = 2;
            while (catsDto.Data.page < catsDto.Data.pages)
            {
                catsDto = httpClient.Get<CategoriesDto>(url + "?page=" + page++, "5a6489d1a5c48c9d174bd31f2a0a8fd0");
                result.AddRange(catsDto.Data.items);
            }
            var csvService = new FilesExportService();
            return csvService.CreateCSVFromGenericList(result);
        }
        public override string GetAccessToken(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public override Task ProcessOrders(string getOrdersUrl = null, string authToken = null, int type = 1, int offset = 0)
        {
            throw new NotImplementedException();
        }

        public override void UpdateOrderStatus(string id, OrderStatus oldStatus, OrderStatus newStatus, string ttn, int tryCount = 1, string sellerComment = null)
        {
            throw new NotImplementedException();
        }
    }
}
