using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TwitchLinkr.TwitchAPI.APIResponseModels;

namespace TwitchLinkr.TwitchAPI
{
	internal class EventSubscriber
	{
		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
		{
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
		};

		public static async Task SubscibeToEventAsync(string oAuthToken, string clientId, string type, uint version, EventsubCondition condition, Transport transport)
		{
			const string endpoint = "https://api.twitch.tv/helix/eventsub/subscriptions";

			var content = JsonSerializer.Serialize(new EventsubSubscription(type, version, condition, transport), _jsonOptions);

			var response = await EndpointCaller.CallPostEndpointAsync(endpoint, oAuthToken, clientId, content);

			// Validate response and keep track of subsribed events

		}


	}

	internal class EventsubSubscription(string type, uint version, EventsubCondition condition, Transport transport)
	{
		public string Type { get; set; } = type;
		public uint Version { get; set; } = version;
		public EventsubCondition Condition { get; set; } = condition;
		public Transport Transport { get; set; } = transport;
	}


	public class EventsubCondition
	{

	}
	

	// All events that can be subscribed to
	public enum SubscriptionType
	{
		
	}
}
