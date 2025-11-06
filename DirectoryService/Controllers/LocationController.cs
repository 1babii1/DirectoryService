using DirectoryService.Application;
using DirectoryService.Contracts.Location;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateLocationHandle handler,
        CreateLocationRequest request, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest((object)result.Error);
        }

        return Ok(result.Value);
    }
}