using ApimPolicyStudio.Core.Models;

namespace ApimPolicyStudio.Core.Services;

public class PolicyFragmentService
{
    private readonly List<PolicyFragment> _fragments = new();
    private int _nextId = 1;

    public Task<List<PolicyFragment>> GetAllFragmentsAsync()
    {
        return Task.FromResult(_fragments);
    }

    public Task<PolicyFragment?> GetFragmentByIdAsync(string id)
    {
        return Task.FromResult(_fragments.FirstOrDefault(f => f.Id == id));
    }

    public Task<PolicyFragment> CreateFragmentAsync(CreateFragmentRequest request)
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
        return Task.FromResult(fragment);
    }

    public Task<PolicyFragment?> UpdateFragmentAsync(string id, UpdateFragmentRequest request)
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

        return Task.FromResult<PolicyFragment?>(fragment);
    }

    public Task<bool> DeleteFragmentAsync(string id)
    {
        var fragment = _fragments.FirstOrDefault(f => f.Id == id);
        if (fragment == null)
        {
            return Task.FromResult(false);
        }

        _fragments.Remove(fragment);
        return Task.FromResult(true);
    }

    public Task IncrementUsageAsync(string id)
    {
        var fragment = _fragments.FirstOrDefault(f => f.Id == id);
        if (fragment != null)
        {
            fragment.UsageCount++;
        }
        return Task.CompletedTask;
    }
}
