using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Wyrobot.Web.Http.Discord
{
    public class DiscordAuthClient
    {
        public const string AuthorizeUrl = "https://discord.com/api/oauth2/authorize";
        public const string TokenUrl = "https://discord.com/api/v6/oauth2/token";

        private HttpClient _client;

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string RedirectUrl { get; set; }
        public List<string> Scopes { get; set; }

        public DiscordAuthClient()
        {
            _client = new HttpClient();
        }

        /// <summary>
        /// Builds the authorize URL to Discord based on this client instance's settings.
        /// The user is then supposed to be redirected to the built URL.
        /// </summary>
        /// <returns>The built URL to which the user should be redirected.</returns>
        public string BuildAuthorizeUrl()
        {
            StringBuilder builder = new StringBuilder(AuthorizeUrl);

            builder.Append("?response_type=code");

            builder.Append($"&client_id={ClientId}");
            builder.Append($"&redirect_url={HttpUtility.UrlEncode(RedirectUrl)}");

            builder.Append("&scope=");
            builder.Append(BuildScopes());

            return builder.ToString();
        }

        /// <summary>
        /// Requests a new access token based on either the code parameter that was passed to the redirect URL or a previously fetches refresh token.
        /// </summary>
        /// <param name="codeOrRefreshToken">The code or refresh token that should be used in the POST request to Discord.</param>
        /// <param name="useRefreshToken">Determines whether to treat the previous parameter as a refresh token.</param>
        /// <returns>The response model for an access token response from Discord.</returns>
        public async Task<DiscordTokenResponse> RequestAccessToken(string codeOrRefreshToken, bool useRefreshToken = false)
        {
            Dictionary<string, string> payload = new Dictionary<string, string>
            {
                {"client_id", ClientId},
                {"client_secret", ClientSecret},
                {"grant_type", "authorization_code"},
                {"redirect_uri", RedirectUrl},
                {"scope", BuildScopes(false)},
                {useRefreshToken ? "refresh_token" : "code", codeOrRefreshToken}
            };

            // Add the code or token parameter based on the request type (refresh token or not)

            FormUrlEncodedContent content = new FormUrlEncodedContent(payload);
            HttpResponseMessage response = await _client.PostAsync(TokenUrl, content);

            string body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<DiscordTokenResponse>(body);
        }

        private string BuildScopes(bool urlEncoded = true)
        {
            // A list of scopes has to be set; this is enforced because otherwise requests to Discord will not work
            if (Scopes == null)
                throw new InvalidOperationException("Scopes list was not set.");

            StringBuilder builder = new StringBuilder();

            foreach (string scope in Scopes)
            {
                builder.Append(scope);
                builder.Append(urlEncoded ? "%20" : " ");
            }

            // If the string is URL encoded, we need to remove the last 3 characters and return it
            if (urlEncoded)
                return builder.ToString().Substring(0, builder.Length - 3);

            // For regular strings, simply trimming the end of the string will do the trick
            return builder.ToString().TrimEnd();
        }
    }
}
