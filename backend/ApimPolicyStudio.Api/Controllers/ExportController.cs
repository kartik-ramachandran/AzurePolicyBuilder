using Microsoft.AspNetCore.Mvc;
using ApimPolicyStudio.Core.Models;
using ApimPolicyStudio.Core.Services;

namespace ApimPolicyStudio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly ArmTemplateService _armTemplateService;

    public ExportController(ArmTemplateService armTemplateService)
    {
        _armTemplateService = armTemplateService;
    }

    [HttpPost("arm")]
    public async Task<ActionResult<ArmTemplate>> ExportToArm([FromBody] ExportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Xml))
        {
            return BadRequest("XML cannot be empty");
        }

        var template = await _armTemplateService.GenerateArmTemplateAsync(request.Xml, request.Scope);
        return Ok(template);
    }

    [HttpPost("bicep")]
    public async Task<ActionResult<ExportBicepResponse>> ExportToBicep([FromBody] ExportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Xml))
        {
            return BadRequest("XML cannot be empty");
        }

        var bicep = await _armTemplateService.GenerateBicepAsync(request.Xml, request.Scope);
        return Ok(new ExportBicepResponse { Bicep = bicep });
    }
}
