using Microsoft.Data.Sqlite;

namespace ChatSystem.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string databasePath = "chat.db")
    {
        _connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
    }

    public SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    public void Initialize()
    {
        using var connection = CreateConnection();
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Username TEXT PRIMARY KEY,
                PasswordHash TEXT
            );
            CREATE TABLE IF NOT EXISTS Messages (
                Id TEXT PRIMARY KEY,
                SenderUsername TEXT,
                RecipientUsername TEXT,
                Text TEXT,
                Timestamp TEXT,
                FOREIGN KEY(SenderUsername) REFERENCES Users(Username)
            );";
        command.ExecuteNonQuery();
    }
}
