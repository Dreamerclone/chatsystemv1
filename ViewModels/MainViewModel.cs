using System.Collections.ObjectModel;
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
    private string _username = string.Empty;
    private string _serverIp = "127.0.0.1";
    private int _port = 8888;
    private string _messageText = string.Empty;
    private bool _isConnected;
    private User? _currentUser;

    public ObservableCollection<Message> Messages { get; } = new();

    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string ServerIp { get => _serverIp; set => SetProperty(ref _serverIp, value); }
    public int Port { get => _port; set => SetProperty(ref _port, value); }
    public string MessageText { get => _messageText; set => SetProperty(ref _messageText, value); }
    public bool IsConnected { get => _isConnected; set => SetProperty(ref _isConnected, value); }

    public ICommand ConnectCommand { get; }
    public ICommand SendCommand { get; }

    public MainViewModel(IClientService clientService, IUserService userService)
    {
        _clientService = clientService;
        _userService = userService;

        ConnectCommand = new RelayCommand(_ => Connect(), _ => !IsConnected && !string.IsNullOrWhiteSpace(Username));
        SendCommand = new RelayCommand(_ => SendMessage(), _ => IsConnected && !string.IsNullOrWhiteSpace(MessageText));

        _clientService.MessageReceived += OnMessageReceived;
    }

    private async void Connect()
    {
        if (!_userService.IsValidUsername(Username, out string error))
        {
            MessageBox.Show(error, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            _currentUser = _userService.CreateUser(Username);
            await _clientService.ConnectAsync(ServerIp, Port);
            IsConnected = true;

            await _clientService.SendMessageAsync(new Message
            {
                Sender = _currentUser,
                Text = $"{Username} has joined the chat.",
                Type = MessageType.System
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void SendMessage()
    {
        if (_currentUser == null) return;

        var message = new Message
        {
            Sender = _currentUser,
            Text = MessageText,
            Timestamp = DateTime.UtcNow,
            Type = MessageType.Chat
        };

        try
        {
            await _clientService.SendMessageAsync(message);
            Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
            MessageText = string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to send message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnMessageReceived(Message message)
    {
        if (message.Sender.Username == _currentUser?.Username && message.Type == MessageType.Chat) return;
        Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
    }
}
