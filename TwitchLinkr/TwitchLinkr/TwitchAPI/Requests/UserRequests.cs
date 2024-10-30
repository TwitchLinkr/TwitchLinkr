using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchLinkr.TwitchAPI.APIResponseModels;

namespace TwitchLinkr.TwitchAPI.Requests
{
	/// <summary>  
	/// Provides methods to interact with Twitch user data.  
	/// </summary>  
	internal static class UserRequests
	{
		/// <summary>  
		/// Gets user information for specified logins and/or IDs.  
		/// Requires an App Access Token or User Access Token.  
		/// To include the verified email of the account linked to the token, the user:read:email scope is required.  
		/// </summary>  
		/// <param name="oAuthToken">The OAuth token for authentication.</param>  
		/// <param name="clientId">The Client ID for authentication.</param>  
		/// <param name="logins">An array of user logins to fetch information for.</param>  
		/// <param name="ids">An array of user IDs to fetch information for.</param>  
		/// <returns>A task that represents the asynchronous operation. The task result contains the user response model.</returns>  
		public static async Task<UserResponseModel> GetUsers(string oAuthToken, string clientId, string[] logins, string[] ids)
		{
			const string endpoint = "https://api.twitch.tv/helix/users";

			var userParams = new List<KeyValuePair<string, string>>();

			if (logins != null && logins.Length > 0)
			{
				foreach (var userName in logins)
				{
					userParams.Add(new KeyValuePair<string, string>("login", userName));
				}
			}

			if (ids != null && ids.Length > 0)
			{
				foreach (var userId in ids)
				{
					userParams.Add(new KeyValuePair<string, string>("id", userId));
				}
			}

			var response = await EndpointCaller.CallGetEndpointAsync(endpoint, oAuthToken, clientId, userParams.ToArray());

			var data = JsonSerializer.Deserialize<UserResponseModel>(response);

			return data!;
		}

		/// <summary>  
		/// Updates the description of the authenticated user.  
		/// Requires a User Access Token with scope user:edit.
		/// To include the verified email of the account linked to the token, the user:read:email scope is required.
		/// </summary>  
		/// <param name="oAuthToken">The OAuth token for authentication.</param>  
		/// <param name="clientId">The Client ID for authentication.</param>  
		/// <param name="descr">The new description for the user.</param>  
		/// <returns>A task that represents the asynchronous operation. The task result contains the user response model.</returns>  
		public static async Task<UserResponseModel> UpdateUser(string oAuthToken, string clientId, string descr)
		{
			const string endpoint = "https://api.twitch.tv/helix/users";

			var response = await EndpointCaller.CallPutEndpointAsync(endpoint, oAuthToken, clientId, new KeyValuePair<string, string>("description", descr));

			var data = JsonSerializer.Deserialize<UserResponseModel>(response);

			return data!;
		}
	}
}
