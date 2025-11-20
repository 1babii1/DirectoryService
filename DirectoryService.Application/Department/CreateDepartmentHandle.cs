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
    private readonly CreateDepartmentValidation _validator;
    private readonly ILogger<CreateDepartmentHandle> _logger;

    public CreateDepartmentHandle(IDepartmentRepository departmentRepository, CreateDepartmentValidation validator,
        ILogger<CreateDepartmentHandle> logger)
    {
        _departmentRepository = departmentRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(CreateDepartmentCommand createDepartmentCommandRequest, CancellationToken cancellationToken)
    {
        DepartmentId departmentId = DepartmentId.NewDepartmentId();
        CreateDepartmentRequest request = createDepartmentCommandRequest.request;

        // Валидация входных данных
        var validateResult = await _validator.ValidateAsync(request);
        if (!validateResult.IsValid)
        {
            _logger.LogError("Failed to validate department");
            var error = validateResult.Errors.First();
            var errorResult =
                Error.Validation(error.ErrorCode ?? "validate.fail", error.ErrorMessage, error.PropertyName);
            return errorResult;
        }

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