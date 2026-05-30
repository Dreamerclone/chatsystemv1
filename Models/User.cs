namespace ChatSystem.Models;

public class User
{
    public string Username { get; set; } = string.Empty;

    public User(string username)
    {
        Username = username;
    }

    // Parameterless constructor for JSON deserialization
    public User() { }
}
