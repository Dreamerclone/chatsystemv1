namespace ChatSystem.Services;

public interface IServerService
{
    Task StartAsync(int port, CancellationToken ct);
}
