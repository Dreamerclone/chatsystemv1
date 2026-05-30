using ChatSystem.Services;

namespace ChatSystem.Commands;

public class HistoryCommand : IChatCommand
{
    private readonly IChatService _chatService;

    public HistoryCommand(IChatService chatService)
    {
        _chatService = chatService;
    }

    public string Name => "/history";
    public string Description => "Displays the chat history.";

    public Task ExecuteAsync(string[] args)
    {
        var history = _chatService.GetChatHistory();
        Console.WriteLine("\n--- Chat History ---");
        foreach (var msg in history)
        {
            string prefix = msg.IsPrivate ? "[PRIVATE] " : "";
            Console.WriteLine($"{prefix}[{msg.Timestamp:HH:mm:ss}] {msg.Sender.Username}: {msg.Text}");
        }
        Console.WriteLine("--------------------\n");
        return Task.CompletedTask;
    }
}
