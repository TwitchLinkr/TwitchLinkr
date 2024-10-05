using Microsoft.Extensions.Logging;
using System.Text.Json;
using TwitchLinkr.APIResponseModels;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using static System.Net.WebRequestMethods;

namespace TwitchLinkr.TwitchAPI
{
    /// <summary>
    /// Provides methods to obtain OAuth tokens for Twitch API requests.
    /// <see href="https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#client-credentials-grant-flow">Twitch OAuth Grant Flow documentation</see>
    /// </summary>
    internal class OAuthToken
    {
        private readonly ILogger<OAuthToken> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthToken"/> class.
        /// </summary>
        /// <param name="logger">The logger to use for logging information and errors.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="logger"/> parameter is null.</exception>
        public OAuthToken(ILogger<OAuthToken> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Opens the default web browser to the Twitch OAuth authorization URL for the implicit grant flow.
        /// This does not retrieve the token directly, as the token is available in the redirect URI fragment after user authorization, making the client-side application responsible for extracting the token.
        /// <para>
        /// Use this flow if your app does not use a server. For example, use this flow if your app is a client-side JavaScript app or mobile app.
        /// </para>
        /// </summary>
        /// <param name="clientId">The client ID of your Twitch application.</param>
        /// <param name="redirectUri">The URI to which the user will be redirected after authorization.</param>
        /// <param name="scopes">The scope of the access request.</param>
        /// <param name="state">A unique string to be passed back upon completion. Used to prevent CSRF attacks. If not provided, a new GUID will be generated.</param>
        /// <param name="force_verify">A boolean indicating whether to force the user to re-approve the app. Defaults to <c>false</c>.</param>
        /// <remarks>
        /// Token type: User Access Token
        /// </remarks>
        public void GetImplicitGrantFlowOAuthTokenAsync(string clientId, string redirectUri, string scopes, string state = "", bool force_verify = false)
        {
            // Add a state parameter to prevent CSRF attacks
            if (string.IsNullOrEmpty(state))
            {
                state = Guid.NewGuid().ToString();
            }

            // Construct the URL with the required parameters
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["client_id"] = clientId;
            queryParams["redirect_uri"] = redirectUri;
            queryParams["response_type"] = "token";
            queryParams["scope"] = scopes;
            queryParams["state"] = state;
            queryParams["force_verify"] = force_verify.ToString().ToLower();

            var url = $"https://id.twitch.tv/oauth2/authorize?{queryParams}";

            // Log the URL for debugging purposes
            logger.LogInformation("Navigate to the following URL to authorize the application: {Url}", url);

            // Optionally, open the URL in the default web browser
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to open the URL in the default web browser.");
            }

            // Since this is a client-side flow, the method cannot directly retrieve the token.
            // The token will be available in the redirect URI fragment after user authorization.
            // The client-side application should handle the redirection and extract the token.

            return;
        }

        /// <summary>
        /// Asynchronously retrieves an OAuth token using the client credentials grant flow.
        /// <para>
        /// Use this flow if your app uses a server, can securely store a client secret, 
        /// and can make server-to-server requests to the Twitch API.
        /// This flow is meant for apps that only need an app access token.
        /// </para>
        /// </summary>
        /// <param name="clientId">The client ID of your Twitch application.</param>
        /// <param name="clientSecret">The client secret of your Twitch application.</param>
        /// <remarks>
        /// Token type: App Access Token
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the access token as a string among other details within the <see cref="OAuthTokenResponseModel"/>.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the access token cannot be retrieved.</exception>
        public async Task<OAuthTokenResponseModel> GetClientCredentialsGrantFlowOAuthTokenAsync(string clientId, string clientSecret)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://id.twitch.tv/oauth2/token");

