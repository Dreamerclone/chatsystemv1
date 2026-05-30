using ChatSystem.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ChatSystem.Services;

public class CommandService : ICommandService
{
    private readonly IServiceProvider _serviceProvider;
    public bool ShouldExit { get; private set; }

    public CommandService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> HandleCommandAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.StartsWith("/"))
            return false;

        var parts = input.Split(' ');
        var commandName = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        var commands = _serviceProvider.GetServices<IChatCommand>();
        var command = commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (command != null)
        {
            if (commandName == "/exit") ShouldExit = true;
            await command.ExecuteAsync(args);
            return true;
        }

        Console.WriteLine($"Unknown command: {commandName}. Type /help for a list of commands.");
        return true;
    }
}
