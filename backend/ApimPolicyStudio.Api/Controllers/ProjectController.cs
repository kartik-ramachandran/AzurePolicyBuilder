using Microsoft.AspNetCore.Mvc;
using ApimPolicyStudio.Core.Models;
using ApimPolicyStudio.Core.Services;

namespace ApimPolicyStudio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly ApimProjectGeneratorService _projectGenerator;

    public ProjectController()
    {
        _projectGenerator = new ApimProjectGeneratorService();
    }

    [HttpPost("generate")]
    public async Task<ActionResult<Dictionary<string, string>>> GenerateProject([FromBody] ApimProject project)
    {
        try
        {
            var files = await _projectGenerator.GenerateProjectStructureAsync(project);
            return Ok(files);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("download")]
    public async Task<IActionResult> DownloadProject([FromBody] ApimProject project)
    {
        try
        {
            var zipBytes = await _projectGenerator.GenerateZipArchiveAsync(project);
            var fileName = $"{project.Name.Replace(" ", "-")}-apim-project.zip";
            
            return File(zipBytes, "application/zip", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
