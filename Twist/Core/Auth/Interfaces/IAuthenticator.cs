using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Twist.Core.Auth.Interfaces;

namespace Twist.Auth.Interfaces
{
    internal interface IAuthenticator
    {
        public ITwistCredentials ICredentials { get; set; }

        Task GetRequestTokenAsync(string url);
        Uri GetAuthorizeUrl(string url);
        Task<(string at, string ats, string uid, string sn)> GetAccessTokenAsync(string url, string pin);
        Task<string> RequestAsync(
           string ck,
           string cs,
           string token,
           string ts,
           string url,
           HttpMethod type,
           IDictionary<string, string> parameters = null,
           Stream stream = null
        );
    }
}
