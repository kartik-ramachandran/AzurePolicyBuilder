using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ApiManagement;
using Azure.ResourceManager.ApiManagement.Models;
using ApimPolicyStudio.Core.Models;

namespace ApimPolicyStudio.Core.Services;

public class ApimInventoryApi
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

public class ApimInventory
{
    public string ServiceName { get; set; } = string.Empty;
    public List<ApimInventoryApi> Apis { get; set; } = new();
    public int ProductCount { get; set; }
    public int NamedValueCount { get; set; }
    public int PolicyFragmentCount { get; set; }
    public int BackendCount { get; set; }
}

/// <summary>
/// Pulls an existing APIM instance from Azure into an ApimProject.
/// Authenticates with DefaultAzureCredential (az login, VS sign-in,
/// environment variables, or managed identity).
/// </summary>
public class AzureApimImportService
{
    private static ApiManagementServiceResource GetService(string subscriptionId, string resourceGroup, string serviceName)
    {
        var client = new ArmClient(new DefaultAzureCredential());
        var id = ApiManagementServiceResource.CreateResourceIdentifier(subscriptionId, resourceGroup, serviceName);
        return client.GetApiManagementServiceResource(id);
    }

    public async Task<ApimInventory> GetInventoryAsync(string subscriptionId, string resourceGroup, string serviceName)
    {
        var service = GetService(subscriptionId, resourceGroup, serviceName);
        var inventory = new ApimInventory { ServiceName = serviceName };

        await foreach (var api in service.GetApis().GetAllAsync())
        {
            inventory.Apis.Add(new ApimInventoryApi
            {
                Name = api.Data.Name,
                DisplayName = api.Data.DisplayName ?? api.Data.Name,
                Path = api.Data.Path ?? string.Empty
            });
        }

        await foreach (var _ in service.GetApiManagementProducts().GetAllAsync()) inventory.ProductCount++;
        await foreach (var _ in service.GetApiManagementNamedValues().GetAllAsync()) inventory.NamedValueCount++;
        await foreach (var _ in service.GetPolicyFragmentContracts().GetAllAsync()) inventory.PolicyFragmentCount++;
        await foreach (var _ in service.GetApiManagementBackends().GetAllAsync()) inventory.BackendCount++;

        return inventory;
    }

    public async Task<ApimProject> ImportAsync(string subscriptionId, string resourceGroup, string serviceName, List<string>? apiNames)
    {
        var service = GetService(subscriptionId, resourceGroup, serviceName);
        var project = new ApimProject
        {
            Name = serviceName,
            Description = $"Imported from Azure APIM instance '{serviceName}' ({resourceGroup})."
        };

        project.GlobalPolicy = await TryGetServicePolicyAsync(service);

        await foreach (var api in service.GetApis().GetAllAsync())
        {
            if (apiNames is { Count: > 0 } && !apiNames.Contains(api.Data.Name, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var definition = new ApiDefinition
            {
                Name = api.Data.Name,
                DisplayName = api.Data.DisplayName ?? api.Data.Name,
                Path = api.Data.Path ?? string.Empty,
                Description = api.Data.Description ?? string.Empty,
                ServiceUrl = api.Data.ServiceUri?.ToString(),
                ApiVersion = api.Data.ApiVersion,
                ApiVersionSetId = api.Data.ApiVersionSetId?.ToString(),
                ApiRevision = api.Data.ApiRevision ?? "1",
                IsCurrent = api.Data.IsCurrent ?? true,
                SubscriptionRequired = api.Data.IsSubscriptionRequired ?? true,
                SubscriptionKeyParameterName = api.Data.SubscriptionKeyParameterNames?.Header,
                Protocols = api.Data.Protocols?.Select(p => p.ToString().ToLowerInvariant()).ToList()
                            ?? new List<string> { "https" }
            };

            definition.Policy = await TryGetApiPolicyAsync(api);

            await foreach (var operation in api.GetApiOperations().GetAllAsync())
            {
                var op = new Models.Operation
                {
                    Name = operation.Data.Name,
                    DisplayName = operation.Data.DisplayName ?? operation.Data.Name,
                    Method = operation.Data.Method ?? "GET",
                    UrlTemplate = operation.Data.UriTemplate ?? "/",
                    Description = operation.Data.Description
                };

                op.Policy = await TryGetOperationPolicyAsync(operation);
                definition.Operations.Add(op);
            }

            project.Apis.Add(definition);
        }

        var importedApiNames = project.Apis.Select(a => a.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        await foreach (var backend in service.GetApiManagementBackends().GetAllAsync())
        {
            project.Backends.Add(new Backend
            {
                Name = backend.Data.Name,
                Title = backend.Data.Title ?? backend.Data.Name,
                Url = backend.Data.Uri?.ToString() ?? string.Empty,
                Protocol = backend.Data.Protocol?.ToString().ToLowerInvariant() ?? "http"
            });
        }

        await foreach (var namedValue in service.GetApiManagementNamedValues().GetAllAsync())
        {
            project.NamedValues.Add(new NamedValue
            {
                Name = namedValue.Data.Name,
                DisplayName = namedValue.Data.DisplayName ?? namedValue.Data.Name,
                // Secret values are not returned by the management API
                Value = namedValue.Data.Value ?? (namedValue.Data.IsSecret == true ? "<secret — set after deploy>" : string.Empty),
                Secret = namedValue.Data.IsSecret ?? false,
                Tags = namedValue.Data.Tags?.ToList() ?? new List<string>()
            });
        }

        await foreach (var fragment in service.GetPolicyFragmentContracts().GetAllAsync())
        {
            project.PolicyFragments.Add(new PolicyFragmentDefinition
            {
                Name = fragment.Data.Name,
                Description = fragment.Data.Description ?? string.Empty,
                PolicyContent = fragment.Data.Value ?? string.Empty
            });
        }

        await foreach (var product in service.GetApiManagementProducts().GetAllAsync())
        {
            project.Products.Add(new Product
            {
                Name = product.Data.Name,
                DisplayName = product.Data.DisplayName ?? product.Data.Name,
                Description = product.Data.Description ?? string.Empty,
                SubscriptionRequired = product.Data.IsSubscriptionRequired ?? true,
                ApprovalRequired = product.Data.IsApprovalRequired ?? false,
                Published = product.Data.State == ApiManagementProductState.Published,
                Apis = importedApiNames.ToList()
            });
        }

        return project;
    }

    private static async Task<string?> TryGetServicePolicyAsync(ApiManagementServiceResource service)
    {
        try
        {
            var policy = await service.GetApiManagementPolicies().GetAsync(PolicyName.Policy, PolicyExportFormat.Xml);
            return policy.Value.Data.Value;
        }
        catch (RequestFailedException)
        {
            return null;
        }
    }

    private static async Task<string?> TryGetApiPolicyAsync(ApiResource api)
    {
        try
        {
            var policy = await api.GetApiPolicies().GetAsync(PolicyName.Policy, PolicyExportFormat.Xml);
            return policy.Value.Data.Value;
        }
        catch (RequestFailedException)
        {
            return null;
        }
    }

    private static async Task<string?> TryGetOperationPolicyAsync(ApiOperationResource operation)
    {
        try
        {
            var policy = await operation.GetApiOperationPolicies().GetAsync(PolicyName.Policy, PolicyExportFormat.Xml);
            return policy.Value.Data.Value;
        }
        catch (RequestFailedException)
        {
            return null;
        }
    }
}
