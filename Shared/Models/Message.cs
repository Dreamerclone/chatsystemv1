namespace ChatSystem.Models;

public enum MessageType
{
    Chat,
    System,
    CommandRequest,
    CommandResponse,
    RegisterRequest,
    LoginRequest,
    AuthResponse
}

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public User Sender { get; set; } = new();
    public string? RecipientUsername { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public MessageType Type { get; set; } = MessageType.Chat;
    public bool Success { get; set; } // Used for AuthResponse

    public bool IsPrivate => !string.IsNullOrEmpty(RecipientUsername) && Type == MessageType.Chat;
}
