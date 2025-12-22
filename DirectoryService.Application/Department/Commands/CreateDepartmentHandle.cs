using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Department.Errors;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Request.Department;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Department.Commands;

public record CreateDepartmentCommand(CreateDepartmentRequest request);

public class CreateDepartmentValidation : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidation()
    {
        RuleFor(x => x.Name).MustBeValueObject(name => DepartmentName.Create(name.Value));
        RuleFor(x => x.Identifier).MustBeValueObject(identifier => DepartmentIdentifier.Create(identifier.Value));
        RuleFor(x => x.DepartmentId).NotEmpty().WithMessage("DepartmentId is not valid")
            .When(x => x.DepartmentId != null);
        RuleFor(x => x.ParentDepartmentId).NotEmpty().WithMessage("ParentId is not valid")
            .When(x => x.ParentDepartmentId != null);
        RuleFor(x => x.LocationsIds).NotEmpty().NotNull()
            .WithMessage("DepartmentsLocationsList is not valid")
            .Must(list =>
            {
                IEnumerable<LocationId> locationIds = list.ToList();
                return locationIds.Select(item => item.Value).Distinct().Count() == locationIds.Count();
            })
            .WithMessage("DepartmentsLocationsList contains duplicates");
        RuleFor(x => x.Depth).NotEmpty().WithMessage("Depth is not valid").When(x => x.Depth != null);
    }
}

public class CreateDepartmentHandle
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationsRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly CreateDepartmentValidation _validator;
    private readonly ILogger<CreateDepartmentHandle> _logger;

    public CreateDepartmentHandle(IDepartmentRepository departmentRepository, CreateDepartmentValidation validator,
        ILogger<CreateDepartmentHandle> logger, ILocationsRepository locationRepository,
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _validator = validator;
        _logger = logger;
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreateDepartmentCommand createDepartmentCommandRequest,
        CancellationToken cancellationToken)
    {
        CreateDepartmentRequest request = createDepartmentCommandRequest.request;

        // Валидация входных данных
        ValidationResult validateResult = await _validator.ValidateAsync(request);
        if (!validateResult.IsValid)
        {
            _logger.LogError("Failed to validate department");
            return validateResult.ToError();
        }

        // Проверка на существование локации
        var locationIdsNotFound = await _locationRepository.GetLocationsIds(request.LocationsIds, cancellationToken);
        if (locationIdsNotFound.IsFailure)
        {
            _logger.LogError("Failed to get locations ids " + locationIdsNotFound.Error.Messages);
            return locationIdsNotFound.Error;
        }

        // Проверка на существование Подразделения(если передан)
        Departments? departmentFromDB = null;
        if (request.ParentDepartmentId != null)
        {
            var getResult =
                await _departmentRepository.GetByIdIncludeLocations(request.ParentDepartmentId, cancellationToken);
            if (getResult.IsFailure)
            {
                _logger.LogError("Parent department not found " + getResult.Error.Messages);
                return Error.Failure("department.notfound", "Parent department not found");
            }

            departmentFromDB = getResult.Value;
        }

        if (locationIdsNotFound.Value.Any())
        {
            _logger.LogError("Locations not found " + string.Join(", ", locationIdsNotFound.Value));
            return DepartmentErrors.LocationsIdsNotFound();
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

        var department = departmentFromDB is null
            ? Departments.CreateParent(departmentName, departmentIdentifier, request.LocationsIds,
                request.DepartmentId)
            : Departments.CreateChild(departmentName, departmentIdentifier, departmentFromDB,
                request.LocationsIds, request.DepartmentId);
        if (department.IsFailure)
        {
            _logger.LogError("Failed to create department");
            return department.Error;
        }

        List<DepartmentLocation> departmentLocationsList = new List<DepartmentLocation>();
        foreach (var locationIdValue in request.LocationsIds)
        {
            var departmentLocation = DepartmentLocation.Create(null, department.Value.Id, locationIdValue);
            departmentLocationsList.Add(departmentLocation.Value);
        }

        department.Value.SetDepartmentsLocationsList(departmentLocationsList);

        var result = await _departmentRepository.Add(department.Value, cancellationToken);
        _logger.LogInformation("Department created successfully");
        if (result.IsFailure)
        {
            _logger.LogError("Failed to create department");
            return result.Error;
        }

        var save = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            _logger.LogError("Failed to create department");
            return save.Error;
        }

        return result;
    }
}