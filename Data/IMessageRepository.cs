using ChatSystem.Models;

namespace ChatSystem.Data;

public interface IMessageRepository
{
    void AddMessage(Message message);
    IEnumerable<Message> GetAllMessages();
}
