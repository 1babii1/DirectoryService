using DirectoryService.Application.Database;
using DirectoryService.Contracts.Request.Department;
using DirectoryService.Contracts.Response.Department;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Department.Queries;

public class GetDepartmentByIdHandler
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentByIdHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<ReadDepartmentWithChildrenDto?> Handle(GetDepartmentByIdRequest request, CancellationToken cancellationToken)
    {
        var department = await _readDbContext.DepartmentsRead
            .Include(d => d.DepartmentsChildrenList)
            .Where(d => d.Id == DepartmentId.FromValue(request.DepartmentId))
            .Select(d => new ReadDepartmentWithChildrenDto
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
            .FirstOrDefaultAsync(cancellationToken);
        if (department is null)
        {
            return null;
        }

        return department;
    }
}