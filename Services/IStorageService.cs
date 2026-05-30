using ChatSystem.Models;

namespace ChatSystem.Services;

public interface IStorageService
{
    void Save(IEnumerable<Message> messages);
    IEnumerable<Message> Load();
}
