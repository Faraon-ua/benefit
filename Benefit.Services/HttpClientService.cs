using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Benefit.Services
{
    public class HttpClientService
    {
        public async Task<T> PostObectToService<T>(string url, HttpContent content, string authorization = null)
        {
            using (var client = new HttpClient())
            {
                if (authorization != null)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                }
                var response = await client.PostAsync(url, content).ConfigureAwait(false);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using (JsonReader reader = new JsonTextReader(new StringReader(responseString)))
                {
                    try
                    {
                        return JsonSerializer.Create().Deserialize<T>(reader);
                    }
                    catch (Exception)
                    {
                        return default(T);
                    }
                }
            }
        }

        public async Task<T> GetObectFromService<T>(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using (JsonReader reader = new JsonTextReader(new StringReader(responseString)))
                {
                    try
                    {
                        return JsonSerializer.Create().Deserialize<T>(reader);
                    }
                    catch (Exception)
                    {
                        return default(T);
                    }
                }
            }
        } 
    }
}
