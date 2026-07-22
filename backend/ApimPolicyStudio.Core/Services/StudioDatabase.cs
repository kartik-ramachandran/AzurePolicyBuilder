using Microsoft.Data.Sqlite;

namespace ApimPolicyStudio.Core.Services;

/// <summary>
/// SQLite database backing the shared library (fragments, products, named values).
/// A single file that can live on a mounted volume so a hosted instance is
/// shared by everyone using the site.
/// </summary>
public class StudioDatabase
{
    public string ConnectionString { get; }

    public StudioDatabase(string databasePath)
    {
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        ConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Cache = SqliteCacheMode.Shared
        }.ToString();

        Initialize();
    }

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    private void Initialize()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
PRAGMA journal_mode=WAL;

CREATE TABLE IF NOT EXISTS Fragments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT NOT NULL DEFAULT '',
    Xml TEXT NOT NULL,
    Version INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    UsageCount INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Products (
    Name TEXT PRIMARY KEY COLLATE NOCASE,
    DisplayName TEXT NOT NULL DEFAULT '',
    Description TEXT NOT NULL DEFAULT '',
    SubscriptionRequired INTEGER NOT NULL DEFAULT 1,
    ApprovalRequired INTEGER NOT NULL DEFAULT 0,
    Published INTEGER NOT NULL DEFAULT 1,
    ApisJson TEXT NOT NULL DEFAULT '[]'
);

CREATE TABLE IF NOT EXISTS NamedValues (
    Name TEXT PRIMARY KEY COLLATE NOCASE,
    DisplayName TEXT NOT NULL DEFAULT '',
    Value TEXT NOT NULL DEFAULT '',
    Secret INTEGER NOT NULL DEFAULT 0,
    TagsJson TEXT NOT NULL DEFAULT '[]'
);";
        command.ExecuteNonQuery();
    }
}
