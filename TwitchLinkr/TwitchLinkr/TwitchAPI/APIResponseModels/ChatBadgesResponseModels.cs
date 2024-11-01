using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLinkr.TwitchAPI.APIResponseModels
{
	internal class ChatBadgesResponseModel
	{

		public Badge[] Data { get; set; } = default!;

	}

	internal class Badge
	{

		public string SetId { get; set; } = default!;
		public BadgeVersions[] Versions { get; set; } = default!;

	}

	internal class BadgeVersions
	{
		public string Id { get; set; } = default!;
		public string ImageUrl1x { get; set; } = default!;
		public string ImageUrl2x { get; set; } = default!;
		public string ImageUrl4x { get; set; } = default!;
		public string Description { get; set; } = default!;
		public string ClickAction { get; set; } = default!;
		public string ClickUrl { get; set; } = default!;

	}
}
