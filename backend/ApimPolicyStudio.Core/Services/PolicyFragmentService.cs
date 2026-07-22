using System.Text.Json;
using ApimPolicyStudio.Core.Models;

namespace ApimPolicyStudio.Core.Services;

public class PolicyFragmentService
{
    private readonly object _lock = new();
    private readonly string _storePath;
    private readonly List<PolicyFragment> _fragments;
    private int _nextId = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public PolicyFragmentService() : this(DefaultStorePath())
    {
    }

    public PolicyFragmentService(string storePath)
    {
        _storePath = storePath;
        _fragments = LoadFragments();
        if (_fragments.Count > 0)
        {
            _nextId = _fragments.Max(f => int.TryParse(f.Id, out var n) ? n : 0) + 1;
        }
    }

    private static string DefaultStorePath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ApimPolicyStudio");
        return Path.Combine(dir, "fragments.json");
    }

    private List<PolicyFragment> LoadFragments()
    {
        try
        {
            if (File.Exists(_storePath))
            {
                var json = File.ReadAllText(_storePath);
                return JsonSerializer.Deserialize<List<PolicyFragment>>(json, JsonOptions) ?? new List<PolicyFragment>();
            }
        }
        catch (Exception)
        {
            // A corrupt or unreadable store shouldn't take down the API — start empty
        }
        return new List<PolicyFragment>();
    }

    private void SaveFragments()
    {
        var dir = Path.GetDirectoryName(_storePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(_storePath, JsonSerializer.Serialize(_fragments, JsonOptions));
    }

    public Task<List<PolicyFragment>> GetAllFragmentsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_fragments.ToList());
        }
    }

    public Task<PolicyFragment?> GetFragmentByIdAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_fragments.FirstOrDefault(f => f.Id == id));
        }
    }

    public Task<PolicyFragment> CreateFragmentAsync(CreateFragmentRequest request)
    {
        lock (_lock)
        {
            var fragment = new PolicyFragment
            {
                Id = _nextId++.ToString(),
                Name = request.Name,
                Description = request.Description,
                Xml = request.Xml,
                Version = request.Version,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UsageCount = 0
            };

            _fragments.Add(fragment);
            SaveFragments();
            return Task.FromResult(fragment);
        }
    }

    public Task<PolicyFragment?> UpdateFragmentAsync(string id, UpdateFragmentRequest request)
    {
        lock (_lock)
        {
            var fragment = _fragments.FirstOrDefault(f => f.Id == id);
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

            SaveFragments();
            return Task.FromResult<PolicyFragment?>(fragment);
        }
    }

    public Task<bool> DeleteFragmentAsync(string id)
    {
        lock (_lock)
        {
            var fragment = _fragments.FirstOrDefault(f => f.Id == id);
            if (fragment == null)
            {
                return Task.FromResult(false);
            }

            _fragments.Remove(fragment);
            SaveFragments();
            return Task.FromResult(true);
        }
    }

    public Task<PolicyFragment?> IncrementUsageAsync(string id)
    {
        lock (_lock)
        {
            var fragment = _fragments.FirstOrDefault(f => f.Id == id);
            if (fragment == null)
            {
                return Task.FromResult<PolicyFragment?>(null);
            }

            fragment.UsageCount++;
            SaveFragments();
            return Task.FromResult<PolicyFragment?>(fragment);
        }
    }
}
