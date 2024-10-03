namespace TwitchLinkr
{
	/// <summary>
	/// Provides methods to call GET and POST endpoints with OAuth token and Client ID authentication.
	/// </summary>
	internal interface IEndpointCaller
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
		/// <returns><c>API</c> response as string in JSON format.</returns>
		abstract public static Task<string> CallGetEndpointAsync(string endpoint, Dictionary<string, string> parameters, string oauthToken, string clientId);

		/// <summary>
		/// Shapes the <c cref="HttpRequestException">HttpRequestException</c> to call a POST endpoint.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="parameters"></param>
		/// <param name="oauthToken"></param>
		/// <param name="clientId"></param>
		/// <param name="content"></param>
		/// <exception cref="HttpRequestException"/>
		/// <returns><c>API</c> response as string in JSON format.</returns>
		abstract public static Task<string> CallPostEndpointAsync(string endpoint, Dictionary<string, string> parameters, string oauthToken, string clientId, string content);


		/// <summary>
		/// Calls the API as specified in the <paramref name="request"/>.
		/// </summary>
		/// <param name="request"></param>
		/// <exception cref="HttpRequestException"/>
		/// <returns>API response as a string </returns>
		abstract public static Task<string> CallAPIAsync(HttpRequestMessage request);


	}
}
