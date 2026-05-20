namespace ApimPolicyStudio.Core.Models;

public class PolicyTemplate
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required PolicyCategory Category { get; set; }
    public required string Xml { get; set; }
    public List<PolicyParameter>? Parameters { get; set; }
}

public enum PolicyCategory
{
    Inbound,
    Backend,
    Outbound,
    OnError
}

public class PolicyParameter
{
    public required string Name { get; set; }
    public required ParameterType Type { get; set; }
    public required string Description { get; set; }
    public string? DefaultValue { get; set; }
    public required bool Required { get; set; }
}

public enum ParameterType
{
    String,
    Number,
    Boolean,
    Expression
}
