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
    private string _messageText = string.Empty;
    private bool _isConnected;
    private User? _currentUser;

    public ObservableCollection<Message> Messages { get; } = new();

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string MessageText
    {
        get => _messageText;
        set => SetProperty(ref _messageText, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

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
            await _clientService.ConnectAsync("127.0.0.1", 8888);
            IsConnected = true;

            // Send an initial message to register on the server
            await _clientService.SendMessageAsync(new Message
            {
                Sender = _currentUser,
                Text = $"User {Username} joined the chat.",
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

            // Add our own message to the list immediately for UI responsiveness
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
        // Don't add our own message if the server echoes it back (we already added it)
        if (message.Sender.Username == _currentUser?.Username && message.Type == MessageType.Chat) return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            Messages.Add(message);
        });
    }
}
