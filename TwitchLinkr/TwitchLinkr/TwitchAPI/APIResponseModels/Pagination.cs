using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLinkr.TwitchAPI.APIResponseModels
{
	internal class Pagination
	{
		public string Cursor { get; set; } = default!;
	}

	internal enum PageDirection
	{
		Before,
		After
	}
}
