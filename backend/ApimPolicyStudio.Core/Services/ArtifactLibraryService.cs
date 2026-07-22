using System.Text.Json;
using Microsoft.Data.Sqlite;
using ApimPolicyStudio.Core.Models;

namespace ApimPolicyStudio.Core.Services;

/// <summary>
/// SQLite-backed library of reusable products and named values, shared by
/// everyone using the same backend instance.
/// </summary>
public class ArtifactLibraryService
{
    private readonly StudioDatabase _database;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ArtifactLibraryService(StudioDatabase database, string? legacyDirectory = null)
    {
        _database = database;
        if (!string.IsNullOrEmpty(legacyDirectory))
        {
            MigrateLegacyJson(legacyDirectory);
        }
    }

    private void MigrateLegacyJson(string directory)
    {
        MigrateLegacyFile<Product>(
            Path.Combine(directory, "library-products.json"), "Products", SaveProduct);
        MigrateLegacyFile<NamedValue>(
            Path.Combine(directory, "library-named-values.json"), "NamedValues", SaveNamedValue);
    }

    private void MigrateLegacyFile<T>(string path, string table, Func<T, T> save)
    {
        try
        {
            if (!File.Exists(path))
            {
                return;
            }

            using (var connection = _database.Open())
            using (var check = connection.CreateCommand())
            {
                check.CommandText = $"SELECT COUNT(*) FROM {table}";
                if (Convert.ToInt64(check.ExecuteScalar()) > 0)
                {
                    return;
                }
            }

            var items = JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), JsonOptions) ?? new List<T>();
            foreach (var item in items)
            {
                save(item);
            }

            File.Move(path, path + ".migrated", true);
        }
        catch (Exception)
        {
            // Best-effort migration — never block startup on legacy data
        }
    }

    public List<Product> GetProducts()
    {
        var products = new List<Product>();
        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Name, DisplayName, Description, SubscriptionRequired, ApprovalRequired, Published, ApisJson FROM Products ORDER BY Name";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            products.Add(new Product
            {
                Name = reader.GetString(0),
                DisplayName = reader.GetString(1),
                Description = reader.GetString(2),
                SubscriptionRequired = reader.GetInt64(3) != 0,
                ApprovalRequired = reader.GetInt64(4) != 0,
                Published = reader.GetInt64(5) != 0,
                Apis = JsonSerializer.Deserialize<List<string>>(reader.GetString(6), JsonOptions) ?? new List<string>()
            });
        }
        return products;
    }

    public Product SaveProduct(Product product)
    {
        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Products (Name, DisplayName, Description, SubscriptionRequired, ApprovalRequired, Published, ApisJson)
VALUES (@name, @displayName, @description, @subscriptionRequired, @approvalRequired, @published, @apisJson)
ON CONFLICT(Name) DO UPDATE SET
    DisplayName = excluded.DisplayName,
    Description = excluded.Description,
    SubscriptionRequired = excluded.SubscriptionRequired,
    ApprovalRequired = excluded.ApprovalRequired,
    Published = excluded.Published,
    ApisJson = excluded.ApisJson";
        command.Parameters.AddWithValue("@name", product.Name);
        command.Parameters.AddWithValue("@displayName", product.DisplayName);
        command.Parameters.AddWithValue("@description", product.Description);
        command.Parameters.AddWithValue("@subscriptionRequired", product.SubscriptionRequired ? 1 : 0);
        command.Parameters.AddWithValue("@approvalRequired", product.ApprovalRequired ? 1 : 0);
        command.Parameters.AddWithValue("@published", product.Published ? 1 : 0);
        command.Parameters.AddWithValue("@apisJson", JsonSerializer.Serialize(product.Apis, JsonOptions));
        command.ExecuteNonQuery();
        return product;
    }

    public bool DeleteProduct(string name)
    {
        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Products WHERE Name = @name";
        command.Parameters.AddWithValue("@name", name);
        return command.ExecuteNonQuery() > 0;
    }

    public List<NamedValue> GetNamedValues()
    {
        var namedValues = new List<NamedValue>();
        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Name, DisplayName, Value, Secret, TagsJson FROM NamedValues ORDER BY Name";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            namedValues.Add(new NamedValue
            {
                Name = reader.GetString(0),
                DisplayName = reader.GetString(1),
                Value = reader.GetString(2),
                Secret = reader.GetInt64(3) != 0,
                Tags = JsonSerializer.Deserialize<List<string>>(reader.GetString(4), JsonOptions) ?? new List<string>()
            });
        }
        return namedValues;
    }

    public NamedValue SaveNamedValue(NamedValue namedValue)
    {
        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO NamedValues (Name, DisplayName, Value, Secret, TagsJson)
VALUES (@name, @displayName, @value, @secret, @tagsJson)
ON CONFLICT(Name) DO UPDATE SET
    DisplayName = excluded.DisplayName,
    Value = excluded.Value,
    Secret = excluded.Secret,
    TagsJson = excluded.TagsJson";
        command.Parameters.AddWithValue("@name", namedValue.Name);
        command.Parameters.AddWithValue("@displayName", namedValue.DisplayName);
        command.Parameters.AddWithValue("@value", namedValue.Value);
        command.Parameters.AddWithValue("@secret", namedValue.Secret ? 1 : 0);
        command.Parameters.AddWithValue("@tagsJson", JsonSerializer.Serialize(namedValue.Tags, JsonOptions));
        command.ExecuteNonQuery();
        return namedValue;
    }

    public bool DeleteNamedValue(string name)
    {
        using var connection = _database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM NamedValues WHERE Name = @name";
        command.Parameters.AddWithValue("@name", name);
        return command.ExecuteNonQuery() > 0;
    }
}
