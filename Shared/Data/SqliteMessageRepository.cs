using ChatSystem.Models;
using ChatSystem.Services;
using Microsoft.Data.Sqlite;

namespace ChatSystem.Data;

public class SqliteMessageRepository : IMessageRepository
{
    private readonly IDatabaseService _dbService;

    public SqliteMessageRepository(IDatabaseService dbService)
    {
        _dbService = dbService;
        _dbService.Initialize();
    }

    public void AddMessage(Message message)
    {
        using var connection = _dbService.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            // Ensure User exists
            var userCmd = connection.CreateCommand();
            userCmd.Transaction = transaction;
            userCmd.CommandText = "INSERT OR IGNORE INTO Users (Username) VALUES (@username)";
            userCmd.Parameters.AddWithValue("@username", message.Sender.Username);
            userCmd.ExecuteNonQuery();

            if (message.IsPrivate)
            {
                var recipientCmd = connection.CreateCommand();
                recipientCmd.Transaction = transaction;
                recipientCmd.CommandText = "INSERT OR IGNORE INTO Users (Username) VALUES (@username)";
                recipientCmd.Parameters.AddWithValue("@username", message.RecipientUsername);
                recipientCmd.ExecuteNonQuery();
            }

            // Insert Message
            var msgCmd = connection.CreateCommand();
            msgCmd.Transaction = transaction;
            msgCmd.CommandText =
            @"
                INSERT INTO Messages (Id, SenderUsername, RecipientUsername, Text, Timestamp)
                VALUES (@id, @sender, @recipient, @text, @timestamp)
            ";
            msgCmd.Parameters.AddWithValue("@id", message.Id.ToString());
            msgCmd.Parameters.AddWithValue("@sender", message.Sender.Username);
            msgCmd.Parameters.AddWithValue("@recipient", (object?)message.RecipientUsername ?? DBNull.Value);
            msgCmd.Parameters.AddWithValue("@text", message.Text);
            msgCmd.Parameters.AddWithValue("@timestamp", message.Timestamp.ToString("o")); // ISO 8601

            msgCmd.ExecuteNonQuery();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public IEnumerable<Message> GetAllMessages()
    {
        var messages = new List<Message>();

        using var connection = _dbService.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, SenderUsername, RecipientUsername, Text, Timestamp FROM Messages ORDER BY Timestamp";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            messages.Add(new Message
            {
                Id = Guid.Parse(reader.GetString(0)),
                Sender = new User(reader.GetString(1)),
                RecipientUsername = reader.IsDBNull(2) ? null : reader.GetString(2),
                Text = reader.GetString(3),
                Timestamp = DateTime.Parse(reader.GetString(4))
            });
        }

        return messages;
    }
}
