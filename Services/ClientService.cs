using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using ChatSystem.Models;

namespace ChatSystem.Services;

public class ClientService : IClientService
{
    private TcpClient? _client;
    private StreamWriter? _writer;
    private CancellationTokenSource? _cts;

    public event Action<Message>? MessageReceived;

    public async Task ConnectAsync(string host, int port)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(host, port);

        var stream = _client.GetStream();
        _writer = new StreamWriter(stream) { AutoFlush = true };
        _cts = new CancellationTokenSource();

        _ = ReceiveLoopAsync(new StreamReader(stream), _cts.Token);
    }

    private async Task ReceiveLoopAsync(StreamReader reader, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line == null) break;

                var message = JsonSerializer.Deserialize<Message>(line);
                if (message != null)
                {
                    MessageReceived?.Invoke(message);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Connection lost: {ex.Message}");
        }
    }

    public async Task SendMessageAsync(Message message)
    {
        if (_writer == null) throw new InvalidOperationException("Not connected");
        var json = JsonSerializer.Serialize(message);
        await _writer.WriteLineAsync(json);
    }

    public Task DisconnectAsync()
    {
        _cts?.Cancel();
        _client?.Close();
        return Task.CompletedTask;
    }
}
