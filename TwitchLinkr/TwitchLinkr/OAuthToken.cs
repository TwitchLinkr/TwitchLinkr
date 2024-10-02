using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TwitchLinkr
{
    public class OAuthToken
    {
        private readonly ILogger<OAuthToken> logger;

        public OAuthToken(ILogger<OAuthToken> logger)
        {
            this.logger = logger;
        }

        public async Task<string> GetOAuthTokenAsync()
        {
            string clientId = Environment.GetEnvironmentVariable("ClientID");
            string clientSecret = Environment.GetEnvironmentVariable("ClientSecret");

            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://id.twitch.tv/oauth2/token");

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                request.Content = content;
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<OAuthTokenResponseModel>(responseBody);

                return tokenData.Access_token;
            }
        }
    }
}