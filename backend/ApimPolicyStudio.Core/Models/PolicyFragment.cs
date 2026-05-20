namespace ApimPolicyStudio.Core.Models;

public class PolicyFragment
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Xml { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UsageCount { get; set; }
}

public class CreateFragmentRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Xml { get; set; }
    public int Version { get; set; }
}

public class UpdateFragmentRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Xml { get; set; }
}
