using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using Shared;

namespace DirectoryService.Application.Database;

public interface IDepartmentRepository
{
    Task<Result<Guid, Error>> Add(Departments department, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<DepartmentId>, Error>> GetDepartmentsIds(CancellationToken cancellationToken);
}