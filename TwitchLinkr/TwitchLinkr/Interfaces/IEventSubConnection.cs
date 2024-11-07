
using System.Net.WebSockets;

namespace TwitchLinkr.Interfaces
{
	internal interface IEventSubConnection
	{

		public delegate void MessageReceivedHandler(string message);
		public event MessageReceivedHandler MessageReceived;
		public string ConnectionType { get; }

		public Task ConnectAsync(string url);
		public Task DisconnectAsync();
		public Task RecieveMessagesAsync();

	}

	public enum ConnectionType
	{
		Websocket,
		WebHook,
		Conduit
	}

}
