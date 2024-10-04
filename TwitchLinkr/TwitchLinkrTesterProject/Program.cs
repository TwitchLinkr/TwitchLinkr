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
            
            // Hardcoded client ID and client secret
            string clientId = "your-client-id";
            string clientSecret = "your-client-secret";

            // Set up a service collection and configure logging
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Build the service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Test endpoints
            var token = await GetToken(serviceProvider, clientId, clientSecret);
            Console.WriteLine(token);
            
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

        // Test the GetOAuthTokenAsync method
        static async Task<string> GetToken(IServiceProvider serviceProvider, string clientId, string clientSecret)
        {
            // Get the logger from the service provider
            var logger = serviceProvider.GetRequiredService<ILogger<OAuthToken>>();

            // Create an instance of OAuthToken with the logger
            OAuthToken oAuthToken = new OAuthToken(logger);
            return await oAuthToken.GetClientCredentialsGrantFlowOAuthTokenAsync(clientId, clientSecret);
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