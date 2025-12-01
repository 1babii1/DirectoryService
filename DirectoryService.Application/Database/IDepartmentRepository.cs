using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using Shared;

namespace DirectoryService.Application.Database;

public interface IDepartmentRepository
{
    Task Save();

    Task<Result<Departments, Error>> GetById(DepartmentId departmentIdId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<Departments>, Error>> GetById(
        IReadOnlyList<DepartmentId> departmentIds,
        CancellationToken cancellationToken);

    Task<Result<Guid, Error>> Add(Departments department, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<DepartmentId>, Error>> GetDepartmentsIds(
        IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken);
}