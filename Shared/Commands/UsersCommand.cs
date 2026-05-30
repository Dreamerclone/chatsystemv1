using ChatSystem.Models;
using ChatSystem.Services;

namespace ChatSystem.Commands;

public class UsersCommand : IChatCommand
{
    private readonly IClientService _clientService;

    public UsersCommand(IClientService clientService)
    {
        _clientService = clientService;
    }

    public string Name => "/users";
    public string Description => "Lists all online users.";

    public async Task ExecuteAsync(string[] args)
    {
        var request = new Message
        {
            Type = MessageType.CommandRequest,
            Text = "LIST_USERS"
        };
        await _clientService.SendMessageAsync(request);
    }
}
