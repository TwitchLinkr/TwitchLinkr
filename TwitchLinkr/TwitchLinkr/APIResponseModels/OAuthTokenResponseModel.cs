using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLinkr.APIResponseModels
{
	/// <summary>
	/// Represents the response model for the OAuth token request.
	/// </summary>
	internal class OAuthTokenResponseModel
	{
		/// <summary>
		/// The access token used to authenticate requests to the Twitch API.
		/// </summary>
		public string access_token { get; set; } = default!;

		/// <summary>
		/// The refresh token used to refresh the access token after expiry.
		/// </summary>
		public string refresh_token { get; set; } = default!;

		/// <summary>
		/// Specifies the type of token returned.
		/// </summary>
		public string token_type { get; set; } = default!;

		/// <summary>
		/// The scope of the access token.
		/// </summary>
		public string scope { get; set; } = default!;

		/// <summary>
		/// The number of seconds until the access token expires.
		/// </summary>
		public int expires_in { get; set; }

		/// <summary>
		/// The timestamp when the access token was granted.
		/// </summary>
		public DateTime time_of_grant { get; set; } = DateTime.Now;

		/// <summary>
		/// The timestamp of when the access token expires.
		/// </summary>
		public DateTime expiration_time => time_of_grant.AddSeconds(expires_in);

	}
}
