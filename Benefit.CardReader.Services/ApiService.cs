using System.IO;
using System.Net;
using Benefit.CardReader.DataTransfer.Dto;
using Benefit.CardReader.DataTransfer.Ingest;
using Benefit.HttpClient;
using Newtonsoft.Json;

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
            var tokenJson = JsonConvert.SerializeObject(token);
            var tokenUrl = Path.Combine(CardReaderSettingsService.ApiHost, CardReaderSettingsService.ApiTokenPrefix);
            var tokenResult = _httpClient.Post<TokenDto>(tokenUrl, tokenJson);
            if (tokenResult.StatusCode == HttpStatusCode.OK)
                return tokenResult.Data.access_token;
            return null;
        }
    }
}
