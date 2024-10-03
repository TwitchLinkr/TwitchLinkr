using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using TwitchLinkr;

namespace TwitchLinkrTesterProject;

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

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.GrafanaLoki("http://localhost:3100")
			.CreateLogger();
        
        // Set up a service collection and configure logging
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        
        // Build the service provider
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        // Test endpoints
        var token = await GetToken(serviceProvider, clientId, clientSecret);
		Console.WriteLine(token);
	}


    // Test the GetOAuthTokenAsync method
    static async Task<string> GetToken(IServiceProvider serviceProvider, string clientId, string clientSecret)
    {
        // Get the logger from the service provider
        var logger = serviceProvider.GetRequiredService<ILogger<OAuthToken>>();

        // Create an instance of OAuthToken with the logger
        OAuthToken oAuthToken = new OAuthToken(logger);
        return await oAuthToken.GetClientCredentialsGrantFlowOAuthTokenAsync(clientId, clientSecret);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(configure => configure.AddSerilog())
                .AddTransient<OAuthToken>();
    }
}
