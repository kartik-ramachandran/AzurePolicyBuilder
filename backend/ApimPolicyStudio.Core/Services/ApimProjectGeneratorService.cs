using System.Text;
using System.Text.Json;
using ApimPolicyStudio.Core.Models;

namespace ApimPolicyStudio.Core.Services;

public class ApimProjectGeneratorService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<Dictionary<string, string>> GenerateProjectStructureAsync(ApimProject project)
    {
        var files = new Dictionary<string, string>();

        // Root level - Global policy
        if (!string.IsNullOrEmpty(project.Description))
        {
            files["README.md"] = GenerateReadme(project);
        }

        // APIs
        foreach (var api in project.Apis)
        {
            var apiFolder = $"apis/{SanitizeName(api.Name)}";
            
            // API Information
            files[$"{apiFolder}/apiInformation.json"] = JsonSerializer.Serialize(new
            {
                name = api.Name,
                properties = new
                {
                    displayName = api.DisplayName,
                    path = api.Path,
                    description = api.Description,
                    protocols = api.Protocols,
                    serviceUrl = api.ServiceUrl,
                    apiVersion = api.ApiVersion,
                    apiVersionSetId = api.ApiVersionSetId,
                    apiRevision = api.ApiRevision,
                    isCurrent = api.IsCurrent,
                    subscriptionRequired = api.SubscriptionRequired,
                    subscriptionKeyParameterNames = new
                    {
                        header = api.SubscriptionKeyParameterName ?? "Ocp-Apim-Subscription-Key"
                    }
                }
            }, JsonOptions);

            // API Policy
            if (!string.IsNullOrEmpty(api.Policy))
            {
                files[$"{apiFolder}/policy.xml"] = api.Policy;
            }

            // OpenAPI Specification
            if (!string.IsNullOrEmpty(api.SpecificationContent))
            {
                var ext = api.SpecificationFormat?.ToLower() switch
                {
                    "openapi" => "json",
                    "swagger" => "json",
                    "openapi-yaml" => "yaml",
                    _ => "json"
                };
                files[$"{apiFolder}/specification.{ext}"] = api.SpecificationContent;
            }

            // Operations
            if (api.Operations.Any())
            {
                var operationsFolder = $"{apiFolder}/operations";
                foreach (var operation in api.Operations)
                {
                    var opName = SanitizeName(operation.Name);
                    
                    // Operation Information
                    files[$"{operationsFolder}/{opName}/operationInformation.json"] = JsonSerializer.Serialize(new
                    {
                        name = operation.Name,
                        properties = new
                        {
                            displayName = operation.DisplayName,
                            method = operation.Method,
                            urlTemplate = operation.UrlTemplate,
                            description = operation.Description
                        }
                    }, JsonOptions);

                    // Operation Policy
                    if (!string.IsNullOrEmpty(operation.Policy))
                    {
                        files[$"{operationsFolder}/{opName}/policy.xml"] = operation.Policy;
                    }
                }
            }
        }

        // Backends
        foreach (var backend in project.Backends)
        {
            var backendFolder = $"backends/{SanitizeName(backend.Name)}";
            files[$"{backendFolder}/backendInformation.json"] = JsonSerializer.Serialize(new
            {
                name = backend.Name,
                properties = new
                {
                    title = backend.Title,
                    url = backend.Url,
                    protocol = backend.Protocol,
                    properties = backend.Properties
                }
            }, JsonOptions);
        }

        // Named Values
        foreach (var namedValue in project.NamedValues)
        {
            var nvFolder = $"named values/{SanitizeName(namedValue.Name)}";
            files[$"{nvFolder}/namedValueInformation.json"] = JsonSerializer.Serialize(new
            {
                name = namedValue.Name,
                properties = new
                {
                    displayName = namedValue.DisplayName,
                    value = namedValue.Value,
                    secret = namedValue.Secret,
                    tags = namedValue.Tags
                }
            }, JsonOptions);
        }

        // Policy Fragments
        foreach (var fragment in project.PolicyFragments)
        {
            var fragmentFolder = $"policy fragments/{SanitizeName(fragment.Name)}";
            
            files[$"{fragmentFolder}/policyFragmentInformation.json"] = JsonSerializer.Serialize(new
            {
                name = fragment.Name,
                properties = new
                {
                    description = fragment.Description
                }
            }, JsonOptions);

            files[$"{fragmentFolder}/policy.xml"] = fragment.PolicyContent;
        }

        // Products
        foreach (var product in project.Products)
        {
            var productFolder = $"products/{SanitizeName(product.Name)}";
            
            files[$"{productFolder}/productInformation.json"] = JsonSerializer.Serialize(new
            {
                name = product.Name,
                properties = new
                {
                    displayName = product.DisplayName,
                    description = product.Description,
                    subscriptionRequired = product.SubscriptionRequired,
                    approvalRequired = product.ApprovalRequired,
                    state = product.Published ? "published" : "notPublished"
                }
            }, JsonOptions);

            // Product APIs
            if (product.Apis.Any())
            {
                files[$"{productFolder}/apis.json"] = JsonSerializer.Serialize(new
                {
                    apis = product.Apis
                }, JsonOptions);
            }
        }

        // Tags
        if (project.Tags.Any())
        {
            files["tags/tags.json"] = JsonSerializer.Serialize(new
            {
                tags = project.Tags.Select(t => new { name = SanitizeName(t), displayName = t })
            }, JsonOptions);
        }

        // Schemas
        foreach (var schema in project.Schemas)
        {
            var schemaFolder = $"schemas/{SanitizeName(schema.Name)}";
            
            files[$"{schemaFolder}/schemaInformation.json"] = JsonSerializer.Serialize(new
            {
                name = schema.Name,
                properties = new
                {
                    contentType = schema.ContentType
                }
            }, JsonOptions);

            var ext = schema.ContentType.Contains("json") ? "json" : "xml";
            files[$"{schemaFolder}/schema.{ext}"] = schema.Content;
        }

        // Global policy.xml (if needed)
        files["policy.xml"] = string.IsNullOrWhiteSpace(project.GlobalPolicy)
            ? GenerateGlobalPolicy()
            : project.GlobalPolicy;

        return files;
    }

    private string GenerateReadme(ApimProject project)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {project.Name}");
        sb.AppendLine();
        sb.AppendLine($"{project.Description}");
        sb.AppendLine();
        sb.AppendLine("## Project Structure");
        sb.AppendLine();
        sb.AppendLine("This APIM project was generated by APIM Policy Studio.");
        sb.AppendLine();
        sb.AppendLine("### Components");
        sb.AppendLine($"- **APIs**: {project.Apis.Count}");
        sb.AppendLine($"- **Backends**: {project.Backends.Count}");
        sb.AppendLine($"- **Named Values**: {project.NamedValues.Count}");
        sb.AppendLine($"- **Policy Fragments**: {project.PolicyFragments.Count}");
        sb.AppendLine($"- **Products**: {project.Products.Count}");
        sb.AppendLine($"- **Schemas**: {project.Schemas.Count}");
        sb.AppendLine();
        sb.AppendLine("## Deployment");
        sb.AppendLine();
        sb.AppendLine("This structure follows Azure APIM standards and can be deployed using:");
        sb.AppendLine("- Azure DevOps APIOps Toolkit");
        sb.AppendLine("- Azure CLI with APIM extensions");
        sb.AppendLine("- ARM/Bicep templates");
        sb.AppendLine();
        sb.AppendLine("## Generated");
        sb.AppendLine($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        
        return sb.ToString();
    }

    private string GenerateGlobalPolicy()
    {
        return @"<policies>
    <inbound>
        <base />
    </inbound>
    <backend>
        <base />
    </backend>
    <outbound>
        <base />
    </outbound>
    <on-error>
        <base />
    </on-error>
</policies>";
    }

    private string SanitizeName(string name)
    {
        return name
            .Replace(" ", "-")
            .Replace("_", "-")
            .ToLowerInvariant()
            .Trim();
    }

    public async Task<byte[]> GenerateZipArchiveAsync(ApimProject project)
    {
        var files = await GenerateProjectStructureAsync(project);
        
        using var memoryStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.Key);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream);
                await writer.WriteAsync(file.Value);
            }
        }
        
        return memoryStream.ToArray();
    }
}
