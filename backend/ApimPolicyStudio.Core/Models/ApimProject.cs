namespace ApimPolicyStudio.Core.Models;

public class ApimProject
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ApiDefinition> Apis { get; set; } = new();
    public List<Backend> Backends { get; set; } = new();
    public List<NamedValue> NamedValues { get; set; } = new();
    public List<PolicyFragmentDefinition> PolicyFragments { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<Schema> Schemas { get; set; } = new();
}

public class ApiDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Protocols { get; set; } = new() { "https" };
    public string? ServiceUrl { get; set; }
    public string? SubscriptionKeyParameterName { get; set; }
    public string? ApiVersion { get; set; }
    public string? ApiVersionSetId { get; set; }
    public string ApiRevision { get; set; } = "1";
    public bool IsCurrent { get; set; } = true;
    public bool SubscriptionRequired { get; set; } = true;
    public string? Policy { get; set; }
    public List<Operation> Operations { get; set; } = new();
    public string? SpecificationFormat { get; set; } // "openapi", "swagger", etc.
    public string? SpecificationContent { get; set; }
}

public class Operation
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty; // GET, POST, etc.
    public string UrlTemplate { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Policy { get; set; }
}

public class Backend
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Protocol { get; set; } = "http";
    public Dictionary<string, string>? Properties { get; set; }
}

public class NamedValue
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Secret { get; set; } = false;
    public List<string> Tags { get; set; } = new();
}

public class PolicyFragmentDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PolicyContent { get; set; } = string.Empty;
}

public class Product
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool SubscriptionRequired { get; set; } = true;
    public bool ApprovalRequired { get; set; } = false;
    public bool Published { get; set; } = true;
    public List<string> Apis { get; set; } = new();
}

public class Schema
{
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
    public string Content { get; set; } = string.Empty;
}
