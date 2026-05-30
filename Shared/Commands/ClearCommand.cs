namespace ChatSystem.Commands;

public class ClearCommand : IChatCommand
{
    public string Name => "/clear";
    public string Description => "Clears the console screen.";

    public Task ExecuteAsync(string[] args)
    {
        Console.Clear();
        return Task.CompletedTask;
    }
}
