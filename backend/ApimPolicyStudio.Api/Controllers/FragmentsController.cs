using Microsoft.AspNetCore.Mvc;
using ApimPolicyStudio.Core.Models;
using ApimPolicyStudio.Core.Services;

namespace ApimPolicyStudio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FragmentsController : ControllerBase
{
    private readonly PolicyFragmentService _fragmentService;

    public FragmentsController(PolicyFragmentService fragmentService)
    {
        _fragmentService = fragmentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PolicyFragment>>> GetAllFragments()
    {
        var fragments = await _fragmentService.GetAllFragmentsAsync();
        return Ok(fragments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PolicyFragment>> GetFragmentById(string id)
    {
        var fragment = await _fragmentService.GetFragmentByIdAsync(id);
        if (fragment == null)
        {
            return NotFound();
        }
        return Ok(fragment);
    }

    [HttpPost]
    public async Task<ActionResult<PolicyFragment>> CreateFragment([FromBody] CreateFragmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Xml))
        {
            return BadRequest("Name and XML are required");
        }

        var fragment = await _fragmentService.CreateFragmentAsync(request);
        return CreatedAtAction(nameof(GetFragmentById), new { id = fragment.Id }, fragment);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PolicyFragment>> UpdateFragment(string id, [FromBody] UpdateFragmentRequest request)
    {
        var fragment = await _fragmentService.UpdateFragmentAsync(id, request);
        if (fragment == null)
        {
            return NotFound();
        }
        return Ok(fragment);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFragment(string id)
    {
        var success = await _fragmentService.DeleteFragmentAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}
