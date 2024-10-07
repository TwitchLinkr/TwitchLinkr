namespace TwitchLinkr.TwitchAPI.APIResponseModels;

internal class CreatePollResponseModel
{
	public string Id { get; set; } = default!;
}

internal class PollResponseModel
{
	public Poll[] Data { get; set; } = default!;
	public Dictionary<string, string> Pagination { get; set; } = default!;
}

internal class Poll
{
	public string Id { get; set; } = default!;
	public string BroadcasterId { get; set; } = default!;
	public string BroadcasterName { get; set; } = default!;
	public string BroadcasterLogin { get; set; } = default!;
	public string Title { get; set; } = default!;
	public PollChoice[] Choices { get; set; } = default!;
	public bool BitsVotingEnabled { get; set; } = default!;
	public int BitsPerVote { get; set; } = default!;
	public bool ChannelPointsVotingEnabled { get; set; } = default!;
	public int ChannelPointsPerVote { get; set; } = default!;
	public string Status { get; set; } = default!;
	public int Duration { get; set; } = default!;
	public string StartedAt { get; set; } = default!;
	public DateTime StartedDateTime => DateTime.Parse(StartedAt);
}

internal class PollChoice
{
	public string Id { get; set; } = default!;
	public string Title { get; set; } = default!;
	public int Votes { get; set; } = default!;
	public int ChannelPointsVotes { get; set; } = default!;
	public int BitsVotes { get; set; } = default!;
}