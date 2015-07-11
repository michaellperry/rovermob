using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace RoverMob.Implementation
{
    public class HttpProxy : IDisposable
    {
        private readonly HttpClient _client;

        public HttpProxy(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> GetJsonAsync(Uri resourceUri)
        {
            var response = await _client.GetAsync(resourceUri);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task PostJsonAsync(Uri resourceUri, string json)
        {
            using (var content = new HttpStringContent(
                json,
                Windows.Storage.Streams.UnicodeEncoding.Utf8,
                "application/json"))
            {
                var response = await _client.PostAsync(resourceUri, content);
                response.EnsureSuccessStatusCode();
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
