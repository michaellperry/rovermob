using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;

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
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            using (var content = new StreamContent(new MemoryStream(buffer)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue(
                    "application/json");
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
