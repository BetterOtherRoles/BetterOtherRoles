using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebsocketsSimple.Server.Models;

namespace BetterOtherRolesApi;

public class Client
{
    public readonly ConnectionWSServer Connection;
    public WebSocket Socket => Connection.Websocket;

    private DateTime _lastPing;

    public Client(ConnectionWSServer connection)
    {
        Connection = connection;
        _lastPing = DateTime.UtcNow;
    }

    public void Runtime()
    {
        if (_lastPing.TimeOfDay.TotalSeconds + 30f <= DateTime.UtcNow.TimeOfDay.TotalSeconds)
        {
            Socket.CloseAsync(WebSocketCloseStatus.Empty, "", new CancellationToken());
        }
    }

    public bool Is(ConnectionWSServer connection)
    {
        return Connection.ConnectionId == connection.ConnectionId;
    }

    public void OnMessage(string message)
    {
        var msg = JsonSerializer.Deserialize<Message>(message);
        if (msg == null) return;
        InternalOnMessage(msg);
    }

    private async void InternalOnMessage(Message msg)
    {
        if (msg.EventName == "ping")
        {
            _lastPing = DateTime.UtcNow;
            await Send(new Message
            {
                Namespace = msg.Namespace,
                EventName = "pong",
                Content = ""
            });
        }
    }

    public async Task Send(Message msg)
    {
        
    }

    public class Message
    {
        [JsonPropertyName("n")] public string Namespace { get; set; }

        [JsonPropertyName("e")] public string EventName { get; set; }

        [JsonPropertyName("c")] public string Content { get; set; }
    }
}