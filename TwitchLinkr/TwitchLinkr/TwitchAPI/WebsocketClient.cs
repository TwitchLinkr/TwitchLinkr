using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TwitchLinkr.Interfaces;
using TwitchLinkr.TwitchAPI.APIResponseModels;

namespace TwitchLinkr.TwitchAPI
{
	internal class WebsocketClient : IEventSubConnection
	{

		private readonly  ClientWebSocket _webSocket = default!;

		// CancellationTokenSource is used to cancel actions in the connection
		private readonly CancellationTokenSource _cancellationTokenSource = default!;

		// Subscribable message events containing metadata and payload
		public delegate void MessageReceivedHandler(WebsocketMessage<NotificationMetadata, NotificationPayload> message);
		public event IEventSubConnection.MessageReceivedHandlerRaw MessageReceivedRaw = default!;

		public delegate void WebsocketMessageReceivedHandler(WebsocketMessage<NotificationMetadata, NotificationPayload> message);
		public event WebsocketMessageReceivedHandler WebsocketMessageReceived = default!;

		// Queue to keep track of PING times
		private readonly Queue<DateTime> PingTimes = new Queue<DateTime>();

		public WebSocketState State = WebSocketState.Closed;
		public bool IsConnected => State == WebSocketState.Open;

		private string ConnectionId = default!;

		private string Uri = default!;

		public string ConnectionType { get; } = "websocket";
		
		public Transport Transport { get; } = default!;

		public WebsocketClient()
		{
			_webSocket = new ClientWebSocket();
			_cancellationTokenSource = new CancellationTokenSource();
			Transport = new Transport { Method = "websocket", SessionId = ConnectionId };
		}


		public async Task ConnectAsync(string uri)
		{
			State = WebSocketState.Connecting;
			await _webSocket.ConnectAsync(new Uri(uri), _cancellationTokenSource.Token);

			if (_webSocket.State == WebSocketState.Open)
			{
				Uri = uri;
				_ = Task.Run(() => RecieveMessagesAsync(_cancellationTokenSource.Token));
				return;
			} 
			else
			{
				await Task.Delay(5000); // Wait 5 seconds before trying to reconnect
				await ConnectAsync(uri);
			}
		}
		public async Task DisconnectAsync()
		{
			State = WebSocketState.Closed;
			_cancellationTokenSource?.Cancel();
			await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
			_webSocket.Dispose();
		}
		public async Task ReconnectAsync(uint reconnectAttempts)
		{
			if (reconnectAttempts == 0)
			{
				throw new WebSocketException("Failed to reconnect");
			}

			await DisconnectAsync();
			await ConnectAsync(Uri);

			if (_webSocket.State == WebSocketState.Open)
			{
				return;
			}


			await Task.Delay(5000); // Wait 5 seconds before trying to reconnect
			await ReconnectAsync(reconnectAttempts - 1);
		}
		public async Task ReconnectAsync(uint reconnectAttempts, string uri)
		{
			if (reconnectAttempts == 0)
			{
				throw new WebSocketException("Failed to reconnect");
			}

			await DisconnectAsync();
			await ConnectAsync(uri);

			if (_webSocket.State == WebSocketState.Open)
			{
				return;
			}


			await Task.Delay(5000); // Wait 5 seconds before trying to reconnect
			await ReconnectAsync(reconnectAttempts - 1, uri);
		}
		public async Task RecieveMessagesAsync(CancellationToken cancellationToken)
		{
			var buffer = new byte[1024];

			while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
			{
				var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

				if (result.MessageType != WebSocketMessageType.Text) continue;

				var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
					
				// Check if the message is a PONG by opcode 0xA and has arrived 10 or more seconds after the corresponding PING
				if (buffer[0] == 0xA && PingTimes.Count > 0)
				{
					PingTimes.Dequeue();
					continue;
				}
				
				WebsocketMessage<Metadata, Payload> websocketMessage = DeserializeResponse<WebsocketMessage<Metadata, Payload>>(message);

				if (websocketMessage.Metadata!.MessageType == "notification")
				{
					var notificationMessage = new WebsocketMessage<NotificationMetadata, NotificationPayload> 
													{ Metadata = (NotificationMetadata) websocketMessage.Metadata!, 
														Payload = (NotificationPayload) websocketMessage.Payload!};
					// If the message is not a service, invoke the MessageReceived event
					WebsocketMessageReceived?.Invoke(notificationMessage);
					MessageReceivedRaw?.Invoke(message);
					continue;
				}

				var serviceMessage = new WebsocketMessage<Metadata, ServicePayload>
												{ Metadata = websocketMessage.Metadata!,
													Payload = (ServicePayload)websocketMessage.Payload!};
				await HandleServiceMessagesAsync(serviceMessage, cancellationToken);
				
			}
		}
		private async Task HandleServiceMessagesAsync(WebsocketMessage<Metadata, ServicePayload> message, CancellationToken cancellation)
		{
			switch (message.Metadata!.MessageType)
			{
				case "session_welcome":
					ConnectionId = message.Payload!.Session.Id!;
					State = WebSocketState.Open;
					break;
				case "session_keepalive":
					break;
				case "session_reconnect":
					await ReconnectAsync(5, message.Payload!.Session.ReconnectUrl!);
					break;
				case "session_revocation":
					await DisconnectAsync();
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Twitch server will close connection if message gets sent. Only use outside Twitch.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private async Task SendMessageAsync(string message)
		{
			var buffer = Encoding.UTF8.GetBytes(message);
			await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		/// <summary>
		/// Twitch server will close connection if message gets sent. Only use outside Twitch.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private async Task SendMessageAsync(byte[] message)
		{
			await _webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		/// <summary>
		/// Twitch server will close connection if message gets sent. Only use outside Twitch.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private async Task SendMessageAsync(byte[] message, WebSocketMessageType messageType)
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

		private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			Converters = { new WebsocketMessageConverter() }
		};

		public static T DeserializeResponse<T>(string response)
		{
			return JsonSerializer.Deserialize<T>(response, JsonSerializerOptions)!;
		}
	}
}
