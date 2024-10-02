using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TwitchLinkr
{
	/// <summary>
	/// Provides methods to obtain OAuth tokens for server-to-server API requests.
	/// </summary>
	public class OAuthToken
	{
		private readonly ILogger<OAuthToken> logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthToken"/> class.
		/// </summary>
		/// <param name="logger">The logger to use for logging information and errors.</param>
		public OAuthToken(ILogger<OAuthToken> logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Asynchronously retrieves an OAuth token using the client credentials grant flow.
		/// </summary>
		/// <returns>A task that represents the asynchronous operation. The task result contains the access token as a string.</returns>
		/// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the access token cannot be retrieved.</exception>
		public async Task<string> GetOAuthTokenAsync(string clientId, string clientSecret)
		{

			var request = new HttpRequestMessage(HttpMethod.Post, "https://id.twitch.tv/oauth2/token");

			var parameters = new Dictionary<string, string>
				{
					{ "client_id", clientId },
					{ "client_secret", clientSecret },
					{ "grant_type", "client_credentials" }
				};

			var content = new FormUrlEncodedContent(parameters);
			request.Content = content;

			try
			{
				string responseBody = await EndpointCaller.CallAPIAsync(request);
				var tokenData = JsonSerializer.Deserialize<OAuthTokenResponseModel>(responseBody);

				if (tokenData == null || string.IsNullOrEmpty(tokenData.access_token))
				{
					logger.LogError("Failed to retrieve access token. Response: {ResponseBody}", responseBody);
					throw new InvalidOperationException("Failed to retrieve access token.");
				}

				return tokenData.access_token;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occurred while retrieving the OAuth token.");
				logger.LogInformation("The failed OAuth Request: {Request}", request);
				throw;
			}
		}
	}

	/// <summary>
	/// Represents the response model for the OAuth token request.
	/// </summary>
	public class OAuthTokenResponseModel
	{
		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		public string access_token { get; set; }

		/// <summary>
		/// Gets or sets the type of the token.
		/// </summary>
		public string token_type { get; set; }

		/// <summary>
		/// Gets or sets the expiration time of the token in seconds.
		/// </summary>
		public int expires_in { get; set; }
	}
}