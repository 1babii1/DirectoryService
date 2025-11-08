using DirectoryService.Application;
using DirectoryService.Contracts.Location;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.EndpointResults;

namespace DirectoryService.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] CreateLocationHandle handler,
        CreateLocationRequest request, CancellationToken cancellationToken) => await handler.Handle(request, cancellationToken);
}