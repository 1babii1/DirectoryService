using CSharpFunctionalExtensions;
using DirectoryService.Application.Department;
using DirectoryService.Application.Department.Commands;
using DirectoryService.Application.Department.Queries;
using DirectoryService.Contracts.Request.Department;
using DirectoryService.Contracts.Response.Department;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.EndpointResults;

namespace DirectoryService.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] CreateDepartmentHandle handler,
        CreateDepartmentCommand request, CancellationToken cancellationToken) =>
        await handler.Handle(request, cancellationToken);

    [HttpPatch("/locations")]
    public async Task<EndpointResult<DepartmentId>> UpdateLocations(
        [FromServices] UpdateDepartmentLocationsHadler handler,
        UpdateDepartmentLocationsCommand request, CancellationToken cancellationToken) =>
        await handler.Handle(request, cancellationToken);

    [HttpPut("{departmentId:guid}/parent")]
    public async Task<EndpointResult<DepartmentId>> UpdateParent(
        [FromRoute] Guid departmentId,
        [FromServices] UpdateParentDepartmentHandle handler,
        UpdateParentDepartmentRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateParentDepartmentCommand(departmentId, request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet("/department/{departmentId:guid}")]
    public async Task<ActionResult<ReadDepartmentWithChildrenDto?>> GetDepartmentById(
        [FromRoute] Guid departmentId,
        [FromServices] GetDepartmentByIdHandle handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(new GetDepartmentByIdRequest(departmentId), cancellationToken);

    [HttpGet("/department/location")]
    public async Task<ActionResult<List<ReadDepartmentDto>?>> GetDepartmentByLocation(
        [FromQuery] GetDepartmentByLocationRequest request,
        [FromServices] GetDepartmentByLocationHandle handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(request, cancellationToken);

    [HttpGet("/top-positions")]
    public async Task<ActionResult<List<ReadDepartmentsTopDto>?>> GetDepartmentsTopForPositions(
        [FromServices] GetDepartmentsTopByPositionsHandle handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(cancellationToken);
}