using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchLinkr.TwitchAPI.APIResponseModels;

namespace TwitchLinkr.TwitchAPI.Requests
{
	/// <summary>
	/// Provides methods to interact with Twitch emotes.
	/// </summary>
	internal class EmoteRequests
	{
		/// <summary>
		/// Gets the list of channel emotes for a specified broadcaster.<br/>
		/// Requires an App Access Token or User Access Token.
		/// </summary>
		/// <param name="oAuthToken">The OAuth token for authentication.</param>
		/// <param name="clientId">The Client ID for authentication.</param>
		/// <param name="broadcasterId">The ID of the broadcaster whose emotes are being fetched.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the emotes response model.</returns>
		public static async Task<EmotesResponseModel> GetChannelEmotesAsync(string oAuthToken, string clientId, string broadcasterId)
		{
			const string endpoint = "https://api.twitch.tv/helix/chat/emotes";
			
			// Format the parameters
			var parameters = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("broadcaster_id", broadcasterId)
			};


			// Call the endpoint
			var response = await EndpointCaller.CallGetEndpointAsync(endpoint, oAuthToken, clientId, parameters.ToArray());

			// Deserialize the response
			var data = JsonSerializer.Deserialize<EmotesResponseModel>(response);

			return data!;
		}

		/// <summary>
		/// Gets the list of global emotes.<br/>
		/// Requires an App Access Token or User Access Token.
		/// </summary>
		/// <param name="oAuthToken">The OAuth token for authentication.</param>
		/// <param name="clientId">The Client ID for authentication.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the emotes response model.</returns>
		public static async Task<EmotesResponseModel> GetGlobalEmotesAsync(string oAuthToken, string clientId)
		{
			const string endpoint = "https://api.twitch.tv/helix/chat/emotes/global";

			// Call the endpoint
			var response = await EndpointCaller.CallGetEndpointAsync(endpoint, oAuthToken, clientId);

			// Deserialize the response
			var data = JsonSerializer.Deserialize<EmotesResponseModel>(response);

			return data!;
		}

		/// <summary>
		/// Gets the list of emotes for specified emote sets.<br/>
		/// Requires an App Access Token or User Access Token.
		/// </summary>
		/// <param name="oAuthToken">The OAuth token for authentication.</param>
		/// <param name="clientId">The Client ID for authentication.</param>
		/// <param name="EmoteSetIds">An array of emote set IDs to fetch emotes for.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the emotes response model.</returns>
		public static async Task<EmotesResponseModel> GetEmoteSetsAsync(string oAuthToken, string clientId, params string[] EmoteSetIds)
		{
			const string endpoint = "https://api.twitch.tv/helix/chat/emotes/set";

			// Format the parameters
			var parameters = new List<KeyValuePair<string, string>>();
			foreach (var setId in EmoteSetIds)
			{
				parameters.Add(new KeyValuePair<string, string>("emote_set_id", setId));
			}

			// Call the endpoint
			var response = await EndpointCaller.CallGetEndpointAsync(endpoint, oAuthToken, clientId, parameters.ToArray());

			// Deserialize the response
			var data = JsonSerializer.Deserialize<EmotesResponseModel>(response);

			return data!;
		}
	}
}
