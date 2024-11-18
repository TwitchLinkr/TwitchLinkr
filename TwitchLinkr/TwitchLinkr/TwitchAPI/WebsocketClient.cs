using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TwitchLinkr.Interfaces;
using TwitchLinkr.TwitchAPI.APIResponseModels;

namespace TwitchLinkr.TwitchAPI
{
	internal class WebsocketClient : IEventSubConnection
	{
		private ClientWebSocket _webSocket = default!;
		private CancellationTokenSource _cancellationTokenSource = default!;

		public delegate void MessageReceivedHandler(WebsocketMessage<NotificationMetadata, NotificationPayload> message);
		public event IEventSubConnection.MessageReceivedHandler MessageReceived = default!;

		public delegate void WebsocketMessageReceivedHandler(WebsocketMessage<NotificationMetadata, NotificationPayload> message);
		public event WebsocketMessageReceivedHandler WebsocketMessageReceived = default!;

		public Queue<DateTime> PingTimes = new Queue<DateTime>();

		private string Uri = default!;

		public string ConnectionType { get; } = "websocket";
		
		public WebsocketClient()
		{
			_webSocket = new ClientWebSocket();
			_cancellationTokenSource = new CancellationTokenSource();
		}


		public async Task ConnectAsync(string uri)
		{

			await _webSocket.ConnectAsync(new Uri(uri), _cancellationTokenSource.Token);

			if (_webSocket.State == WebSocketState.Open)
			{
				Uri = uri;
				_ = Task.Run(() => RecieveMessagesAsync(_cancellationTokenSource.Token));
				return;
			}
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
				await Task.Delay(5000); // Wait 5 seconds before trying to reconnect
				await ReconnectAsync(reconnectAttempts - 1);
			}
		}

		public async Task ReconnectAsync(int reconnectAttempts, string uri)
		{
			await DisconnectAsync();
			await ConnectAsync(uri);

			if (_webSocket.State != WebSocketState.Open && reconnectAttempts > 0)
			{
				await Task.Delay(5000); // Wait 5 seconds before trying to reconnect
				await ReconnectAsync(reconnectAttempts - 1, uri);
			}
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

					continue;
				}

				var serviceMessage = new WebsocketMessage<Metadata, ServicePayload>
												{ Metadata = websocketMessage.Metadata!,
													Payload = (ServicePayload)websocketMessage.Payload!};
				await HandleServiceMessagesAsync(serviceMessage, cancellationToken);
				
			}
		}

		// TODO: Implement service message handling

		private async Task HandleServiceMessagesAsync(WebsocketMessage<Metadata, ServicePayload> message, CancellationToken cancellation)
		{
			
		}

		/// <summary>
		/// Twitch server will close connection if message gets sent. Only use outside Twitch.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public async Task SendMessageAsync(string message)
		{
			var buffer = Encoding.UTF8.GetBytes(message);
			await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		/// <summary>
		/// Twitch server will close connection if message gets sent. Only use outside Twitch.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public async Task SendMessageAsync(byte[] message)
		{
			await _webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		/// <summary>
		/// Twitch server will close connection if message gets sent. Only use outside Twitch.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
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
