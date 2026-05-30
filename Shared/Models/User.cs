namespace ChatSystem.Models;

public class User
{
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; } // Only used during login/register requests

    public User(string username) => Username = username;
    public User() { }
}
