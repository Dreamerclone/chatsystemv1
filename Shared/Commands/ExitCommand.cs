namespace ChatSystem.Commands;

public class ExitCommand : IChatCommand
{
    public string Name => "/exit";
    public string Description => "Exits the application.";

    public Task ExecuteAsync(string[] args)
    {
        Console.WriteLine("Exiting...");
        return Task.CompletedTask;
    }
}
