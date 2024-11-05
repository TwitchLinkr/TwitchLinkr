using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLinkr.TwitchAPI;
using TwitchLinkr.TwitchAPI.APIResponseModels;
using TwitchLinkr.TwitchAPI.Exceptions;
using TwitchLinkr.TwitchAPI.Requests;

namespace TwitchLinkr
{
	public class AuthorizationUserLinker
	{
		// Static variables7
		public static bool TestModeEnabled { get; set; } = false;


		// Local variables
		private string OAuthToken { get; set; }
		private string RefreshToken { get; set; }

		private string ClientId { get; set; }
		private string ClientSecret { get; set; }
		private bool IsAuthorizedInternal { get; set; } = false;

		// public IsAuthorized is a property that returns the value of IsAuthorizedInternal such that it will be read-only
		public bool IsAuthorized
		{
			get
			{
				return IsAuthorizedInternal;
			}
		}

		private AuthorizationUserLinker(string oAuthToken, string refreshToken, string clientId, string clientSecret)
		{
			this.OAuthToken = oAuthToken;
			this.RefreshToken = refreshToken;
			this.ClientId = clientId;
			this.ClientSecret = clientSecret;
			this.IsAuthorizedInternal = true;
		}

		private AuthorizationUserLinker(string clientId, string clientSecret)
		{
			this.ClientId = clientId;
			this.ClientSecret = clientSecret;
			this.OAuthToken = "";
			this.RefreshToken = "";
			this.IsAuthorizedInternal = false;
		}

		public static async Task<AuthorizationUserLinker> CreateAsync(string clientId, string clientSecret, string redirectUri, string scopes, bool force_verify = false)
		{
			var linker = new AuthorizationUserLinker(clientId, clientSecret);
			await linker.AuthorizeAsync(redirectUri, scopes, force_verify);
			return linker;
		}

		public static async Task<AuthorizationUserLinker> CreateWithTokensAsync(string oAuthToken, string refreshToken, string clientId, string clientSecret)
		{
			var linker = new AuthorizationUserLinker(oAuthToken, refreshToken, clientId, clientSecret);
			await linker.ValidateTokenAsync();
			return linker;
		}

		// #########################################################
		// ########         Authorization fuctions         #########
		// #########################################################

		public async Task<bool> AuthorizeAsync(string redirectUri, string scopes, bool force_verify = false)
		{
			OAuthTokenResponseModel AuthResponse = await new OAuthToken().GetAuthorizationCodeGrantFlowOAuthTokenAsync(this.ClientId, this.ClientSecret, redirectUri, scopes, force_verify);

			this.OAuthToken = AuthResponse.Access_token;
			this.RefreshToken = AuthResponse.Refresh_token;

			this.IsAuthorizedInternal = true;
			return true;
		}

		public async Task<bool> RefreshTokenAsync()
		{
			var response = await new OAuthToken().RefreshOAuthTokenAsync(this.ClientId, this.ClientSecret, this.RefreshToken!);

			this.OAuthToken = response.Access_token;
			this.RefreshToken = response.Refresh_token;

			this.IsAuthorizedInternal = true;
			return true;
		}

		public async Task<bool> ValidateTokenAsync()
		{
			try
			{
				var response = await new OAuthToken().ValidateOAuthTokenAsync(OAuthToken);
				IsAuthorizedInternal = true;
				return true;
			} catch (Exception)
			{

				var refreshResult = await RefreshTokenAsync();

				IsAuthorizedInternal = refreshResult;
				return refreshResult;
			}
			
		}

		public async Task<bool> RevokeTokenAsync()
		{
			try
			{
				await new OAuthToken().RevokeOAuthTokenAsync(OAuthToken, ClientId);
				IsAuthorizedInternal = false;
				return true;
			} catch (Exception)
			{
				return false;
			}
		}


		// #########################################################
		// ########         User Linking functions         #########
		// #########################################################

		public async Task<UserResponseModel> GetUsersAsync(string[] logins, string[] ids)
		{
			if (!IsAuthorizedInternal)
			{
				try {
					var validateResult = await ValidateTokenAsync();
				} catch (Exception)
				{
					throw new InvalidOperationException("User is not authorized.");
				}
			}

			try
			{
				return await UserRequests.GetUsersAsync(OAuthToken, ClientId, logins, ids);
			} catch (Exception)
			{
				throw;
			}
		}

		public async Task<UserResponseModel> UpdateUserAsync(string descr)
		{
			if (!IsAuthorizedInternal)
			{
				try
				{
					await ValidateTokenAsync();
				}
				catch (Exception)
				{
					throw new InvalidOperationException("User is not authorized.");
				}
			}

			try
			{
				return await UserRequests.UpdateUserAsync(OAuthToken, ClientId, descr);
			} catch (Exception)
			{
				throw;
			}
		}

		// #########################################################
		// ########             Poll functions             #########
		// #########################################################




	}
}
