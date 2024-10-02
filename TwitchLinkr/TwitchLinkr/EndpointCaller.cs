using System.Text;
using System.Text.Json;

namespace TwitchLinkr
{
	/// <summary>
	/// Provides methods to call GET and POST endpoints with OAuth token and Client ID authentication.
	/// </summary>
	internal static class EndpointCaller
	{
		/// <summary>
		/// Shapes the <c cref="HttpRequestException">HttpRequestException</c> to call a GET endpoint.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="parameters"></param>
		/// <param name="oauthToken"></param>
		/// <param name="clientId"></param>
		/// <param name="content"></param>
		/// <exception cref="HttpRequestException"></exception>"
		/// <returns><c>API</c> response as JSON.</returns>
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
		/// Shapes the <c cref="HttpRequestException">HttpRequestException</c> to call a POST endpoint.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="parameters"></param>
		/// <param name="oauthToken"></param>
		/// <param name="clientId"></param>
		/// <param name="content"></param>
		/// <exception cref="HttpRequestException"></exception>"
		/// <returns><c>API</c> response as JSON.</returns>
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
		/// Calls the API as specified in the <paramref name="request"/>.
		/// </summary>
		/// <param name="request"></param>
		/// <returns>API response as a string </returns>
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
