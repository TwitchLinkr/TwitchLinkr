using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.Grafana.Loki;
using TwitchLinkr;

namespace TwitchLinkrTesterProject;

internal class Program
{
    static async Task Main(string[] args)
    {
        // Enable Serilog self-logging for debugging purposes
        SelfLog.Enable(Console.Error);

        // Configure Serilog to log to the console and Grafana Loki
		// Could be replaced with a configuration file
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}")
			.WriteTo.GrafanaLoki("http://localhost:3100", labels: new[] 
            { 
				// Adds these labels to all log events
                new LokiLabel { Key = "app", Value = "TwitchLinkrTesterProject" },
            })
			// Adds this property to all log events
            .Enrich.WithProperty("Application", "TwitchLinkrTesterProject")
            .CreateLogger();
        
        try
        {
            // Start Logging
            Log.Information("Starting application");
            
            // Set up configuration to read from user secrets
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()  // Add user secrets for this project
                .Build();
            
            // Get the client id and secret from the environment variables
            string clientId = configuration["TwitchApp:ClientId"]!;
            string clientSecret = configuration["TwitchApp:ClientSecret"]!;

            // Set up a service collection and configure logging
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Build the service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            // Get the logger
            // var logger = serviceProvider.GetRequiredService<ILogger<OAuthToken>>();
            
            // Create a new OAuthToken instance
            // OAuthToken oAuthToken = new OAuthToken(logger);
            
            // Test OAuthToken methods
            // var token = await oAuthToken.GetClientCredentialsGrantFlowOAuthTokenAsync(clientId, clientSecret);
            // Console.WriteLine("OAuth Token: " + token);

            // oAuthToken.GetImplicitGrantFlowOAuthTokenAsync(clientId, "http://localhost:3000", "");
               
            // var token = await oAuthToken.GetAuthorizationCodeGrantFlowOAuthTokenAsync(clientId, clientSecret, "http://localhost:3000", "", true);
            // Console.WriteLine("OAuth Token: " + token);
               
            // var token = await oAuthToken.GetDeviceCodeGrantFlowOAuthTokenAsync(clientId, "");
            // Console.WriteLine("OAuth Token: " + token.access_token);
        
            
            Log.Information("Application completed successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }

        static void ConfigureServices(IServiceCollection services)
        {
            // Add logging
            services.AddLogging(configure => configure
                .ClearProviders()
                .AddSerilog(dispose: true));
        }
    }
}