using ChatSystem.Models;

namespace ChatSystem.Services;

public class UserService : IUserService
{
    public bool IsValidUsername(string username, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            errorMessage = "Username cannot be empty.";
            return false;
        }

        if (username.Length < 3)
        {
            errorMessage = "Username must be at least 3 characters long.";
            return false;
        }

        if (username.Length > 20)
        {
            errorMessage = "Username must be no more than 20 characters long.";
            return false;
        }

        return true;
    }

    public User CreateUser(string username)
    {
        return new User(username.Trim());
    }
}
