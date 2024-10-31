using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLinkr.TwitchAPI.APIResponseModels
{
	internal class EmotesResponseModel
	{

		public Emote[] Data { get; set; } = default!;
		public string Template { get; set; } = default!;
	}

	internal class Emote
	{
		public string Id { get; set; } = default!;
		public string Name { get; set; } = default!;
		public EmoteImages Images { get; set; } = default!;
		public string Tier { get; set; } = default!;
		public string EmoteType { get; set; } = default!;
		public string EmoteSetId { get; set; } = default!;
		public string[] Format { get; set; } = default!;
		public string[] Scale { get; set; } = default!;
		public string[] ThemeMode { get; set; } = default!;
		
	
	
	
	}

	internal class EmoteImages
	{
		public string Url_1x { get; set; } = default!;
		public string Url_2x { get; set; } = default!;
		public string Url_4x { get; set; } = default!;


	}
}
