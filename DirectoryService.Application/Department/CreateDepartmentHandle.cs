using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Department;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Department;

public class CreateDepartmentHandle
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<CreateDepartmentHandle> _logger;

    public CreateDepartmentHandle(IDepartmentRepository departmentRepository, ILogger<CreateDepartmentHandle> logger)
    {
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        DepartmentId departmentId = DepartmentId.NewDepartmentId();

        var departmentNameResult = DepartmentName.Create(request.Name.Value);
        if (departmentNameResult.IsFailure)
        {
            _logger.LogError("Failed to create department name");
            return departmentNameResult.Error;
        }

        DepartmentName departmentName = departmentNameResult.Value;

        var departmentIdentifierResult = DepartmentIdentifier.Create(request.Identifier.Value);
        if (departmentIdentifierResult.IsFailure)
        {
            _logger.LogError("Failed to create department identifier");
            return departmentIdentifierResult.Error;
        }

        DepartmentIdentifier departmentIdentifier = departmentIdentifierResult.Value;

        var department = request.department is null
            ? Departments.CreateParent(departmentName, departmentIdentifier, request.DepartmentsLocations,
                request.DepartmentId)
            : Departments.CreateChild(departmentName, departmentIdentifier, request.department, request.Depth ?? 0,
                request.DepartmentsLocations, request.DepartmentId);

        if (department.IsFailure)
        {
            _logger.LogError("Failed to create department");
            return department.Error;
        }

        var result = await _departmentRepository.Add(department.Value, cancellationToken);
        _logger.LogInformation("Department created successfully");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to create department");
            return result.Error;
        }

        return result;
    }
}