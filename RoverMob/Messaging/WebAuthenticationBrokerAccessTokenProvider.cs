using Newtonsoft.Json.Linq;
using RoverMob.Implementation;
using RoverMob.Tasks;
using RoverMob.Utilities;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public class WebAuthenticationBrokerAccessTokenProvider : Process, IAccessTokenProvider
    {
        private readonly string _baseUri;
        private readonly string _role;
        private readonly string _loggedInPath;
        private readonly string _externalLoginsPath;

        private string _accessToken;
        private ImmutableList<TaskCompletionSource<string>> _accessTokenCompletions =
            ImmutableList<TaskCompletionSource<string>>.Empty;

        public WebAuthenticationBrokerAccessTokenProvider(
            string baseUri,
            string role,
            string loggedInPath,
            string externalLoginsPath = "/api/account/externalLogins")
        {
            _baseUri = baseUri;
            _role = role;
            _loggedInPath = loggedInPath;
            _externalLoginsPath = externalLoginsPath;
        }

        public Task<string> GetAccessTokenAsync()
        {
            if (!String.IsNullOrEmpty(_accessToken))
                return Task.FromResult(_accessToken);

            var completion = new TaskCompletionSource<string>();
            lock (this)
            {
                _accessTokenCompletions = _accessTokenCompletions.Add(
                    completion);
            }
            Authenticate();

            return completion.Task;
        }

        public void RefreshAccessToken()
        {
            _accessToken = null;
            PasswordVaultImplementation.RemoveCredential(_baseUri, _role);
        }

        public void Authenticate()
        {
            Perform(async delegate
            {
                await InternalAuthenticateAsync();
            });
        }

        private async Task InternalAuthenticateAsync()
        {
            if (!String.IsNullOrEmpty(_accessToken))
            {
                ReceiveAccessToken(_accessToken);
                return;
            }

            try
            {
                string accessToken = PasswordVaultImplementation.RetrieveCredential(_baseUri, _role);
                if (accessToken != null)
                {
                    ReceiveAccessToken(accessToken);
                }
                else
                {
                    accessToken = await LogIn(_baseUri);
                    PasswordVaultImplementation.StoreCredential(_baseUri, _role, accessToken);
                }
            }
            catch (Exception x)
            {
                ReceiveError(x);
            }
        }

        private async Task<string> LogIn(string resource)
        {
            Uri baseUri = new Uri(resource, UriKind.Absolute);
            var externalLoginsUrl = new Uri(baseUri, $"{_externalLoginsPath}?returnUrl={_loggedInPath}&generateState=true").ToString();
            var providers = await ApiUtility.GetJsonAsync(externalLoginsUrl);
            var providerUrl = providers
                .OfType<JObject>()
                .Select(p => p["Url"].Value<string>())
                .FirstOrDefault();

            var requestUri = new Uri(baseUri, providerUrl);
            var callbackUri = new Uri(baseUri, _loggedInPath);

            string accessToken = await WebAuthenticationBrokerImplementation
                .GetAccessToken(requestUri, callbackUri);
            return ReceiveAccessToken(accessToken);
        }

        private string ReceiveAccessToken(string accessToken)
        {
            _accessToken = accessToken;
            ImmutableList<TaskCompletionSource<string>> completions;
            lock (this)
            {
                completions = _accessTokenCompletions;
                _accessTokenCompletions = ImmutableList<
                    TaskCompletionSource<string>>.Empty;
            }
            foreach (var completion in completions)
                completion.SetResult(_accessToken);

            return accessToken;
        }

        private void ReceiveError(Exception exception)
        {
            ImmutableList<TaskCompletionSource<string>> completions;
            lock (this)
            {
                completions = _accessTokenCompletions;
                _accessTokenCompletions = ImmutableList<
                    TaskCompletionSource<string>>.Empty;
            }
            foreach (var completion in completions)
                completion.SetException(exception);
        }
    }
}
