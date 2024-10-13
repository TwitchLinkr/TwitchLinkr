using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchLinkr.TwitchAPI.APIResponseModels;

namespace TwitchLinkr.TwitchAPI.Requests
{
	internal static class RaidRequests
	{

		public static async Task<RaidResponseModel> CreateRaidAsync(string oAuthToken, string clientId, string broadcasterId, string targetBroadcasterId)
		{
			const string endpoint = "https://api.twitch.tv/helix/raids";

			var raidData = new
			{
				from_broadcaster_id = broadcasterId,
				to_broadcaster_id = targetBroadcasterId,
			};

			var jsonContent = JsonSerializer.Serialize(raidData);

			var response = await EndpointCaller.CallPostEndpointAsync(endpoint, [], oAuthToken, clientId, jsonContent);

			var raidResponse = JsonSerializer.Deserialize<RaidResponseModel>(response);

			return raidResponse!;
		}
	}
}
