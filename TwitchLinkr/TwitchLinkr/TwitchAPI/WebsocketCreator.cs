using System.Net.WebSockets;

namespace TwitchLinkr.TwitchAPI
{
	internal class WebsocketCreator
	{
		private ClientWebSocket _webSocket = default!;
		private CancellationTokenSource _cancellationTokenSource = default!;

		public delegate void MessageReceivedHandler(string message);

		public event MessageReceivedHandler MessageReceived = default!;

		public async Task Connect(string url)
		{
			_webSocket = new ClientWebSocket();
			_cancellationTokenSource = new CancellationTokenSource();

			await _webSocket.ConnectAsync(new Uri(url), _cancellationTokenSource.Token);

			await ReceiveMessages();
		}

		public async Task Disconnect()
		{
			_cancellationTokenSource?.Cancel();
			await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
			_webSocket.Dispose();
		}

		private async Task ReceiveMessages()
		{
			var buffer = new byte[1024];

			while (_webSocket.State == WebSocketState.Open)
			{
				var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

				if (result.MessageType == WebSocketMessageType.Text)
				{
					var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
					MessageReceived?.Invoke(message);
				}
			}
		}
	}
}
