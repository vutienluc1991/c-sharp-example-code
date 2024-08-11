using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static ClientWebSocket _webSocket;
    private static Uri _serverUri = new Uri("ws://example.com/socket"); // Replace with your WebSocket server URL
    private static int _reconnectInterval = 5000; // 5 seconds

    static async Task Main(string[] args)
    {
        await ConnectWebSocketAsync();

        // Send an example message
        await SendMessageAsync("Hello WebSocket!");

        // Standby and wait for messages or unexpected closure
        await ReceiveMessagesAsync();

        // Reconnect if connection unexpectedly closes
        while (true)
        {
            if (_webSocket.State == WebSocketState.Closed)
            {
                Console.WriteLine("WebSocket closed unexpectedly. Reconnecting...");
                await Task.Delay(_reconnectInterval);
                await ConnectWebSocketAsync();
                await SendMessageAsync("Reconnected!");
            }
        }
    }

    private static async Task ConnectWebSocketAsync()
    {
        _webSocket = new ClientWebSocket();

        try
        {
            await _webSocket.ConnectAsync(_serverUri, CancellationToken.None);
            Console.WriteLine("Connected to WebSocket server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to WebSocket: {ex.Message}");
        }
    }

    private static async Task SendMessageAsync(string message)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(messageBytes);

            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Sent: {message}");
        }
        else
        {
            Console.WriteLine("WebSocket is not open. Cannot send message.");
        }
    }

    private static async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024];

        try
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
        }
    }
}
