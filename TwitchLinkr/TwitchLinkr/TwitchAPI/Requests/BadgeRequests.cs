using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using TwitchLinkr.TwitchAPI.APIResponseModels;

namespace TwitchLinkr.TwitchAPI.Requests
{
	internal class BadgeRequests
	{

		public static async Task<ChatBadgesResponseModel> GetChannelChatBadgesAsync(string oAuthToken, string clientId, string broadcasterId)
		{
			const string endpoint = "https://api.twitch.tv/helix/chat/badges";

			// Format the parameters
			var parameters = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("broadcaster_id", broadcasterId)
			};

			// Call the endpoint
			var response = await EndpointCaller.CallGetEndpointAsync(endpoint, oAuthToken, clientId, parameters.ToArray());

			var data = JsonSerializer.Deserialize<ChatBadgesResponseModel>(response);

			return data!;

		}

		public static async Task<object> GetGlobalChatBadgesAsync(string oAuthToken, string clientId)
		{
			const string endpoint = "https://api.twitch.tv/helix/chat/badges/global";

			// Call the endpoint
			var response = await EndpointCaller.CallGetEndpointAsync(endpoint, oAuthToken, clientId);

			var data = JsonSerializer.Deserialize<ChatBadgesResponseModel>(response);
			return data!;
		}

	}
}
