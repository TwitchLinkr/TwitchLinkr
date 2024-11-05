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

		// TODO: fully implement test mode
		public bool TestModeEnabled { get; set; } = false;
		private string BaseUri { 
			get
			{
				return TestModeEnabled ? "http://localhost8080/" : "https://api.twitch.tv/";
			}
		}

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

		private AuthorizationUserLinker(string oAuthToken, string refreshToken, string clientId, string clientSecret, bool testMode = false)
		{
			this.OAuthToken = oAuthToken;
			this.RefreshToken = refreshToken;
			this.ClientId = clientId;
			this.ClientSecret = clientSecret;
			this.IsAuthorizedInternal = true;
			this.TestModeEnabled = testMode;
		}

		private AuthorizationUserLinker(string clientId, string clientSecret, bool testMode = false)
		{
			this.ClientId = clientId;
			this.ClientSecret = clientSecret;
			this.OAuthToken = "";
			this.RefreshToken = "";
			this.IsAuthorizedInternal = false;
			this.TestModeEnabled = testMode;
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
			OAuthTokenResponseModel AuthResponse = await Authorizer.GetAuthorizationCodeGrantFlowOAuthTokenAsync(this.ClientId, this.ClientSecret, redirectUri, scopes, force_verify);

			this.OAuthToken = AuthResponse.Access_token;
			this.RefreshToken = AuthResponse.Refresh_token;

			this.IsAuthorizedInternal = true;
			return true;
		}

		public async Task<bool> RefreshTokenAsync()
		{
			var response = await Authorizer.RefreshOAuthTokenAsync(this.ClientId, this.ClientSecret, this.RefreshToken);

			this.OAuthToken = response.Access_token;
			this.RefreshToken = response.Refresh_token;

			this.IsAuthorizedInternal = true;
			return true;
		}

		public async Task<bool> ValidateTokenAsync()
		{
			try
			{
				var response = await Authorizer.ValidateOAuthTokenAsync(OAuthToken);
				IsAuthorizedInternal = true;
				return true;
			} catch (Exception)
			{
				var refreshResult = await RefreshTokenAsync();
				return refreshResult;
			}
			
		}

		public async Task<bool> RevokeTokenAsync()
		{
			try
			{
				await Authorizer.RevokeOAuthTokenAsync(OAuthToken, ClientId);
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
					throw new UnauthorizedException("User is not authorized.");
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
					throw new UnauthorizedException("User is not authorized.");
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

		public async Task<string> CreatePollAsync(string broadcasterId, string title, string[] choices, int duration, int channelPointsPerVote = 500, bool enableChannelPointVoting = false, int bitsPerVote = 10, bool enableBitsVoting = false)
		{

			if (!IsAuthorizedInternal)
			{
				try
				{
					await ValidateTokenAsync();
				}
				catch (Exception)
				{
					throw new UnauthorizedException("User is not authorized.");
				}
			}

			try
			{
				return (await PollRequests.CreatePollAsync(OAuthToken, ClientId, broadcasterId, title, choices, duration, channelPointsPerVote, enableChannelPointVoting, bitsPerVote, enableBitsVoting)).Id;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public async Task EndPollAsync(string broadcasterId, string pollId, bool archive = false)
		{
			if (!IsAuthorizedInternal)
			{
				try
				{
					await ValidateTokenAsync();
				}
				catch (Exception)
				{
					throw new UnauthorizedException("User is not authorized.");
				}
			}

			try
			{
				await PollRequests.EndPollAsync(OAuthToken, ClientId, broadcasterId, pollId, archive);
			}
			catch (Exception)
			{
				throw;
			}
		}

		private class PollPagination
		{
			public string broadcasterId { get; set; } = default!;
			public string[]? pollIds { get; set; } = default!;
			public int First { get; set; } = default!;
			public string Cursor { get; set; } = default!;
		}
		private PollPagination? pollPagination = null;
		public async Task<Poll[]> GetPollsAsync(string broadcasterId, string[]? pollIds, int first = 20, string cursor = "")
		{
			try
			{
				var response = await PollRequests.GetPollsAsync(OAuthToken, ClientId, broadcasterId, pollIds, first, cursor);

				pollPagination = new PollPagination
				{
					broadcasterId = broadcasterId,
					pollIds = pollIds,
					First = first,
					Cursor = response.Pagination.Cursor
				};

				Array.Sort(response.Data, (x, y) => x.StartedDateTime.CompareTo(y.StartedDateTime));

				return response.Data;

			}
			catch (Exception)
			{
				throw;
			}
		}

		public async Task<Poll[]> GetNextPollsAsync()
		{
			if (pollPagination == null)
			{
				throw new NotFoundException("No polls have been retrieved yet.");
			}

			try
			{
				var response = await PollRequests.GetPollsAsync(OAuthToken, ClientId, pollPagination.broadcasterId, pollPagination.pollIds, pollPagination.First, pollPagination.Cursor);

				pollPagination.Cursor = response.Pagination.Cursor;

				Array.Sort(response.Data, (x, y) => x.StartedDateTime.CompareTo(y.StartedDateTime));

				return response.Data;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public async Task<Poll[]> GetAllPollsAsync(string broadcasterId, string[]? pollIds)
		{
			
			if (pollIds != null && pollIds.Length <= 20 && pollIds.Length > 0)
			{
				return await GetPollsAsync(broadcasterId, pollIds);
			}

			List<Poll> allPolls = new List<Poll>();
			
			var polls = await GetPollsAsync(broadcasterId, null);
			allPolls.AddRange(polls);

			while (polls.Length == 20)
			{
				polls = await GetNextPollsAsync();
				allPolls.AddRange(polls);
			}

			allPolls.Sort((x, y) => x.StartedDateTime.CompareTo(y.StartedDateTime));

			return allPolls.ToArray();

		}


	}
}
