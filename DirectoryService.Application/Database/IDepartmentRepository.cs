using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using Shared;

namespace DirectoryService.Application.Database;

public interface IDepartmentRepository
{
    Task<Result<Guid, Error>> Add(Departments department, CancellationToken cancellationToken = default);
}