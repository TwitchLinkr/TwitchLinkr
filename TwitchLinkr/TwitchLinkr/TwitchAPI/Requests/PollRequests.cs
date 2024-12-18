using System.Text.Json;
using TwitchLinkr.TwitchAPI.APIResponseModels;

namespace TwitchLinkr.TwitchAPI.Requests;

internal static class PollRequests
{
	/// <summary>
	/// Creates a poll on Twitch using the provided OAuth token and client ID. <br/>
	/// Requires a user access token with scope channel:manage:polls.
	/// </summary>
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
	/// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="CreatePollResponseModel"/>.</returns>
	public static async Task<CreatePollResponseModel> CreatePollAsync(string oAuthToken, string clientId, string broadcasterId, string title, string[] choices, int duration, int channelPointsPerVote = 500, bool enableChannelPointVoting = false, int bitsPerVote = 10, bool enableBitsVoting = false)
	{
		const string endpoint = "https://api.twitch.tv/helix/polls";

		if (choices.Length < 2)
		{
			throw new ArgumentOutOfRangeException("The poll must have at least two choices.", nameof(choices));
		} else if (choices.Length > 5)
		{
			throw new ArgumentOutOfRangeException("The poll can have at most five choices.", nameof(choices));
		}


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
		var response = await EndpointCaller.CallPostEndpointAsync(endpoint, oAuthToken, clientId, jsonContent);

		// Deserialize the response
		var pollResponse = JsonSerializer.Deserialize<CreatePollResponseModel>(response);

		// Return the response. Unlikely to be null, since the EndpointCaller will throw an exception if the call is unsuccessful.
		return pollResponse!;
	}

	/// <summary>
	/// Ends a poll on Twitch using the provided OAuth token and client ID.	<br/>
	/// Requires a user access token with scope channel:manage:polls.
	/// </summary>
	/// <param name="oAuthToken">The OAuth token for authorization. Requires scope channel:manage:polls.</param>
	/// <param name="clientId">The client ID of the application.</param>
	/// <param name="broadcasterId">The ID of the broadcaster ending the poll.</param>
	/// <param name="pollId">The ID of the poll to end.</param>
	/// <param name="archive">Whether to archive the poll. Default is false.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
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

	/// <summary>
	/// Gets specific polls for a broadcaster using the provided OAuth token and client ID.	<br/>
	/// Requires a user access token with scope channel:manage:polls or channel:read:polls if you're only reading polls.
	/// </summary>
	/// <param name="oAuthToken">The OAuth token for authorization. Requires scope channel:manage:polls.</param>
	/// <param name="clientId">The client ID of the application.</param>
	/// <param name="broadcasterId">The ID of the broadcaster whose polls to retrieve.</param>
	/// <param name="pollIds">An array of poll IDs to retrieve. Maximum of 20 Ids at a time</param>
	/// <param name="first">The number of polls to retrieve. Maximum of 20 polls at a time.</param>
	/// <param name="cursor">The cursor for the next page of results.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="PollResponseModel"/>.</returns>
	public static async Task<PollResponseModel> GetPollsAsync(string oAuthToken, string clientId, string broadcasterId, string[]? pollIds, int first = 20, string cursor = "")
	{
		const string endpoint = "https://api.twitch.tv/helix/polls";


		// Ensure the first value is within the valid range
		if (first < 1 || first > 20)
		{
			throw new ArgumentOutOfRangeException(nameof(first), "The value of 'first' must be between 1 and 20.");
		}


		// Create parameters
		List<KeyValuePair<string, string>> parameters =
		[
			new ("broadcaster_id", broadcasterId),
			new ("first", first.ToString())
		];

		// Add cursor to parameters
		if (string.IsNullOrEmpty(cursor))
		{
			parameters.Add(new("after", cursor));
		}

		// Add IDs to parameters
		if (pollIds != null && pollIds.Length > 0)
		{
			parameters = parameters.Concat(
									pollIds.Select(id => new KeyValuePair<string, string>("id", id)))
									.ToList();
		}

		// Call the endpoint
		var response = await EndpointCaller.CallGetEndpointAsync(endpoint, oAuthToken, clientId, [.. parameters]);

		// Deserialize the response
		var data = JsonSerializer.Deserialize<PollResponseModel>(response);

		return data!;
	}
}
