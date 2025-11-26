using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Department;

public class GetIdsDepartment
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<GetIdsDepartment> _logger;

    public GetIdsDepartment(IDepartmentRepository departmentRepository, ILogger<GetIdsDepartment> logger)
    {
        _departmentRepository = departmentRepository;
        _logger = logger;
    }
    
    public async Task<Result<IEnumerable<DepartmentId>, Error>> GetDepartmentsIds(CancellationToken cancellationToken)
    {
        var existDepartmentIds = await _departmentRepository.GetDepartmentsIds(cancellationToken);
        if (existDepartmentIds.IsFailure)
        {
            _logger.LogError("Failed to get departments ids");
            return existDepartmentIds.Error;
        }

        return existDepartmentIds;
    }
}