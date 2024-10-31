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
	/// Provides methods to interact with Twitch chat data.<br/>
	/// </summary>
	internal class ChatRequests
	{
		/// <summary>
		/// Gets the list of chatters in a broadcaster's chat.<br/>
		/// Requires a user access token that includes the <c>moderator:read:chatters</c> scope.<br/>
		/// The maximum value for 'first' is 1000 and the minimum is 1.
		/// </summary>
		/// <param name="oAuthToken">The OAuth token for authentication.</param>
		/// <param name="clientId">The Client ID for authentication.</param>
		/// <param name="broadcasterId">The ID of the broadcaster whose chatters are being fetched.</param>
		/// <param name="first">The number of chatters to fetch. Maximum is 1000, minimum is 1.</param>
		/// <param name="cursor">The cursor for pagination.</param>
		/// <returns>The response model containing the list of chatters.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the value of 'first' is not between 1 and 1000.</exception>
		public static async Task<GetChattersResponseModel> GetChattersAsync(string oAuthToken, string clientId, string broadcasterId, int first = 100, string cursor = "")
		{
			const string endpoint = "https://api.twitch.tv/helix/chat/chatters";

			// Ensure the first value is within the valid range
			if (first < 1 || first > 1000)
			{
				throw new ArgumentOutOfRangeException(nameof(first), "The value of 'first' must be between 1 and 1000.");
			}

			var chatParams = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("broadcaster_id", broadcasterId),
					new KeyValuePair<string, string>("first", first.ToString())
				};

			// Add the cursor if it's not empty
			if (!string.IsNullOrEmpty(cursor))
			{
				chatParams.Add(new KeyValuePair<string, string>("after", cursor));
			}

			var response = await EndpointCaller.CallGetEndpointAsync(endpoint, oAuthToken, clientId, chatParams.ToArray());

			var data = JsonSerializer.Deserialize<GetChattersResponseModel>(response);

			return data!;
		}
	

	
	}
}
