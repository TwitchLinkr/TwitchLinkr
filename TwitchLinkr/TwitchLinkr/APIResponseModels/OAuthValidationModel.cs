using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLinkr.APIResponseModels
{
	internal class OAuthValidationModel
	{

		public string client_id { get; set; } = default!;
		public string login { get; set; } = default!;
		public string[] scopes { get; set; } = default!;
		public string user_id { get; set; } = default!;
		public int expires_in { get; set; }

		public DateTime expire_date => DateTime.Now.AddSeconds(expires_in);

	}
}
