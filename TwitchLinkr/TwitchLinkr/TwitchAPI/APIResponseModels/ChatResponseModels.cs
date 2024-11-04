using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLinkr.TwitchAPI.APIResponseModels
{
	internal class GetChattersResponseModel
	{
		public Chatter[] Data { get; set; } = default!;
		public Pagination Pagination { get; set; } = default!;
		public int Total { get; set; } = default!;
	}

	internal class Chatter
	{
		public string UserId { get; set; } = default!;
		public string UserLogin { get; set; } = default!;
		public string UserName { get; set; } = default!;
	}

	internal class ChatSettingsResponseModel
	{
		public ChatSettings[] Data { get; set; } = default!;
	}

	internal class ChatSettings
	{
		public string BroadcasterId { get; set; } = default!;
		public bool SlowMode { get; set; } = default!;
		public int SlowModeWaitTime { get; set; } = default!;
		public bool FollowerMode { get; set; } = default!;
		public int FollowerModeDuration { get; set; } = default!;
		public bool SubscriberMode { get; set; } = default!;
		public bool EmoteMode { get; set; } = default!;
		public bool UniqueChatMode { get; set; } = default!;
		public bool NonModeratorChatDelay { get; set; } = default!;
		public int NonModeratorChatDelayDuration { get; set; } = default!;
	
}
