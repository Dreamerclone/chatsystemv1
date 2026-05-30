using Microsoft.Extensions.DependencyInjection;

namespace ChatSystem.Commands;

public class HelpCommand : IChatCommand
{
    private readonly IServiceProvider _serviceProvider;

    public HelpCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public string Name => "/help";
    public string Description => "Shows a list of available commands.";

    public Task ExecuteAsync(string[] args)
    {
        var commands = _serviceProvider.GetServices<IChatCommand>();
        Console.WriteLine("\n--- Available Commands ---");
        foreach (var cmd in commands)
        {
            Console.WriteLine($"{cmd.Name,-10} : {cmd.Description}");
        }
        Console.WriteLine("--------------------------\n");
        return Task.CompletedTask;
    }
}
