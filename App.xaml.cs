using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ChatSystem.Data;
using ChatSystem.Services;
using ChatSystem.ViewModels;
using ChatSystem.Views;

namespace ChatSystem;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Data & Core
        services.AddSingleton<IDatabaseService, DatabaseService>(sp => new DatabaseService("chat.db"));
        services.AddSingleton<IMessageRepository, SqliteMessageRepository>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IChatService, ChatService>();

        // Networking
        services.AddSingleton<IClientService, ClientService>();

        // UI
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        base.OnStartup(e);
    }
}
