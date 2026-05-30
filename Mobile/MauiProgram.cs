using Microsoft.Extensions.Logging;
using ChatSystem.Services;
using ChatSystem.ViewModels;

namespace ChatSystem.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				builder.Logging.AddDebug();
			});

		// Core Services
		builder.Services.AddSingleton<IClientService, ClientService>();
		builder.Services.AddSingleton<IUserService, UserService>();

		// ViewModels & Pages
		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddSingleton<MainPage>();

		return builder.Build();
	}
}
