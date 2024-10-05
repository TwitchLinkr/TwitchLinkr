using System.Text;
using System.Text.Json;

namespace TwitchLinkr
{
	/// <summary>
	/// Provides methods to call GET and POST endpoints with OAuth token and Client ID authentication.
	/// </summary>
	internal class EndpointCaller : IEndpointCaller
	{
		/// <summary>
		/// Calls a GET endpoint with the specified parameters, OAuth token, and Client ID.
		/// </summary>
		/// <param name="endpoint">The URL of the endpoint to call.</param>
		/// <param name="parameters">The parameters to include in the request URL.</param>
		/// <param name="oauthToken">The OAuth token for authentication.</param>
		/// <param name="clientId">The Client ID for authentication.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the response from the endpoint.</returns>
		public static async Task<string> CallGetEndpointAsync(string endpoint, Dictionary<string, string> parameters, string oauthToken, string clientId)
		{
			// Create a new HttpRequestMessage object with the endpoint as the URL.
			var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

			// Add the OAuth token and Client ID to the request headers.
			request.Headers.Add("Authorization", $"Bearer {oauthToken}");
			request.Headers.Add("Client-ID", clientId);

			// If there are parameters, add them to the URL.
			if (parameters != null)
			{
				var query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
				request.RequestUri = new Uri($"{request.RequestUri}?{query}");
			}

			// Send the request and return the response.
			return await CallAPIAsync(request);
		}

		/// <summary>
		/// Calls a POST endpoint with the specified parameters, OAuth token, Client ID, and content.
		/// </summary>
		/// <param name="endpoint">The URL of the endpoint to call.</param>
		/// <param name="parameters">The parameters to include in the request URL.</param>
		/// <param name="oauthToken">The OAuth token for authentication.</param>
		/// <param name="clientId">The Client ID for authentication.</param>
		/// <param name="content">The content to include in the request body.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the response from the endpoint.</returns>
		public static async Task<string> CallPostEndpointAsync(string endpoint, Dictionary<string, string> parameters, string oauthToken, string clientId, string content)
		{
			// Create a new HttpRequestMessage object with the endpoint as the URL.
			var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

			// Add the OAuth token and Client ID to the request headers.
			request.Headers.Add("Authorization", $"Bearer {oauthToken}");
			request.Headers.Add("Client-ID", clientId);
			request.Headers.Add("Content-Type", "application/json");

			// Add the content to the request.
			request.Content = new StringContent(content, Encoding.UTF8, "application/json");

			// If there are parameters, add them to the URL.
			if (parameters != null)
			{
				var query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
				request.RequestUri = new Uri($"{request.RequestUri}?{query}");
			}

			// Send the request and return the response.
			return await CallAPIAsync(request);
		}

		/// <summary>
		/// Sends an HTTP request and returns the response.
		/// </summary>
		/// <param name="request">The HTTP request to send.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the response from the request.</returns>
		public static async Task<string> CallAPIAsync(HttpRequestMessage request)
		{
			using (HttpClient client = new HttpClient())
			{
				HttpResponseMessage response = await client.SendAsync(request);
				response.EnsureSuccessStatusCode();

				string responseBody = await response.Content.ReadAsStringAsync();
				return responseBody!;
			}
		}
	}
}
