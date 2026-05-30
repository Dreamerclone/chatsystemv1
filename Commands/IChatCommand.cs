namespace ChatSystem.Commands;

public interface IChatCommand
{
    string Name { get; }
    string Description { get; }
    Task ExecuteAsync(string[] args);
}
