using ChatSystem.Models;

namespace ChatSystem.Services;

public interface IUserService
{
    bool IsValidUsername(string username, out string errorMessage);
    User CreateUser(string username);
}
