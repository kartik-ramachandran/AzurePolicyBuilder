using Microsoft.AspNetCore.Mvc;
using ApimPolicyStudio.Core.Models;
using ApimPolicyStudio.Core.Services;

namespace ApimPolicyStudio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly PolicyTemplateService _templateService;

    public TemplatesController(PolicyTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PolicyTemplate>>> GetAllTemplates()
    {
        var templates = await _templateService.GetAllTemplatesAsync();
        return Ok(templates);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PolicyTemplate>> GetTemplateById(string id)
    {
        var template = await _templateService.GetTemplateByIdAsync(id);
        if (template == null)
        {
            return NotFound();
        }
        return Ok(template);
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<List<PolicyTemplate>>> GetTemplatesByCategory(string category)
    {
        if (!Enum.TryParse<PolicyCategory>(category, true, out var categoryEnum))
        {
            return BadRequest("Invalid category");
        }

        var templates = await _templateService.GetTemplatesByCategoryAsync(categoryEnum);
        return Ok(templates);
    }
}
