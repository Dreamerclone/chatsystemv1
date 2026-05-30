using ChatSystem.Models;

namespace ChatSystem.Services;

public interface IChatService
{
    void SendMessage(User sender, string text);
    IEnumerable<Message> GetChatHistory();
}
