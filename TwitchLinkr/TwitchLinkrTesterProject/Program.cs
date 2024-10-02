using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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


			// Test endpoints
			var token = await GetToken(clientId, clientSecret);
			Console.WriteLine(token);
		}


		// Test the GetOAuthTokenAsync method
		static async Task<string> GetToken(string clientId, string clientSecret)
		{
			// Set up a service collection and configure logging
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			// Build the service provider
			var serviceProvider = serviceCollection.BuildServiceProvider();

			// Get the logger from the service provider
			var logger = serviceProvider.GetRequiredService<ILogger<OAuthToken>>();

			// Create an instance of OAuthToken with the logger
			OAuthToken oAuthToken = new OAuthToken(logger);
			return await oAuthToken.GetOAuthTokenAsync(clientId, clientSecret);
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			// Add logging
			services.AddLogging(configure => configure.AddConsole())
					.AddTransient<OAuthToken>();
		}
	}
}
