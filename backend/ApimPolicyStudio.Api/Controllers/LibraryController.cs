using Microsoft.AspNetCore.Mvc;
using ApimPolicyStudio.Core.Models;
using ApimPolicyStudio.Core.Services;

namespace ApimPolicyStudio.Api.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly ArtifactLibraryService _library;

    public LibraryController(ArtifactLibraryService library)
    {
        _library = library;
    }

    [HttpGet("products")]
    public ActionResult<List<Product>> GetProducts()
    {
        return Ok(_library.GetProducts());
    }

    [HttpPost("products")]
    public ActionResult<Product> SaveProduct([FromBody] Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            return BadRequest(new { error = "Product name is required." });
        }
        return Ok(_library.SaveProduct(product));
    }

    [HttpDelete("products/{name}")]
    public ActionResult DeleteProduct(string name)
    {
        return _library.DeleteProduct(name) ? NoContent() : NotFound();
    }

    [HttpGet("named-values")]
    public ActionResult<List<NamedValue>> GetNamedValues()
    {
        return Ok(_library.GetNamedValues());
    }

    [HttpPost("named-values")]
    public ActionResult<NamedValue> SaveNamedValue([FromBody] NamedValue namedValue)
    {
        if (string.IsNullOrWhiteSpace(namedValue.Name))
        {
            return BadRequest(new { error = "Named value name is required." });
        }
        return Ok(_library.SaveNamedValue(namedValue));
    }

    [HttpDelete("named-values/{name}")]
    public ActionResult DeleteNamedValue(string name)
    {
        return _library.DeleteNamedValue(name) ? NoContent() : NotFound();
    }
}
