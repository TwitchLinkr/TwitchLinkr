
using System.Net.WebSockets;
using TwitchLinkr.TwitchAPI.APIResponseModels;

namespace TwitchLinkr.Interfaces
{
	internal interface IEventSubConnection
	{

		public delegate void MessageReceivedHandlerRaw(string message);
		public event MessageReceivedHandlerRaw MessageReceivedRaw;
		public string ConnectionType { get; }
		public bool IsConnected { get; }

		public Transport Transport { get; }
		public Task ConnectAsync(string url);
		public Task DisconnectAsync();
		public Task ReconnectAsync(uint reconnectAttempts);
		public Task ReconnectAsync(uint reconnectAttempts, string uri);
		public Task RecieveMessagesAsync(CancellationToken cancellation);
	}

	public enum ConnectionType
	{
		Websocket,
		WebHook,
		Conduit
	}

}
