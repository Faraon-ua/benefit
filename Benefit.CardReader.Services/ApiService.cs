using System;
using System.Net;
using System.Threading.Tasks;
using Benefit.CardReader.DataTransfer.Dto;
using Benefit.CardReader.DataTransfer.Dto.Base;
using Benefit.CardReader.DataTransfer.Ingest;
using Benefit.CardReader.DataTransfer.Offline;
using Benefit.CardReader.Services.Factories;
using Benefit.Common.Helpers;
using Benefit.HttpClient;
using Newtonsoft.Json;

namespace Benefit.CardReader.Services
{
    public class ApiService : IReaderManager
    {
        private BenefitHttpClient _httpClient = new BenefitHttpClient();
        public string AuthToken { get; set; }
        private DataService dataService = new DataService();

        public string GetAuthToken(string licenseKey)
        {
            var token = new TokenIngest
            {
                username = licenseKey
            };
            var tokenForm = ContentTypeConvert.SerializeToXwwwFormUrlencoded(token);
            var baseUri = new Uri(CardReaderSettingsService.ApiHost);
            var tokenUrl = new Uri(baseUri, CardReaderSettingsService.ApiTokenPrefix).ToString();
            var tokenResult = _httpClient.Post<TokenDto>(tokenUrl, tokenForm, "application/x-www-form-urlencoded");
            if (tokenResult.StatusCode == HttpStatusCode.OK)
            {
                if (tokenResult.Data.access_token != null)
                {
                    AuthToken = tokenResult.Data.access_token;
                    //add to offline
                    Task.Factory.StartNew(() => dataService.Add(AuthToken));
                }
                return tokenResult.Data.access_token;
            }
            return null;
        }

        public SellerCashierAuthDto AuthCashier(string cashierNfc)
        {
            var authCashierUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "AuthCashier?nfc=" + cashierNfc;
            var cashierResult = _httpClient.Get<SellerCashierAuthDto>(authCashierUrl, AuthToken);
            if (cashierResult.StatusCode == HttpStatusCode.OK)
            {
                if (cashierResult.Data != null)
                {
                    //add to offline
                    Task.Factory.StartNew(() => dataService.Add(new Cashier()
                    {
                        CardNfc = cashierNfc,
                        Name = cashierResult.Data.CashierName,
                        SellerName = cashierResult.Data.SellerName,
                        SellerShowBill = cashierResult.Data.ShowBill,
                        SellerShowKeyboard = cashierResult.Data.ShowKeyboard
                    }));
                }
                return cashierResult.Data;
            }
            return null;
        }

        public BenefitCardUserAuthDto AuthUser(string userNfc)
        {
            var authUserUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "AuthUser?nfc=" + userNfc;
            var userResult = _httpClient.Get<BenefitCardUserAuthDto>(authUserUrl, AuthToken);
            if (userResult.StatusCode == HttpStatusCode.OK)
                return userResult.Data;
            return null;
        }

        public BenefitCardUserDto GetUserInfo(string userNfc)
        {
            var userUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "UserInfo?nfc=" + userNfc;
            var userResult = _httpClient.Get<BenefitCardUserDto>(userUrl, AuthToken);
            if (userResult.StatusCode == HttpStatusCode.OK)
                return userResult.Data;
            return null;
        }

        public ResponseResult<PaymentResultDto> ProcessPayment(PaymentIngest paymentIngest)
        {
            var paymentUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "ProcessPayment";
            var postData = JsonConvert.SerializeObject(paymentIngest);
            var paymentResult = _httpClient.Post<PaymentResultDto>(paymentUrl, postData, "application/json", AuthToken);
            return paymentResult;
        }

        public void PingOnline(string authorizationToken, string licenseKey)
        {
            var pingUrl = string.Format("{0}{1}PingOnline?licenseKey={2}", CardReaderSettingsService.ApiHost,
                CardReaderSettingsService.ApiPrefix, licenseKey);
            _httpClient.Get<string>(pingUrl, authorizationToken);
        }
    }
}
