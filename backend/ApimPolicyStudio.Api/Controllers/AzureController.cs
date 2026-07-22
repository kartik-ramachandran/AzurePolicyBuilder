using Microsoft.AspNetCore.Mvc;
using Azure.Identity;
using ApimPolicyStudio.Core.Models;
using ApimPolicyStudio.Core.Services;

namespace ApimPolicyStudio.Api.Controllers;

public record AzureApimRequest(string SubscriptionId, string ResourceGroup, string ServiceName, List<string>? ApiNames);

[ApiController]
[Route("api/azure/apim")]
public class AzureController : ControllerBase
{
    private readonly AzureApimImportService _azureService;

    public AzureController(AzureApimImportService azureService)
    {
        _azureService = azureService;
    }

    [HttpPost("inventory")]
    public async Task<ActionResult<ApimInventory>> GetInventory([FromBody] AzureApimRequest request)
    {
        var validationError = Validate(request);
        if (validationError != null) return BadRequest(new { error = validationError });

        try
        {
            return Ok(await _azureService.GetInventoryAsync(request.SubscriptionId, request.ResourceGroup, request.ServiceName));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ToFriendlyError(ex) });
        }
    }

    [HttpPost("import")]
    public async Task<ActionResult<ApimProject>> Import([FromBody] AzureApimRequest request)
    {
        var validationError = Validate(request);
        if (validationError != null) return BadRequest(new { error = validationError });

        try
        {
            return Ok(await _azureService.ImportAsync(request.SubscriptionId, request.ResourceGroup, request.ServiceName, request.ApiNames));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ToFriendlyError(ex) });
        }
    }

    private static string? Validate(AzureApimRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SubscriptionId)) return "Subscription ID is required.";
        if (string.IsNullOrWhiteSpace(request.ResourceGroup)) return "Resource group is required.";
        if (string.IsNullOrWhiteSpace(request.ServiceName)) return "APIM service name is required.";
        return null;
    }

    private static string ToFriendlyError(Exception ex)
    {
        if (ex is CredentialUnavailableException or AuthenticationFailedException)
        {
            return "Azure sign-in failed. Run 'az login' on the machine hosting this backend " +
                   "(or set AZURE_TENANT_ID / AZURE_CLIENT_ID / AZURE_CLIENT_SECRET), then try again.";
        }

        if (ex is Azure.RequestFailedException requestFailed)
        {
            return requestFailed.Status switch
            {
                403 => "Access denied. The signed-in identity needs at least the 'API Management Service Reader' role on the APIM instance.",
                404 => "APIM instance not found. Check the subscription ID, resource group, and service name.",
                _ => $"Azure request failed ({requestFailed.Status}): {requestFailed.ErrorCode}"
            };
        }

        return $"Azure import failed: {ex.Message}";
    }
}
