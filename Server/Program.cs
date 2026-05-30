using Microsoft.Extensions.DependencyInjection;
using ChatSystem.Data;
using ChatSystem.Services;

namespace ChatSystem.Server;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDatabaseService, DatabaseService>(sp => new DatabaseService("chat.db"));
        services.AddSingleton<IMessageRepository, SqliteMessageRepository>();
        services.AddSingleton<IServerService, ServerService>();

        var provider = services.BuildServiceProvider();
        var server = provider.GetRequiredService<IServerService>();

        // Read port from Environment Variable (for Cloud) or default to 8888
        string? envPort = Environment.GetEnvironmentVariable("PORT");
        int port = string.IsNullOrEmpty(envPort) ? 8888 : int.Parse(envPort);

        Console.WriteLine($"=== Chat Server (Cloud Ready) ===");
        Console.WriteLine($"Listening on port {port}...");

        await server.StartAsync(port, CancellationToken.None);
    }
}
