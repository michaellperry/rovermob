using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace RoverMob.Implementation
{
    public static class WebAuthenticationBrokerImplementation
    {
        public static async Task<string> GetAccessToken(Uri requestUri, Uri callbackUri)
        {
            var result = await WebAuthenticationBroker.AuthenticateAsync(
                WebAuthenticationOptions.None,
                requestUri,
                callbackUri);

            if (result.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var parameters = ParseParameters(result.ResponseData);
                var accessToken = GetParameter(parameters, "access_token");
                if (parameters.Any(p => p[0] == "error"))
                    throw new InvalidOperationException(GetParameter(parameters, "error_description"));
                else
                {
                    if (string.IsNullOrEmpty(accessToken))
                        throw new InvalidOperationException("No access token provided.");
                    return accessToken;
                }
            }
            else if (result.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
            {
                throw new InvalidOperationException("HTTP Error returned by AuthenticateAsync() : " + result.ResponseErrorDetail.ToString());
            }
            else
            {
                throw new InvalidOperationException("Error returned by AuthenticateAsync() : " + result.ResponseStatus.ToString());
            }
        }

        private static string[][] ParseParameters(string responseUrl)
        {
            var parameters = responseUrl
                .Split('#')
                .Skip(1)
                .SelectMany(q => q.Split('&'))
                .Select(p => p.Split('='))
                .Where(p => p.Length >= 2)
                .Select(p => p.Select(t => WebUtility.UrlDecode(t)).ToArray())
                .ToArray();
            return parameters;
        }

        private static string GetParameter(string[][] parameters, string name)
        {
            var accessToken = parameters
                .Where(p => p[0] == name)
                .Select(p => p[1])
                .FirstOrDefault();
            return accessToken;
        }
    }
}
