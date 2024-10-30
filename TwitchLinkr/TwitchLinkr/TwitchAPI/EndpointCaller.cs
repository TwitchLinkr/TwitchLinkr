using System.Text;
using System.Net;
using TwitchLinkr.TwitchAPI.Exceptions;
using Microsoft.AspNetCore.Http;

namespace TwitchLinkr.TwitchAPI
{
    /// <summary>
    /// Provides methods to call GET and POST endpoints with OAuth token and Client ID authentication.
    /// </summary>
    internal static class EndpointCaller
    {

		private static HttpClient dontUseClient = default!;

		private static HttpClient Client
		{
			get
			{
				if (dontUseClient == null)
				{
					dontUseClient = new HttpClient();
				}
				return dontUseClient;
			}
		}

		public static async Task<string> CallPutEndpointAsync(string endpoint, string oauthToken, string clientId, params KeyValuePair<string, string>[] parameters)
		{
			// Create a new HttpRequestMessage object with the endpoint as the URL.
			var request = new HttpRequestMessage(HttpMethod.Put, endpoint);

			// Add the OAuth token and Client ID to the request headers.
			request.Headers.Add("Authorization", $"Bearer {oauthToken}");
			request.Headers.Add("Client-ID", clientId);

			request.RequestUri = AddParametersToUri(request.RequestUri!, parameters);

			// Send the request and return the response.
			return await CallAPIAsync(request);
		}
		/// <summary>
		/// Calls a GET endpoint with the specified parameters, OAuth token, and Client ID.
		/// </summary>
		/// <param name="endpoint">The URL of the endpoint to call.</param>
		/// <param name="parameters">The parameters to include in the request URL.</param>
		/// <param name="oauthToken">The OAuth token for authentication.</param>
		/// <param name="clientId">The Client ID for authentication.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the response from the endpoint.</returns>
		public static async Task<string> CallGetEndpointAsync(string endpoint, string oauthToken, string clientId, params KeyValuePair<string, string>[] parameters)
        {
            // Create a new HttpRequestMessage object with the endpoint as the URL.
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            
            // Add the OAuth token and Client ID to the request headers.
            request.Headers.Add("Authorization", $"Bearer {oauthToken}");
            request.Headers.Add("Client-ID", clientId);

			request.RequestUri = AddParametersToUri(request.RequestUri!, parameters);

			// Send the request and return the response.
			return await CallAPIAsync(request);
        }
		/// <summary>
		/// Calls a PATCH endpoint with the specified parameters, OAuth token, and Client ID.
		/// </summary>
		/// <param name="endpoint">The URL of the endpoint to call.</param>
		/// <param name="parameters">The parameters to include in the request URL.</param>
		/// <param name="oauthToken">The OAuth token for authentication.</param>
		/// <param name="clientId">The Client ID for authentication.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the response from the endpoint.</returns>
		public static async Task<string> CallPatchEndpointAsync(string endpoint, string oauthToken, string clientId, params KeyValuePair<string, string>[] parameters)
		{
			// Create a new HttpRequestMessage object with the endpoint as the URL.
			var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);

			// Add the OAuth token and Client ID to the request headers.
			request.Headers.Add("Authorization", $"Bearer {oauthToken}");
			request.Headers.Add("Client-ID", clientId);

			request.RequestUri = AddParametersToUri(request.RequestUri!, parameters);

			// Send the request and return the response.
			return await CallAPIAsync(request);
		}
		/// <summary>
		/// Calls a POST endpoint with the specified parameters, OAuth token, Client ID, and content.
		/// </summary>
		/// <remarks>
		/// Note: that you must serialize the content before calling this method.
		/// <code>
		/// var content = new {
		///     id = "12345",
		///     name = "Example"
		/// }
		/// var serializedContent = JsonSerializer.Serialize(pollData);
		/// 
		/// </code>
		/// </remarks>
		/// <param name="endpoint">The URL of the endpoint to call.</param>
		/// <param name="parameters">The parameters to include in the request URL.</param>
		/// <param name="oauthToken">The OAuth token for authentication.</param>
		/// <param name="clientId">The Client ID for authentication.</param>
		/// <param name="content">The content to include in the request body.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the response from the endpoint.</returns>
		public static async Task<string> CallPostEndpointAsync(string endpoint, string oauthToken, string clientId, string content, params KeyValuePair<string, string>[] parameters)
        {
            // Create a new HttpRequestMessage object with the endpoint as the URL.
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

            // Add the OAuth token and Client ID to the request headers.
            request.Headers.Add("Authorization", $"Bearer {oauthToken}");
            request.Headers.Add("Client-ID", clientId);
            request.Headers.Add("Content-Type", "application/json");

			// Add the content to the request.
			request.Content = new StringContent(content, Encoding.UTF8, "application/json");


			request.RequestUri = AddParametersToUri(request.RequestUri!, parameters);

			// Send the request and return the response.
			return await CallAPIAsync(request);
        }
		public static async Task<string> CallDeleteEndpointAsync(string endpoint, string oauthToken, string clientId, params KeyValuePair<string, string>[] parameters)
		{
			var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);

			// Add the OAuth token and Client ID to the request headers.
			request.Headers.Add("Authorization", $"Bearer {oauthToken}");
			request.Headers.Add("Client-ID", clientId);

			request.RequestUri = AddParametersToUri(request.RequestUri!, parameters);

			return await CallAPIAsync(request);
		}

		private static Uri AddParametersToUri(Uri requestUri, KeyValuePair<string, string>[] parameters)
		{
			// If there are parameters, add them to the URL.
			if (parameters == null || parameters.Length == 0) return requestUri;

			var query = string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
			return new Uri($"{requestUri}?{query}");
		}

        /// <summary>
        /// Sends an HTTP request and returns the response.
        /// </summary>
        /// <param name="request">The HTTP request to send.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response from the request.</returns>
        public static async Task<string> CallAPIAsync(HttpRequestMessage request)
        {
			using HttpResponseMessage response = await Client.SendAsync(request);
			
			if (response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				return responseBody!;
			}

			switch (response.StatusCode)
			{
				case HttpStatusCode.BadRequest:
					throw new BadRequestException(response.ReasonPhrase!);
				case HttpStatusCode.Unauthorized:
					throw new UnauthorizedException(response.ReasonPhrase!);
				case HttpStatusCode.NotFound:
					throw new NotFoundException(response.ReasonPhrase!);
				case HttpStatusCode.Conflict:
					throw new ConflictException(response.ReasonPhrase!);
				case HttpStatusCode.TooManyRequests:
					throw new TooManyRequestsException(response.ReasonPhrase!);
				default:
					throw new HttpRequestException(response.StatusCode + " - " + response.ReasonPhrase);
			}
			
			
		}
    }
}
