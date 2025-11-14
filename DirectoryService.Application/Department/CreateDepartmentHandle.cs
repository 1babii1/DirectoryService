using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Department;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using Shared;

namespace DirectoryService.Application.Department;

public class CreateDepartmentHandle
{
    private readonly IDepartmentRepository _departmentRepository;

    public CreateDepartmentHandle(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<Result<Guid, Error>> Handle(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        DepartmentId departmentId = DepartmentId.NewDepartmentId();

        var departmentNameResult = DepartmentName.Create(request.Name.Value);
        if (departmentNameResult.IsFailure)
        {
            return departmentNameResult.Error;
        }

        DepartmentName departmentName = departmentNameResult.Value;

        var departmentIdentifierResult = DepartmentIdentifier.Create(request.Identifier.Value);
        if (departmentIdentifierResult.IsFailure)
        {
            return departmentIdentifierResult.Error;
        }

        DepartmentIdentifier departmentIdentifier = departmentIdentifierResult.Value;

        var department = request.department is null
            ? Departments.CreateParent(departmentName, departmentIdentifier, request.DepartmentsLocations,
                request.DepartmentId)
            : Departments.CreateChild(departmentName, departmentIdentifier, request.department, request.Depth ?? 0,
                request.DepartmentsLocations, request.DepartmentId);

        return await _departmentRepository.Add(department.Value, cancellationToken);
    }
}