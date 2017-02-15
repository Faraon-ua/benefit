using System.IO;
using System.Net;
using Benefit.CardReader.DataTransfer.Dto;
using Benefit.CardReader.DataTransfer.Ingest;
using Benefit.Common.Helpers;
using Benefit.HttpClient;

namespace Benefit.CardReader.Services
{
    public class ApiService
    {
        private BenefitHttpClient _httpClient = new BenefitHttpClient();
        public string GetAuthAToken(string licenseKey)
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

        public void PingOnline(string authorizationToken)
        {
            var pingUrl = CardReaderSettingsService.ApiHost + CardReaderSettingsService.ApiPrefix + "PingOnline";
            _httpClient.Get<string>(pingUrl, authorizationToken);
        }
    }
}
