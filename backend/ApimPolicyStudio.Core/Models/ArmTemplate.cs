namespace ApimPolicyStudio.Core.Models;

public class ArmTemplate
{
    public string Schema { get; set; } = "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#";
    public string ContentVersion { get; set; } = "1.0.0.0";
    public Dictionary<string, object> Parameters { get; set; } = new();
    public Dictionary<string, object> Variables { get; set; } = new();
    public List<object> Resources { get; set; } = new();
}

public class ExportRequest
{
    public required string Xml { get; set; }
    public required string Scope { get; set; }
}

public class ExportBicepResponse
{
    public required string Bicep { get; set; }
}
