
namespace TwitchLinkr.TwitchAPI.APIResponseModels
{
	/// <summary>
	/// Used to deserialize and determine the response type from the Twitch API when establishing a websocket connection.
	/// </summary>
	internal class WebsocketMessageMetadata
	{
		public Metadata Metadata { get; set; } = default!;
	}

	internal class WebsocketMessage<T, X>
		where T : Metadata
		where X : Payload
	{
		public T? Metadata { get; set; } = default!;
		public X? Payload { get; set; } = default!;
	}

	internal class Metadata
	{
		public string MessageId { get; set; } = default!;
		public string MessageType { get; set; } = default!;
		public string MessageTimestamp { get; set; } = default!;

	}

	internal class NotificationMetadata : Metadata
	{
		public string SubscriptionType { get; set; } = default!;
		public string SubscriptionVersion { get; set; } = default!;
	}

	internal class Payload { }

    internal class NotificationPayload : Payload 
    {
        public Subscription Subscription { get; set; } = default!;
    }

	internal class ServicePayload : Payload
	{
		public Session Session { get; set; } = default!;
		public Subscription Subscription { get; set; } = default!;

	}

	internal class Session
    {
		public string Id { get; set; } = default!;
		public string Status { get; set; } = default!;
		public string KeepaliveTimeoutSeconds { get; set; } = default!;
		public string ReconnectUrl { get; set; } = default!;
		public string ConnectedAt { get; set; } = default!;
	}

	internal class Subscription
	{
		public string Id { get; set; } = default!;
		public string Status { get; set; } = default!;
		public string Type { get; set; } = default!;
		public string Version { get; set; } = default!;
		public int Cost { get; set; } = default!;
		public SubscriptionCondition Condition { get; set; } = default!;
		public Transport Transport { get; set; } = default!;
		public string CreatedAt { get; set; } = default!;
	}

	/// <summary>
	/// Temporary class until we know what the condition is
	/// </summary>
	internal class SubscriptionCondition // Temporary class until we know what the condition is
	{
		public string BroadcasterUserId { get; set; } = default!;
	}
	
	internal class Transport
	{
		public string Method { get; set; } = default!;
		public string SessionId { get; set; } = default!;
	}
}
