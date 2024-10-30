using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLinkr.TwitchAPI.APIResponseModels
{
	internal class RaidResponseModels
	{
		public RaidResponseDataContent[] Data { get; set; } = default!;
	}

    internal class RaidResponseDataContent
    {
		string created_at { get; set; } = default!;
		public bool is_mature { get; set; } = default!;
		public DateTime CreatedAtDateTime => DateTime.Parse(created_at);
    }
}
