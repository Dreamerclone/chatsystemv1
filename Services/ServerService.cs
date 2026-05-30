using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using ChatSystem.Data;
using ChatSystem.Models;

namespace ChatSystem.Services;

public class ServerService : IServerService
{
    private readonly IMessageRepository _repository;
    private readonly ConcurrentDictionary<string, StreamWriter> _connectedUsers = new();

    public ServerService(IMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task StartAsync(int port, CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[Server] Started on port {port}...");

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(ct);
                _ = HandleClientAsync(client, ct);
            }
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        await using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream))
        await using (var writer = new StreamWriter(stream) { AutoFlush = true })
        {
            string? currentUsername = null;

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(ct);
                    if (line == null) break;

                    var message = JsonSerializer.Deserialize<Message>(line);
                    if (message == null) continue;

                    // Registration on first message
                    if (currentUsername == null)
                    {
                        currentUsername = message.Sender.Username;
                        if (!_connectedUsers.TryAdd(currentUsername, writer))
                        {
                            // Optional: Handle duplicate username (e.g., append suffix or disconnect)
                            Console.WriteLine($"[Server] User {currentUsername} already connected. Overwriting session.");
                            _connectedUsers[currentUsername] = writer;
                        }
                        Console.WriteLine($"[Server] User registered: {currentUsername}");
                    }

                    await ProcessMessageAsync(message, writer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Error with client {currentUsername}: {ex.Message}");
            }
            finally
            {
                if (currentUsername != null)
                {
                    _connectedUsers.TryRemove(currentUsername, out _);
                    Console.WriteLine($"[Server] User disconnected: {currentUsername}");
                }
            }
        }
    }

    private async Task ProcessMessageAsync(Message message, StreamWriter senderWriter)
    {
        if (message.Type == MessageType.CommandRequest)
        {
            await HandleCommandRequestAsync(message, senderWriter);
            return;
        }

        // Parse for private messages command in regular chat
        if (message.Text.StartsWith("/msg ", StringComparison.OrdinalIgnoreCase))
        {
            await HandlePrivateMessageAsync(message, senderWriter);
        }
        else
        {
            _repository.AddMessage(message);
            await BroadcastAsync(message);
        }
    }

    private async Task HandleCommandRequestAsync(Message message, StreamWriter senderWriter)
    {
        if (message.Text == "LIST_USERS")
        {
            var userList = string.Join(", ", _connectedUsers.Keys);
            var response = new Message
            {
                Type = MessageType.CommandResponse,
                Sender = new User("System"),
                Text = $"Online users: {userList}",
                Timestamp = DateTime.UtcNow
            };
            await senderWriter.WriteLineAsync(JsonSerializer.Serialize(response));
        }
    }

    private async Task HandlePrivateMessageAsync(Message message, StreamWriter senderWriter)
    {
        var parts = message.Text.Split(' ', 3);
        if (parts.Length < 3)
        {
            await SendSystemMessageAsync(senderWriter, "Usage: /msg <username> <message>");
            return;
        }

        string targetUsername = parts[1];
        string content = parts[2];

        message.RecipientUsername = targetUsername;
        message.Text = content;

        if (_connectedUsers.TryGetValue(targetUsername, out var targetWriter))
        {
            var json = JsonSerializer.Serialize(message);
            await targetWriter.WriteLineAsync(json);

            // Also notify the sender (like a 'sent' confirmation)
            // In a real app, the client would handle its own UI for sent messages
            await SendSystemMessageAsync(senderWriter, $"[Private to {targetUsername}]: {content}");

            // Note: Private messages usually aren't stored in a public repository
            // but could be stored in a separate PrivateMessage table.
        }
        else
        {
            await SendSystemMessageAsync(senderWriter, $"User '{targetUsername}' is not online.");
        }
    }

    private async Task BroadcastAsync(Message message)
    {
        var json = JsonSerializer.Serialize(message);
        foreach (var user in _connectedUsers)
        {
            try
            {
                await user.Value.WriteLineAsync(json);
            }
            catch { /* Handled by individual client loop */ }
        }
    }

    private async Task SendSystemMessageAsync(StreamWriter writer, string text)
    {
        var sysMsg = new Message
        {
            Sender = new User("System"),
            Text = text,
            Timestamp = DateTime.UtcNow
        };
        await writer.WriteLineAsync(JsonSerializer.Serialize(sysMsg));
    }
}
