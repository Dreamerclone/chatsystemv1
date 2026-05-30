namespace ChatSystem.Services;

public interface ICommandService
{
    Task<bool> HandleCommandAsync(string input);
    bool ShouldExit { get; }
}
