using System.Net.WebSockets;
using System.Text;
using TwitchLinkr.Interfaces;

namespace TwitchLinkr.TwitchAPI
{
	internal class WebsocketClient : IEventSubConnection
	{
		private ClientWebSocket _webSocket = default!;
		private CancellationTokenSource _cancellationTokenSource = default!;

		public delegate void MessageReceivedHandler(string message);

		public event IEventSubConnection.MessageReceivedHandler MessageReceived = default!;

		public Queue<DateTime> PingTimes = new Queue<DateTime>();

		private string Uri = default!;

		public string ConnectionType { get; } = "websocket";

		public async Task ConnectAsync(string uri)
		{
			_webSocket = new ClientWebSocket();
			_cancellationTokenSource = new CancellationTokenSource();

			await _webSocket.ConnectAsync(new Uri(uri), _cancellationTokenSource.Token);

			Uri = uri;

			await RecieveMessagesAsync();
		}

		public async Task DisconnectAsync()
		{
			_cancellationTokenSource?.Cancel();
			await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
			_webSocket.Dispose();
		}

		public async Task ReconnectAsync(int reconnectAttempts)
		{
			await DisconnectAsync();
			await ConnectAsync(Uri);

			if (_webSocket.State != WebSocketState.Open && reconnectAttempts > 0)
			{
				Task.Delay(5000).Wait(); // Wait 5 seconds before trying to reconnect
				await ReconnectAsync(reconnectAttempts - 1);
			}
		}

		public async Task RecieveMessagesAsync()
		{
			var buffer = new byte[1024];

			while (_webSocket.State == WebSocketState.Open)
			{
				var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

				if (result.MessageType == WebSocketMessageType.Text)
				{
					var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
					
					// Check if the message is a PONG by opcode 0xA and has arrived 10 or more seconds after the corresponding PING
					if (buffer[0] == 0xA && PingTimes.Count > 0)
					{
						PingTimes.Dequeue();
					}
					else
					{
						// If the message is not a PONG, invoke the MessageReceived event
						MessageReceived?.Invoke(message);
					}					
				}
			}
		}

		public async Task SendMessageAsync(string message)
		{
			var buffer = Encoding.UTF8.GetBytes(message);
			await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		public async Task SendMessageAsync(byte[] message)
		{
			await _webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		public async Task SendMessageAsync(byte[] message, WebSocketMessageType messageType)
		{
			await _webSocket.SendAsync(new ArraySegment<byte>(message), messageType, true, CancellationToken.None);
		}

		// Ping every 5 seconds 
		public async Task MaintainHeartbeatAsync()
		{
			while (_webSocket.State == WebSocketState.Open)
			{
				await Task.Delay(5000);
				// The ping contains the opcode 0x9 and the message "PING"
				await SendMessageAsync(new byte[] { 0x9, 0x50, 0x49, 0x4E, 0x47 });
				PingTimes.Enqueue(DateTime.Now);

				// If the last PING was sent more than 10 seconds ago, attempt reconnect
				if (PingTimes.Count > 0 && (DateTime.Now - PingTimes.Peek()).TotalSeconds > 10)
				{
					await ReconnectAsync(5);
				}

			}

		}
	}
}
