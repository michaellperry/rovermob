using RoverMob.Implementation;
using System;
using System.Threading.Tasks;

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
            string accessToken = await _accessTokenProvider
                .GetAccessTokenAsync();
            using (var client = await HttpImplementation.CreateProxyAsync(
                accessToken))
            {
                string result = await client.GetJsonAsync(_uri);
                return Guid.Parse(result);
            }
        }
    }
}
