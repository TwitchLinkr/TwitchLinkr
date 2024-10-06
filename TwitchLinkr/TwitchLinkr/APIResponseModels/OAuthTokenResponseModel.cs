using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLinkr.APIResponseModels
{
	/// <summary>
	/// Represents the response model for the OAuth token request.
	/// <para>
	/// This Model is used to deserialize the JSON response from the Twitch API when requesting an OAuth token. And is meant for usage with all types of OAuth token requests.
	/// </para>
	/// </summary>
	public class OAuthTokenResponseModel
	{
		/// <summary>
		/// The access token used to authenticate requests to the Twitch API.
		/// </summary>
		public string Access_token { get; set; } = default!;

		/// <summary>
		/// The refresh token used to refresh the access token after expiry.
		/// </summary>
		public string Refresh_token { get; set; } = default!;

		/// <summary>
		/// Specifies the type of token returned.
		/// </summary>
		public string Token_type { get; set; } = default!;

		/// <summary>
		/// The scope of the access token.
		/// </summary>
		public string Scope { get; set; } = default!;

		/// <summary>
		/// The state parameter that was passed in the request.
		/// </summary>
		public string State { get; set; } = default!;

		/// <summary>
		/// The number of seconds until the access token expires.
		/// </summary>
		public int Expires_in { get; set; }

		/// <summary>
		/// The timestamp when the access token was granted.
		/// </summary>
		public DateTime Time_of_grant { get; set; } = DateTime.Now;

		/// <summary>
		/// The timestamp of when the access token expires.
		/// </summary>
		public DateTime Expiration_time => Time_of_grant.AddSeconds(Expires_in);

	}
}
