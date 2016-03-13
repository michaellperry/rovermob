using RoverMob.Implementation;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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
            int attempts = 2;
            while (true)
            {
                attempts--;
                try
                {
                    string accessToken = await _accessTokenProvider
                        .GetAccessTokenAsync();
                    using (var client = await HttpImplementation.CreateProxyAsync(
                        accessToken, null))
                    {
                        string result = await client.GetJsonAsync(_uri);
                        return Guid.Parse(result);
                    }
                }
                catch (COMException x) when (
                    x.HResult == unchecked((int)0x80190191) &&
                    attempts > 0)
                {
                    _accessTokenProvider.RefreshAccessToken();
                }
            }
        }
    }
}
