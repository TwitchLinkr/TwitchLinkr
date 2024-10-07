using System.Text.Json;
using TwitchLinkr.APIResponseModels;

namespace TwitchLinkr.TwitchAPI
{
	internal class TwitchAPIRequests
	{
		/// <summary>
		/// Creates a poll on Twitch using the provided OAuth token and client ID.
		/// </summary>
		/// <remarks>
		/// Requires a user access token with scope channel:manage:polls.
		/// </remarks>
		/// <param name="oAuthToken">The OAuth token for authorization. Requires scope channel:manage:polls.</param>
		/// <param name="clientId">The client ID of the application.</param>
		/// <param name="broadcasterId">The ID of the broadcaster creating the poll.</param>
		/// <param name="title">The title of the poll.</param>
		/// <param name="choices">An array of choices for the poll. Each choice is a string.</param>
		/// <param name="duration">The duration of the poll in seconds.</param>
		/// <param name="channelPointsPerVote">The number of Channel Points required per additional vote. Default is 500.</param>
		/// <param name="enableChannelPointVoting">Whether to enable voting with Channel Points. Default is false.</param>
		/// <param name="bitsPerVote">The number of Bits required per additional vote. Default is 10.</param>
		/// <param name="enableBitsVoting">Whether to enable voting with Bits. Default is false.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="CreatePollResponseModel"/></returns>
		public static async Task<CreatePollResponseModel> CreatePollAsync(string oAuthToken, string clientId, string broadcasterId, string title, string[] choices, int duration, int channelPointsPerVote = 500, bool enableChannelPointVoting = false, int bitsPerVote = 10, bool enableBitsVoting = false)
		{
			const string endpoint = "https://api.twitch.tv/helix/polls";

			// Format data content
			var choiceArray = choices.Select(choice => new { title = choice }).ToArray();
			var pollData = new
			{
				broadcaster_id = broadcasterId,
				title,
				choices = choiceArray,
				duration,
				channel_points_voting_enabled = enableChannelPointVoting,
				channel_points_per_vote = channelPointsPerVote,
				bits_voting_enabled = enableBitsVoting,
				bits_per_vote = bitsPerVote
			};
			
			// Serialize content
			var jsonContent = JsonSerializer.Serialize(pollData);

			// Call the endpoint
			var response = await EndpointCaller.CallPostEndpointAsync(endpoint, [], oAuthToken, clientId, jsonContent);

			// Deserialize the response
			var pollResponse = JsonSerializer.Deserialize<CreatePollResponseModel>(response);

			// Return the response. Unlikely to be null, since the EndpointCaller will throw an exception if the call is unsuccessful.
			return pollResponse!;
		}
		public static async Task EndPollAsync(string oAuthToken, string clientId, string broadcasterId, string pollId, bool archive = false)
		{
			const string endpoint = "https://api.twitch.tv/helix/polls";

			var parameters = new KeyValuePair<string, string>[]
			{
				new ("broadcaster_id", broadcasterId),
				new ("id", pollId),
				new ("status", archive ? "ARCHIVED" : "COMPLETED")
			};

			// Call the endpoint
			await EndpointCaller.CallPatchEndpointAsync(endpoint, oAuthToken, clientId, parameters);
		}
		public static async Task<PollResponseModel> GetPoll(string oAuthToken, string clientId, string broadcasterId)
		{
			var pollResponse = await GetPollBase(oAuthToken, clientId, new KeyValuePair<string, string>("broadcaster_id", broadcasterId));
			return pollResponse;
		}
		public static async Task<PollResponseModel> GetPoll(string oAuthToken, string clientId, string broadcasterId, params string[] pollIds)
		{

			var parameters = new KeyValuePair<string, string>[]
			{
				new ("broadcaster_id", broadcasterId),
			};

			// Add IDs to parameters
			parameters = parameters.Concat(pollIds.Select(id => new KeyValuePair<string, string>("id", id))).ToArray();

			return await GetPollBase(oAuthToken, clientId, parameters);
		}
		private static async Task<PollResponseModel> GetPollBase(string oAuthToken, string clientId, params KeyValuePair<string, string>[] parameters)
		{
			const string endpoint = "https://api.twitch.tv/helix/polls";
			
			// Call the endpoint
			var response = await EndpointCaller.CallGetEndpointAsync(endpoint, oAuthToken, clientId, parameters);
			
			// Deserialize the response
			return JsonSerializer.Deserialize<PollResponseModel>(response)!;

		}
		
		



	}
}
