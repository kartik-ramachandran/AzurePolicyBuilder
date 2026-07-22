using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using ApimPolicyStudio.Core.Models;

namespace ApimPolicyStudio.Core.Services;

public class PolicyFragmentService
{
    private readonly StudioDatabase _database;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PolicyFragmentService(StudioDatabase database, string? legacyJsonPath = null)
    {
        _database = database;
        if (!string.IsNullOrEmpty(legacyJsonPath))
        {
            MigrateLegacyJson(legacyJsonPath);
        }
    }

    private void MigrateLegacyJson(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return;
            }

            using var connection = _database.Open();
            using (var check = connection.CreateCommand())
            {
                check.CommandText = "SELECT COUNT(*) FROM Fragments";
                if (Convert.ToInt64(check.ExecuteScalar()) > 0)
                {
                    return;
                }
            }

            var fragments = JsonSerializer.Deserialize<List<PolicyFragment>>(File.ReadAllText(path), JsonOptions)
                            ?? new List<PolicyFragment>();
            foreach (var fragment in fragments)
            {
                using var insert = connection.CreateCommand();
                insert.CommandText = @"
INSERT INTO Fragments (Name, Description, Xml, Version, CreatedAt, UpdatedAt, UsageCount)
VALUES (@name, @description, @xml, @version, @createdAt, @updatedAt, @usageCount)";
                insert.Parameters.AddWithValue("@name", fragment.Name);
                insert.Parameters.AddWithValue("@description", fragment.Description);
                insert.Parameters.AddWithValue("@xml", fragment.Xml);
                insert.Parameters.AddWithValue("@version", fragment.Version);
                insert.Parameters.AddWithValue("@createdAt", fragment.CreatedAt.ToString("o"));
                insert.Parameters.AddWithValue("@updatedAt", fragment.UpdatedAt.ToString("o"));
                insert.Parameters.AddWithValue("@usageCount", fragment.UsageCount);
                insert.ExecuteNonQuery();
            }

            File.Move(path, path + ".migrated", true);
        }
        catch (Exception)
        {
            // Best-effort migration — never block startup on legacy data
        }
    }

    private static PolicyFragment ReadFragment(SqliteDataReader reader)
    {
        return new PolicyFragment
        {
            Id = reader.GetInt64(0).ToString(),
            Name = reader.GetString(1),
            Description = reader.GetString(2),
            Xml = reader.GetString(3),
            Version = (int)reader.GetInt64(4),
            CreatedAt = DateTime.Parse(reader.GetString(5), null, DateTimeStyles.RoundtripKind),
            UpdatedAt = DateTime.Parse(reader.GetString(6), null, DateTimeStyles.RoundtripKind),
            UsageCount = (int)reader.GetInt64(7)
        };
    }

    private const string SelectColumns = "Id, Name, Description, Xml, Version, CreatedAt, UpdatedAt, UsageCount";

    public Task<List<PolicyFragment>> GetAllFragmentsAsync()
    {
        var fragments = new List<PolicyFragment>();
        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM Fragments ORDER BY Id";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            fragments.Add(ReadFragment(reader));
        }
        return Task.FromResult(fragments);
    }

    public Task<PolicyFragment?> GetFragmentByIdAsync(string id)
    {
        return Task.FromResult(FindById(id));
    }

    private PolicyFragment? FindById(string id)
    {
        if (!long.TryParse(id, out var rowId))
        {
            return null;
        }

        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM Fragments WHERE Id = @id";
        command.Parameters.AddWithValue("@id", rowId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadFragment(reader) : null;
    }

    public Task<PolicyFragment> CreateFragmentAsync(CreateFragmentRequest request)
    {
        var now = DateTime.UtcNow;
        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Fragments (Name, Description, Xml, Version, CreatedAt, UpdatedAt, UsageCount)
VALUES (@name, @description, @xml, @version, @createdAt, @updatedAt, 0);
SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@name", request.Name);
        command.Parameters.AddWithValue("@description", request.Description);
        command.Parameters.AddWithValue("@xml", request.Xml);
        command.Parameters.AddWithValue("@version", request.Version);
        command.Parameters.AddWithValue("@createdAt", now.ToString("o"));
        command.Parameters.AddWithValue("@updatedAt", now.ToString("o"));
        var id = Convert.ToInt64(command.ExecuteScalar());

        return Task.FromResult(new PolicyFragment
        {
            Id = id.ToString(),
            Name = request.Name,
            Description = request.Description,
            Xml = request.Xml,
            Version = request.Version,
            CreatedAt = now,
            UpdatedAt = now,
            UsageCount = 0
        });
    }

    public Task<PolicyFragment?> UpdateFragmentAsync(string id, UpdateFragmentRequest request)
    {
        var fragment = FindById(id);
        if (fragment == null)
        {
            return Task.FromResult<PolicyFragment?>(null);
        }

        if (request.Name != null) fragment.Name = request.Name;
        if (request.Description != null) fragment.Description = request.Description;
        if (request.Xml != null)
        {
            fragment.Xml = request.Xml;
            fragment.Version++;
        }
        fragment.UpdatedAt = DateTime.UtcNow;

        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Fragments
SET Name = @name, Description = @description, Xml = @xml, Version = @version, UpdatedAt = @updatedAt
WHERE Id = @id";
        command.Parameters.AddWithValue("@name", fragment.Name);
        command.Parameters.AddWithValue("@description", fragment.Description);
        command.Parameters.AddWithValue("@xml", fragment.Xml);
        command.Parameters.AddWithValue("@version", fragment.Version);
        command.Parameters.AddWithValue("@updatedAt", fragment.UpdatedAt.ToString("o"));
        command.Parameters.AddWithValue("@id", long.Parse(id));
        command.ExecuteNonQuery();

        return Task.FromResult<PolicyFragment?>(fragment);
    }

    public Task<bool> DeleteFragmentAsync(string id)
    {
        if (!long.TryParse(id, out var rowId))
        {
            return Task.FromResult(false);
        }

        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Fragments WHERE Id = @id";
        command.Parameters.AddWithValue("@id", rowId);
        return Task.FromResult(command.ExecuteNonQuery() > 0);
    }

    public Task<PolicyFragment?> IncrementUsageAsync(string id)
    {
        if (!long.TryParse(id, out var rowId))
        {
            return Task.FromResult<PolicyFragment?>(null);
        }

        using var connection = _database.Open();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "UPDATE Fragments SET UsageCount = UsageCount + 1 WHERE Id = @id";
            command.Parameters.AddWithValue("@id", rowId);
            if (command.ExecuteNonQuery() == 0)
            {
                return Task.FromResult<PolicyFragment?>(null);
            }
        }

        return Task.FromResult(FindById(id));
    }
}