            var parameters = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "grant_type", "client_credentials" }
                };

            var content = new FormUrlEncodedContent(parameters);
            request.Content = content;

            try
            {
                string responseBody = await EndpointCaller.CallAPIAsync(request);
                var tokenData = JsonSerializer.Deserialize<OAuthTokenResponseModel>(responseBody);

                if (tokenData == null || string.IsNullOrEmpty(tokenData.access_token))
                {
                    logger.LogError("Failed to retrieve access token. Response: {ResponseBody}", responseBody);
                    throw new InvalidOperationException("Failed to retrieve access token.");
                }

                return tokenData;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving the OAuth token.");
                logger.LogInformation("The failed OAuth Request: {Request}", request);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves an OAuth token using the authorization code grant flow.
        /// <para>
        /// Use this flow if your app uses a server, can securely store a client secret, and can make server-to-server requests to the Twitch API.
        /// </para>
        /// </summary>
        /// <param name="clientId">The client ID of your Twitch application.</param>
        /// <param name="clientSecret">The client secret of your Twitch application.</param>
        /// <param name="redirectUri">The URI to which the user will be redirected after authorization.</param>
        /// <param name="scopes">The scope of the access request.</param>
        /// <param name="force_verify">A boolean indicating whether to force the user to re-approve the app. Defaults to <c>false</c>.</param>
        /// <remarks>
        /// Token type: User Access Token
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the access token as a string among other details within the <see cref="OAuthTokenResponseModel"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the state does not match or the access token cannot be retrieved.</exception>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        public async Task<OAuthTokenResponseModel> GetAuthorizationCodeGrantFlowOAuthTokenAsync(string clientId, string clientSecret, string redirectUri, string scopes, bool force_verify = false)
        {
            var state = Guid.NewGuid().ToString();

            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["client_id"] = clientId;
            queryParams["redirect_uri"] = redirectUri;
            queryParams["response_type"] = "code";
            queryParams["scope"] = scopes;
            queryParams["state"] = state;
            queryParams["force_verify"] = force_verify.ToString().ToLower();

            var authorizationUrl = $"https://id.twitch.tv/oauth2/authorize?{queryParams}";

            // Log the URL for debugging purposes
            logger.LogInformation("Navigate to the following URL to authorize the application: {Url}", authorizationUrl);

            // Optionally, open the URL in the default web browser
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = authorizationUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to open the URL in the default web browser.");
                throw;
            }

            // Step 2: Use the authorization code to get a token
            // This part assumes you have a way to capture the authorization code from the redirect URI
            string authorizationCode = await CaptureAuthorizationCodeAsync(redirectUri, state);

            var tokenRequestParams = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "code", authorizationCode },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", redirectUri }
                };

            var tokenRequestContent = new FormUrlEncodedContent(tokenRequestParams);
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://id.twitch.tv/oauth2/token")
            {
                Content = tokenRequestContent
            };

            try
            {
                string tokenResponseBody = await EndpointCaller.CallAPIAsync(tokenRequest);
                var tokenData = JsonSerializer.Deserialize<OAuthTokenResponseModel>(tokenResponseBody);

                if (tokenData == null || string.IsNullOrEmpty(tokenData.access_token))
                {
                    logger.LogError("Failed to retrieve access token. Response: {ResponseBody}", tokenResponseBody);
                    throw new InvalidOperationException("Failed to retrieve access token.");
                }

                return tokenData;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving the OAuth token.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves an OAuth token using the device code grant flow.
        /// <para>
        /// Use this flow if your app is run on a client with limited input capabilities, such as set-top boxes or video games.
        /// </para>
        /// </summary>
        /// <param name="clientId">The client ID of your Twitch application.</param>
        /// <param name="scopes">The scope of the access request.</param>
        /// <remarks>
        /// Token type: User Access Token
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the access token as a string among other details within the <see cref="OAuthTokenResponseModel"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the device code cannot be retrieved or the access token cannot be retrieved.</exception>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        public async Task<OAuthTokenResponseModel> GetDeviceCodeGrantFlowOAuthTokenAsync(string clientId, string scopes)
        {
            var queryParams = new Dictionary<string, string> {
                { "client_id", clientId},
                { "scopes", scopes    }
            };

            var deviceRequestContent = new FormUrlEncodedContent(queryParams);
            var deviceRequest = new HttpRequestMessage(HttpMethod.Post, "https://id.twitch.tv/oauth2/device")
            {
                Content = deviceRequestContent
            };

            try
            {

                string deviceResponseBody = await EndpointCaller.CallAPIAsync(deviceRequest);
                var deviceData = JsonSerializer.Deserialize<DeviceCodeResponseModel>(deviceResponseBody);

                if (deviceData == null || string.IsNullOrEmpty(deviceData.device_code))
                {
                    logger.LogError("Failed to retrieve device code. Response: {ResponseBody}", deviceResponseBody);
                    throw new InvalidOperationException("Failed to retrieve device code.");
                }

                logger.LogInformation("Navigate to the following URL to authorize the application: {Url}", deviceData.verification_uri);

                // Poll the token endpoint until the user has authorized the app
                return await PollDeviceCodeGrantFlowTokenAsync(clientId, scopes, deviceData.device_code); ;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving the OAuth token.");
                throw;
            }
        }

        /// <summary>
        /// Polls the token endpoint until the user has authorized the app using the device code grant flow.
        /// </summary>
        /// <param name="clientId">The client ID of your Twitch application.</param>
        /// <param name="scopes">The scope of the access request.</param>
        /// <param name="deviceCode">The device code received from the device code grant flow.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the access token as a string.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the access token cannot be retrieved.</exception>
        private async Task<OAuthTokenResponseModel> PollDeviceCodeGrantFlowTokenAsync(string clientId, string scopes, string deviceCode)
        {
            var tokenEndpoint = "https://id.twitch.tv/oauth2/token";
            var parameters = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "device_code", deviceCode },
                { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" },
                { "scopes", scopes }
            };

            while (true)
            {
                var content = new FormUrlEncodedContent(parameters);
                var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
                {
                    Content = content
                };

                try
                {
                    string responseBody = await EndpointCaller.CallAPIAsync(request);
                    var tokenData = JsonSerializer.Deserialize<OAuthTokenResponseModel>(responseBody);

                    if (tokenData != null && !string.IsNullOrEmpty(tokenData.access_token))
                    {
                        return tokenData;
                    }

                    // Handle specific error responses if needed
                    // For example, if the response indicates that the user has not yet authorized the device, you might want to wait and retry
                }
                catch (HttpRequestException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        // Handle 400 Bad Request specifically, which might indicate that the device code is not yet authorized
                        logger.LogWarning("The device code has not yet been authorized. Retrying in 5 seconds.");
                        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay as needed
                        continue;
                    }
                    logger.LogError(ex, "An error occurred while polling the OAuth token.");
                    throw;
                }

                // Wait for the interval specified by the device code response before polling again
                await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay as needed
            }
        }

        /// <summary>
        /// Captures the authorization code from the redirect URI.
        /// </summary>
        /// <param name="redirectUri">The URI to which the user will be redirected after authorization.</param>
        /// <param name="state">The state parameter to prevent CSRF attacks.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the authorization code as a string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the state does not match.</exception>
        private async Task<string> CaptureAuthorizationCodeAsync(string redirectUri, string state)
        {
            // Start the local web server
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(redirectUri)
                .UseStartup<Startup>()
                .Build();

            await host.StartAsync();

            // Wait for the response from the authorization server
            var res = await GrantFlowController.WaitForAuthorizationCodeAsync();

            // Stop the web server
            await host.StopAsync();

            // check if the state matches
            if (res.state != state)
            {
                logger.LogError("State does not match. Expected: {ExpectedState}, Received: {ReceivedState}", state, res.state);
                throw new InvalidOperationException("State does not match.");
            }

            return res.code;
        }
    }

    /// <summary>
    /// Represents the response model for the device code grant flow.
    /// </summary>
    internal class DeviceCodeResponseModel
    {
        /// <summary>
        /// Gets or sets the device code.
        /// </summary>
        public string device_code { get; set; } = default!;

        /// <summary>
        /// Gets or sets the user code.
        /// </summary>
        public string user_code { get; set; } = default!;

        /// <summary>
        /// Gets or sets the verification URI.
        /// </summary>
        public string verification_uri { get; set; } = default!;

        /// <summary>
        /// Gets or sets the expiration time in seconds.
        /// </summary>
        public int expires_in { get; set; }

        /// <summary>
        /// Gets or sets the interval in seconds at which the client should poll the token endpoint.
        /// </summary>
        public int interval { get; set; }
    }
}