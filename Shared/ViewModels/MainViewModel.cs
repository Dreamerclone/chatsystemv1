using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using ChatSystem.Infrastructure;
using ChatSystem.Models;
using ChatSystem.Services;

namespace ChatSystem.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IClientService _clientService;
    private readonly IUserService _userService;
    private readonly string _settingsPath = "settings.json";

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _serverIp = "zephyr.proxy.rlwy.net";
    private int _port = 54526;
    private string _messageText = string.Empty;
    private bool _isConnected;
    private bool _isLoggedIn;
    private User? _currentUser;

    public ObservableCollection<Message> Messages { get; } = new();

    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string ServerIp { get => _serverIp; set => SetProperty(ref _serverIp, value); }
    public int Port { get => _port; set => SetProperty(ref _port, value); }
    public string MessageText { get => _messageText; set => SetProperty(ref _messageText, value); }
    public bool IsConnected { get => _isConnected; set => SetProperty(ref _isConnected, value); }
    public bool IsLoggedIn { get => _isLoggedIn; set => SetProperty(ref _isLoggedIn, value); }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand SendCommand { get; }

    public MainViewModel(IClientService clientService, IUserService userService)
    {
        _clientService = clientService;
        _userService = userService;

        LoginCommand = new RelayCommand(_ => Authenticate(MessageType.LoginRequest), _ => !IsLoggedIn && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));
        RegisterCommand = new RelayCommand(_ => Authenticate(MessageType.RegisterRequest), _ => !IsLoggedIn && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));
        SendCommand = new RelayCommand(_ => SendMessage(), _ => IsLoggedIn && !string.IsNullOrWhiteSpace(MessageText));

        _clientService.MessageReceived += OnMessageReceived;

        LoadSettings();
    }

    private void LoadSettings()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var settings = JsonSerializer.Deserialize<LocalSettings>(File.ReadAllText(_settingsPath));
                if (settings != null)
                {
                    Username = settings.LastUsername;
                    ServerIp = settings.LastHost;
                    Port = settings.LastPort;
                }
            }
            catch { }
        }
    }

    private void SaveSettings()
    {
        var settings = new LocalSettings
        {
            LastUsername = Username,
            LastHost = ServerIp,
            LastPort = Port
        };
        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings));
    }

    private async void Authenticate(MessageType type)
    {
        try
        {
            if (!IsConnected)
            {
                await _clientService.ConnectAsync(ServerIp, Port);
                IsConnected = true;
            }

            var authMsg = new Message
            {
                Type = type,
                Sender = new User(Username) { Password = Password }
            };

            Action<Message>? authHandler = null;
            authHandler = (msg) =>
            {
                if (msg.Type == MessageType.AuthResponse)
                {
                    _clientService.MessageReceived -= authHandler;
                    Application.Current.Dispatcher.Invoke(() => {
                        if (msg.Success) {
                            IsLoggedIn = true;
                            _currentUser = new User(Username);
                            SaveSettings(); // Save details on success!
                        } else {
                            MessageBox.Show(msg.Text, "Auth Failed");
                        }
                    });
                }
            };

            _clientService.MessageReceived += authHandler;
            await _clientService.SendMessageAsync(authMsg);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection failed: {ex.Message}");
        }
    }

    private async void SendMessage()
    {
        var message = new Message { Sender = _currentUser!, Text = MessageText };
        await _clientService.SendMessageAsync(message);
        Messages.Add(message);
        MessageText = string.Empty;
    }

    private void OnMessageReceived(Message message)
    {
        if (message.Type == MessageType.AuthResponse) return;
        if (message.Sender.Username == _currentUser?.Username && message.Type == MessageType.Chat) return;
        Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
    }
}
