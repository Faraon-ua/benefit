using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using Benefit.CardReader.DataTransfer.Dto.Base;
using Newtonsoft.Json;
using System.IO;
using Benefit.Common.Extensions;

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

                var responseStream = ex.Response == null ? null : ex.Response.GetResponseStream();
                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        var definition = new { Message = "" };
                        var json = reader.ReadToEnd();
                        try
                        {
                            var jsonResult = JsonConvert.DeserializeAnonymousType(json, definition);
                            result.ErrorMessage = jsonResult == null ? null : jsonResult.Message;
                        }
                        catch (Exception exc)
                        {
                            Debug.WriteLine(exc.Message);
                        }
                    }
                }
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
                result.StatusCode = ((HttpWebResponse)ex.Response).StatusCode;

                var responseStream = ex.Response == null ? null : ex.Response.GetResponseStream();
                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        var definition = new { Message = "" };
                        var content = reader.ReadToEnd();
                        if (content.IsJson())
                        {
                            var jsonResult = JsonConvert.DeserializeAnonymousType(content, definition);
                            result.ErrorMessage = jsonResult.Message;
                        }
                    }
                }
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
