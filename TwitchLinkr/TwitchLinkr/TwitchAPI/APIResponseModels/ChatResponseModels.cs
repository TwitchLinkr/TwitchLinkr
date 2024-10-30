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
	
}
