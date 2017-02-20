using System.Net;
using System.Text;
using Benefit.CardReader.DataTransfer.Dto.Base;
using Newtonsoft.Json;

namespace Benefit.HttpClient
{
    public class BenefitHttpClient
    {
        private WebClient client;

        public BenefitHttpClient()
        {
            client = new WebClient();
            client.Encoding = Encoding.UTF8;
        }

        public bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public ResponseResult<T> Get<T>(string url, string authorizationToken = null)
        {
            var result = new ResponseResult<T>();
            string response = null;
            if (authorizationToken != null && client.Headers["Authorization"] == null)
            {
                client.Headers.Add("Authorization", string.Format("Bearer {0}", authorizationToken));
            }
            try
            {
                response = client.DownloadString(url);
                result.StatusCode = HttpStatusCode.OK;
            }
            catch (WebException ex)
            {
                result.StatusCode = ((HttpWebResponse)ex.Response).StatusCode;
            }
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var dataResult = JsonConvert.DeserializeObject<T>(response);
                result.Data = dataResult;
            }
            return result;
        }

        public ResponseResult<T> Post<T>(string url, string ingestData, string contentType, string authorizationToken = null)
        {
            var result = new ResponseResult<T>();
            string response = null;
            try
            {
                if (authorizationToken != null && client.Headers["Authorization"] == null)
                {
                    client.Headers.Add("Authorization", string.Format("Bearer {0}", authorizationToken));
                }
                client.Headers[HttpRequestHeader.ContentType] = contentType;
                response = client.UploadString(url, ingestData);
                result.StatusCode = HttpStatusCode.OK;
            }
            catch (WebException ex)
            {
                result.StatusCode = ((HttpWebResponse) ex.Response).StatusCode;
            }
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var dataResult = JsonConvert.DeserializeObject<T>(response);
                result.Data = dataResult;
            }
            return result;
        }
    }
}
