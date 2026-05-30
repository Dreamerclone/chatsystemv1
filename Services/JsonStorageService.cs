using System.IO;
using System.Text.Json;
using ChatSystem.Models;

namespace ChatSystem.Services;

public class JsonStorageService : IStorageService
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public JsonStorageService(string filePath = "chat_history.json")
    {
        _filePath = filePath;
    }

    public void Save(IEnumerable<Message> messages)
    {
        try
        {
            string json = JsonSerializer.Serialize(messages, _options);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving chat history: {ex.Message}");
        }
    }

    public IEnumerable<Message> Load()
    {
        if (!File.Exists(_filePath))
        {
            return Enumerable.Empty<Message>();
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Message>>(json, _options) ?? new List<Message>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading chat history: {ex.Message}");
            return Enumerable.Empty<Message>();
        }
    }
}
