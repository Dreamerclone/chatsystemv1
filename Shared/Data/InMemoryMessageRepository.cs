using ChatSystem.Models;
using ChatSystem.Services;

namespace ChatSystem.Data;

public class InMemoryMessageRepository : IMessageRepository
{
    private readonly List<Message> _messages;
    private readonly IStorageService _storageService;

    public InMemoryMessageRepository(IStorageService storageService)
    {
        _storageService = storageService;
        // Load existing messages on startup
        _messages = _storageService.Load().ToList();
    }

    public void AddMessage(Message message)
    {
        _messages.Add(message);
        // Persist change immediately
        _storageService.Save(_messages);
    }

    public IEnumerable<Message> GetAllMessages()
    {
        return _messages.AsReadOnly();
    }
}
