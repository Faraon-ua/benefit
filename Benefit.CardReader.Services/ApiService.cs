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
using NLog;

namespace Benefit.CardReader.Services
{
    public class ApiService : IReaderManager
    {
        private BenefitHttpClient _httpClient = new BenefitHttpClient();
        public string AuthToken { get; set; }
        public bool LogRequests { get; set; }
        private DataService dataService = new DataService();
        private Logger _logger = LogManager.GetCurrentClassLogger();

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

        public string GetSellerName(string licenseKey)
        {
            var getSellerNameUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "GetSellerName?licenseKey=" + licenseKey;
            var sellerNameResult = _httpClient.Get<string>(getSellerNameUrl, AuthToken);
            if (sellerNameResult.StatusCode == HttpStatusCode.OK)
                return sellerNameResult.Data;
            return null;
        }

        public SellerCashierAuthDto AuthCashier(string cashierNfc)
        {
            var authCashierUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "AuthCashier?nfc=" + cashierNfc;
            var cashierResult = _httpClient.Get<SellerCashierAuthDto>(authCashierUrl, AuthToken);
            if (LogRequests)
            {
                var message = string.Format("[AuthCashier]\n Url: {0}", authCashierUrl);
                _logger.Info(message);
            }
            if (cashierResult.StatusCode == HttpStatusCode.OK)
            {
                if (LogRequests)
                {
                    _logger.Info("OK\n\n");
                }
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
            if (LogRequests)
            {
                _logger.Info(cashierResult.StatusCode + Environment.NewLine + cashierResult.ErrorMessage);
            }
            return null;
        }

        public BenefitCardUserAuthDto AuthUser(string userNfc)
        {
            var authUserUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "AuthUser?nfc=" + userNfc;
            if (LogRequests)
            {
                var message = string.Format("[AuthUser]\n Url: {0}", authUserUrl);
                _logger.Info(message);
            }
            var userResult = _httpClient.Get<BenefitCardUserAuthDto>(authUserUrl, AuthToken);
            if (userResult.StatusCode == HttpStatusCode.OK)
            {
                if (LogRequests)
                {
                    _logger.Info("OK\n\n");
                }
                return userResult.Data;
            }
            if (LogRequests)
            {
                _logger.Info(userResult.StatusCode + Environment.NewLine + userResult.ErrorMessage);
            }
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
            if (LogRequests)
            {
                var message = string.Format("{0}\n Url: {1}\nData: {2}\nStatus: {3}\n\n", "[ProcessPayment]", paymentUrl, postData, paymentResult.StatusCode);
                _logger.Info(message);
            }
            return paymentResult;
        }

        public void PingOnline(string authorizationToken, string licenseKey)
        {
            var pingUrl = string.Format("{0}{1}PingOnline?licenseKey={2}", CardReaderSettingsService.ApiHost,
                CardReaderSettingsService.ApiPrefix, licenseKey);
            var result = _httpClient.Get<string>(pingUrl, authorizationToken);
            if (LogRequests)
            {
                var message = string.Format("[PingOnline]\n Url: {0}", pingUrl);
                _logger.Info(message);
            }
            if (result.StatusCode == HttpStatusCode.OK)
            {
                _logger.Info("OK\n\n");
            }
            if (result.StatusCode != HttpStatusCode.OK)
            {
                _logger.Fatal(result.StatusCode + Environment.NewLine + result.ErrorMessage+"\n\n");
            }
        }
    }
}
