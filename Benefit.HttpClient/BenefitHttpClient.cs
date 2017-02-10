using System;
using System.IO;
using System.Net;
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

        public ResponseResult<T> Get<T>(string url)
        {
            var result = new ResponseResult<T>();
            string response = null;
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

        public ResponseResult<T> Post<T>(string url, string ingestData, string contentType)
        {
            var result = new ResponseResult<T>();
            string response = null;
            try
            {
                client.Headers[HttpRequestHeader.ContentType] = contentType;
                response = client.UploadString(url, ingestData);
                result.StatusCode = HttpStatusCode.OK;
            }
            catch (WebException ex)
            {
                result.StatusCode = ((HttpWebResponse) ex.Response).StatusCode;
            }
            catch (Exception ex)
            {
                
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
