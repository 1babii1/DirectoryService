using System.Text.Json;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Request.Department;
using DirectoryService.Contracts.Response.Department;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Department.Queries;

public class GetDepartmentByLocationHandle
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentByLocationHandle(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<List<ReadDepartmentDto>?> Handle(
        GetDepartmentByLocationRequest request,
        CancellationToken cancellationToken)
    {
        var department = _readDbContext.DepartmentsRead;

        if (request.LocationIds != null)
        {
            var locationIds = request.LocationIds.Select(LocationId.FromValue).ToList();
            department = department.Where(d =>
                d.DepartmentsLocationsList.Any(dl => locationIds.Contains(dl.LocationId)));
        }

        if (!string.IsNullOrWhiteSpace(request.Search) && request.Search != null)
        {
            department = department.Where(d => request.Search.Contains(d.Name.Value));
        }

        var departmentDto = await department.Select(d => new ReadDepartmentDto
            {
                Id = d.Id.Value,
                ParentId = d.ParentId!.Value,
                Name = d.Name.Value,
                Identifier = d.Identifier.Value,
                Path = d.Path.Value,
                Depth = d.Depth,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return departmentDto;
    }
}