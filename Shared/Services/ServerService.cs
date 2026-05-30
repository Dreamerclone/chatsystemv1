using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using ChatSystem.Data;
using ChatSystem.Models;
using BCrypt.Net;

namespace ChatSystem.Services;

public class ServerService : IServerService
{
    private readonly IMessageRepository _repository;
    private readonly IDatabaseService _dbService;
    private readonly ConcurrentDictionary<string, StreamWriter> _connectedUsers = new();

    public ServerService(IMessageRepository repository, IDatabaseService dbService)
    {
        _repository = repository;
        _dbService = dbService;
    }

    public async Task StartAsync(int port, CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        while (!ct.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(ct);
            _ = HandleClientAsync(client, ct);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        {
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            await using var writer = new StreamWriter(stream) { AutoFlush = true };

            string? currentUsername = null;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(ct);
                    if (line == null) break;

                    var message = JsonSerializer.Deserialize<Message>(line);
                    if (message == null) continue;

                    if (message.Type == MessageType.RegisterRequest)
                    {
                        await HandleRegister(message, writer);
                        continue;
                    }

                    if (message.Type == MessageType.LoginRequest)
                    {
                        if (await HandleLogin(message, writer))
                        {
                            currentUsername = message.Sender.Username;
                            _connectedUsers[currentUsername] = writer;
                        }
                        continue;
                    }

                    if (currentUsername != null) await ProcessMessageAsync(message, writer);
                }
            }
            catch { }
            finally
            {
                if (currentUsername != null) _connectedUsers.TryRemove(currentUsername, out _);
            }
        }
    }

    private async Task<bool> HandleLogin(Message msg, StreamWriter writer)
    {
        using var conn = _dbService.CreateConnection();
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT PasswordHash FROM Users WHERE Username = @u";
        cmd.Parameters.AddWithValue("@u", msg.Sender.Username);

        var hash = await cmd.ExecuteScalarAsync() as string;
        bool success = hash != null && BCrypt.Net.BCrypt.Verify(msg.Sender.Password, hash);

        await writer.WriteLineAsync(JsonSerializer.Serialize(new Message {
            Type = MessageType.AuthResponse,
            Success = success,
            Text = success ? "Login successful" : "Invalid username or password"
        }));
        return success;
    }

    private async Task HandleRegister(Message msg, StreamWriter writer)
    {
        using var conn = _dbService.CreateConnection();
        await conn.OpenAsync();

        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @u";
        checkCmd.Parameters.AddWithValue("@u", msg.Sender.Username);

        if ((long)(await checkCmd.ExecuteScalarAsync() ?? 0) > 0)
        {
            await writer.WriteLineAsync(JsonSerializer.Serialize(new Message { Type = MessageType.AuthResponse, Success = false, Text = "User already exists" }));
            return;
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(msg.Sender.Password);
        var insCmd = conn.CreateCommand();
        insCmd.CommandText = "INSERT INTO Users (Username, PasswordHash) VALUES (@u, @p)";
        insCmd.Parameters.AddWithValue("@u", msg.Sender.Username);
        insCmd.Parameters.AddWithValue("@p", hash);
        await insCmd.ExecuteNonQueryAsync();

        await writer.WriteLineAsync(JsonSerializer.Serialize(new Message { Type = MessageType.AuthResponse, Success = true, Text = "Registration successful" }));
    }

    private async Task ProcessMessageAsync(Message message, StreamWriter senderWriter)
    {
        if (message.Type == MessageType.CommandRequest) { await HandleCommandRequestAsync(message, senderWriter); return; }
        if (message.Text != null && message.Text.StartsWith("/msg ")) { await HandlePrivateMessageAsync(message, senderWriter); }
        else { _repository.AddMessage(message); await BroadcastAsync(message); }
    }

    private async Task HandleCommandRequestAsync(Message message, StreamWriter senderWriter)
    {
        if (message.Text == "LIST_USERS")
        {
            var response = new Message { Type = MessageType.CommandResponse, Sender = new User("System"), Text = $"Online: {string.Join(", ", _connectedUsers.Keys)}" };
            await senderWriter.WriteLineAsync(JsonSerializer.Serialize(response));
        }
    }

    private async Task HandlePrivateMessageAsync(Message message, StreamWriter senderWriter)
    {
        var parts = message.Text.Split(' ', 3);
        if (parts.Length < 3) return;
        message.RecipientUsername = parts[1];
        message.Text = parts[2];
        if (_connectedUsers.TryGetValue(parts[1], out var target)) {
            await target.WriteLineAsync(JsonSerializer.Serialize(message));
        }
    }

    private async Task BroadcastAsync(Message message)
    {
        var json = JsonSerializer.Serialize(message);
        foreach (var user in _connectedUsers.Values) await user.WriteLineAsync(json);
    }
}
