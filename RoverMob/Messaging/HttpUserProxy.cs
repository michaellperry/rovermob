using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace RoverMob.Messaging
{
    public class HttpUserProxy : IUserProxy
    {
        private readonly Uri _uri;
        private readonly IAccessTokenProvider _accessTokenProvider;
        
        public HttpUserProxy(
            Uri uri,
            IAccessTokenProvider accessTokenProvider)
        {
            _uri = uri;
            _accessTokenProvider = accessTokenProvider;
        }

        public async Task<Guid> GetUserIdentifier(string role)
        {
            var httpBaseFilder = new HttpBaseProtocolFilter
            {
                AllowUI = false
            };
            using (HttpClient client = new HttpClient(httpBaseFilder))
            {
                string accessToken = await _accessTokenProvider
                    .GetAccessTokenAsync();
                client.DefaultRequestHeaders.Authorization =
                    new HttpCredentialsHeaderValue(
                        "Bearer", accessToken);

                var result = await client.GetStringAsync(_uri);
                return Guid.Parse(result);
            }
        }
    }
}
