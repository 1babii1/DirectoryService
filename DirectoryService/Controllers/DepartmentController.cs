using DirectoryService.Application.Department;
using DirectoryService.Contracts.Department;
using Microsoft.AspNetCore.Mvc;
using Shared.EndpointResults;

namespace DirectoryService.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] CreateDepartmentHandle handler,
        CreateDepartmentRequest request, CancellationToken cancellationToken) => await handler.Handle(request, cancellationToken);
}