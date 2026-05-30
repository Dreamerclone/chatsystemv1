using Microsoft.Data.Sqlite;

namespace ChatSystem.Services;

public interface IDatabaseService
{
    SqliteConnection CreateConnection();
    void Initialize();
}
