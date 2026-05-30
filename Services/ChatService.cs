using ChatSystem.Data;
using ChatSystem.Models;

namespace ChatSystem.Services;

public class ChatService : IChatService
{
    private readonly IMessageRepository _repository;

    public ChatService(IMessageRepository repository)
    {
        _repository = repository;
    }

    public void SendMessage(User sender, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var message = new Message
        {
            Sender = sender,
            Text = text,
            Timestamp = DateTime.UtcNow
        };

        _repository.AddMessage(message);
    }

    public IEnumerable<Message> GetChatHistory()
    {
        return _repository.GetAllMessages();
    }
}
