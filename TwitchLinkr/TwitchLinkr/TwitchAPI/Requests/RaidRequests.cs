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
	/// Provides methods to create and cancel raids on Twitch.  
	/// Requires a User Access token with the channel:manage:raids scope.  
	/// </summary>  
	internal static class RaidRequests
	{
		/// <summary>  
		/// Creates a raid from one broadcaster to another.  <br/>
		/// Requires a User Access token with the <c>channel:manage:raids</c> scope. 
		/// </summary>  
		/// <param name="oAuthToken">The OAuth token for authentication.</param>  
		/// <param name="clientId">The Client ID for authentication.</param>  
		/// <param name="broadcasterId">The ID of the broadcaster initiating the raid.</param>  
		/// <param name="targetBroadcasterId">The ID of the target broadcaster to raid.</param>  
		/// <returns>A task that represents the asynchronous operation. The task result contains the raid response model.</returns>  
		public static async Task<RaidResponseModels> CreateRaidAsync(string oAuthToken, string clientId, string broadcasterId, string targetBroadcasterId)
		{
			const string endpoint = "https://api.twitch.tv/helix/raids";

			var raidData = new
			{
				from_broadcaster_id = broadcasterId,
				to_broadcaster_id = targetBroadcasterId,
			};

			var jsonContent = JsonSerializer.Serialize(raidData);

			var response = await EndpointCaller.CallPostEndpointAsync(endpoint, oAuthToken, clientId, jsonContent);

			var raidResponse = JsonSerializer.Deserialize<RaidResponseModels>(response);

			return raidResponse!;
		}

		/// <summary>  
		/// Cancels an ongoing raid for a broadcaster.		<br/>
		/// Requires a User Access token with the <c>channel:manage:raids</c> scope.  
		/// </summary>  
		/// <param name="oAuthToken">The OAuth token for authentication.</param>  
		/// <param name="clientId">The Client ID for authentication.</param>  
		/// <param name="broadcasterId">The ID of the broadcaster canceling the raid.</param>  
		/// <returns>A task that represents the asynchronous operation.</returns>  
		public static async Task CancelRaidAsync(string oAuthToken, string clientId, string broadcasterId)
		{
			const string endpoint = "https://api.twitch.tv/helix/raids";
			await EndpointCaller.CallDeleteEndpointAsync(endpoint, oAuthToken, clientId, new KeyValuePair<string, string>("broadcaster_id", broadcasterId));
		}
	}
}
