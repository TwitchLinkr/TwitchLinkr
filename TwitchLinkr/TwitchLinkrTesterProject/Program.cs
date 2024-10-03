using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using TwitchLinkr;

namespace TwitchLinkrTesterProject
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			// Set up configuration to read from user secrets
			var configuration = new ConfigurationBuilder()
				.AddUserSecrets<Program>()  // Add user secrets for this project
				.Build();

			string clientId = configuration["TwitchApp:ClientId"]!;
			string clientSecret = configuration["TwitchApp:ClientSecret"]!;


			// Set up a service collection and configure logging
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			// Build the service provider
			var serviceProvider = serviceCollection.BuildServiceProvider();

			// Get the logger from the service provider
			var logger = serviceProvider.GetRequiredService<ILogger<OAuthToken>>();

			// Create an instance of OAuthToken with the logger
			OAuthToken oAuthToken = new OAuthToken(logger);

			// Test OAuthToken methods
			//var token = await oAuthToken.GetClientCredentialsGrantFlowOAuthTokenAsync(clientId, clientSecret);
			//Console.WriteLine("OAuth Token: " + token);

			//oAuthToken.GetImplicitGrantFlowOAuthTokenAsync(clientId, "http://localhost:3000", "");

			//var token = await oAuthToken.GetAuthorizationCodeGrantFlowOAuthTokenAsync(clientId, clientSecret, "http://localhost:3000", "", true);
			//Console.WriteLine("OAuth Token: " + token);

			var token = await oAuthToken.GetDeviceCodeGrantFlowOAuthTokenAsync(clientId, "");
			Console.WriteLine("OAuth Token: " + token.access_token);
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			// Add logging
			services.AddLogging(configure => configure.AddConsole())
					.AddTransient<OAuthToken>();
		}
	}
}
