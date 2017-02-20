﻿using System.IO;
using System.Net;
using Benefit.CardReader.DataTransfer.Dto;
using Benefit.CardReader.DataTransfer.Ingest;
using Benefit.Common.Helpers;
using Benefit.HttpClient;
using Newtonsoft.Json;

namespace Benefit.CardReader.Services
{
    public class ApiService
    {
        private BenefitHttpClient _httpClient = new BenefitHttpClient();
        public string AuthToken { get; set; }
        public string GetAuthToken(string licenseKey)
        {
            var token = new TokenIngest
            {
                username = licenseKey
            };
            var tokenForm = ContentTypeConvert.SerializeToXwwwFormUrlencoded(token);
            var tokenUrl = Path.Combine(CardReaderSettingsService.ApiHost, CardReaderSettingsService.ApiTokenPrefix);
            var tokenResult = _httpClient.Post<TokenDto>(tokenUrl, tokenForm, "application/x-www-form-urlencoded");
            if (tokenResult.StatusCode == HttpStatusCode.OK)
                return tokenResult.Data.access_token;
            return null;
        }

        public SellerCashierAuthDto AuthCashier(string cashierNfc)
        {
            var authCashierUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "AuthCashier?nfc=" + cashierNfc;
            var cashierResult = _httpClient.Get<SellerCashierAuthDto>(authCashierUrl, AuthToken);
            if (cashierResult.StatusCode == HttpStatusCode.OK)
                return cashierResult.Data;
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

        public PaymentResultDto ProcessPayment(PaymentIngest paymentIngest)
        {
            var paymentUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "ProcessPayment";
            var postData = JsonConvert.SerializeObject(paymentIngest);
            var paymentResult = _httpClient.Post<PaymentResultDto>(paymentUrl, postData, "application/json", AuthToken);
            if (paymentResult.StatusCode == HttpStatusCode.OK)
                return paymentResult.Data;
            return null;
        }

        public void PingOnline(string authorizationToken)
        {
            var pingUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "PingOnline";
            _httpClient.Get<string>(pingUrl, authorizationToken);
        }
    }
}
