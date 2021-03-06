﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Windows.Web.Http.Filters;

namespace RoverMob.Implementation
{
    public static class HttpImplementation
    {
        public static async Task<HttpProxy> CreateProxyAsync(string accessToken, string apiKey)
        {
            var httpBaseFilder = new HttpBaseProtocolFilter
            {
                AllowUI = false
            };
            HttpClient client = new HttpClient(httpBaseFilder);
            if (accessToken != null)
            {
                client.DefaultRequestHeaders.Authorization =
                    new HttpCredentialsHeaderValue(
                        "Bearer", accessToken);
            }
            if (!string.IsNullOrWhiteSpace(apiKey))
                client.DefaultRequestHeaders.Add("ApiKey", apiKey);
            return new HttpProxy(client);
        }
    }
}
