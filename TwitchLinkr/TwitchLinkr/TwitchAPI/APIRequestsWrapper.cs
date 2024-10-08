using System.Text.Json;
using TwitchLinkr.TwitchAPI.APIResponseModels;
using TwitchLinkr.TwitchAPI.Requests;

namespace TwitchLinkr.TwitchAPI
{
	internal class APIRequestsWrapper
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
			return await PollRequests.CreatePollAsync(oAuthToken, clientId, broadcasterId, title, choices, duration,
				channelPointsPerVote, enableChannelPointVoting, bitsPerVote, enableBitsVoting);
		}
		public static async Task EndPollAsync(string oAuthToken, string clientId, string broadcasterId, string pollId, bool archive = false)
		{
			await PollRequests.EndPollAsync(oAuthToken, clientId, broadcasterId, pollId, archive);
		}
		public static async Task<PollResponseModel> GetPoll(string oAuthToken, string clientId, string broadcasterId)
		{
			return await PollRequests.GetPoll(oAuthToken, clientId, broadcasterId);
		}
		public static async Task<PollResponseModel> GetPoll(string oAuthToken, string clientId, string broadcasterId, params string[] pollIds)
		{
			return await PollRequests.GetPoll(oAuthToken, clientId, broadcasterId, pollIds);
		}
		
	}
}
