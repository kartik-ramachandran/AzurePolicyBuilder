using Microsoft.AspNetCore.Mvc;
using ApimPolicyStudio.Core.Models;
using ApimPolicyStudio.Core.Services;

namespace ApimPolicyStudio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidateController : ControllerBase
{
    private readonly PolicyValidationService _validationService;

    public ValidateController(PolicyValidationService validationService)
    {
        _validationService = validationService;
    }

    [HttpPost]
    public async Task<ActionResult<ValidationResult>> ValidatePolicy([FromBody] ValidatePolicyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Xml))
        {
            return BadRequest("XML cannot be empty");
        }

        var result = await _validationService.ValidatePolicyAsync(request.Xml);
        return Ok(result);
    }

    [HttpPost("expression")]
    public async Task<ActionResult<ValidationResult>> ValidateExpression([FromBody] ValidateExpressionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Expression))
        {
            return BadRequest("Expression cannot be empty");
        }

        var result = await _validationService.ValidateExpressionAsync(request.Expression);
        return Ok(result);
    }
}
