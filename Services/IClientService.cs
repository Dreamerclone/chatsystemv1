using ChatSystem.Models;

namespace ChatSystem.Services;

public interface IClientService
{
    event Action<Message>? MessageReceived;
    Task ConnectAsync(string host, int port);
    Task SendMessageAsync(Message message);
    Task DisconnectAsync();
}
