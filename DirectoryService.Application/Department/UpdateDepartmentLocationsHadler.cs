using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Department;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Department;

public class UpdateDepartmentLocationsValidation : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsValidation()
    {
        RuleFor(x => x.Request.departmentId).NotNull().NotEmpty().WithMessage("DepartmentId is not valid");
        RuleFor(x => x.Request.locationIds).NotNull().WithMessage("LocationIds is not valid");
    }
}

public record UpdateDepartmentLocationsCommand(UpdateDepartmentLocationsRequest Request);

public class UpdateDepartmentLocationsHadler
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationsRepository _locationRepository;
    private readonly UpdateDepartmentLocationsValidation _validator;
    private readonly ILogger<UpdateDepartmentLocationsHadler> _logger;

    public UpdateDepartmentLocationsHadler(
        IDepartmentRepository departmentRepository,
        ILogger<UpdateDepartmentLocationsHadler> logger, ILocationsRepository locationRepository,
        UpdateDepartmentLocationsValidation validator)
    {
        _departmentRepository = departmentRepository;
        _logger = logger;
        _locationRepository = locationRepository;
        _validator = validator;
    }

    public async Task<Result<DepartmentId, Error>> Handle(
        UpdateDepartmentLocationsCommand commandRequest,
        CancellationToken cancellationToken)
    {
        // Валидация данных запроса
        var validateResult = await _validator.ValidateAsync(commandRequest, cancellationToken);
        if (!validateResult.IsValid)
        {
            _logger.LogError("Failed to validate update department locations");
            return validateResult.ToError();
        }

        // Находим департамент
        var department = await _departmentRepository.GetById(commandRequest.Request.departmentId, cancellationToken);
        if (department.IsFailure)
        {
            _logger.LogError("Failed to get department by id");
            return department.Error;
        }

        // Проверяем локации
        var locations =
            await _locationRepository.GetLocationsIds(commandRequest.Request.locationIds, cancellationToken);
        if (locations.IsFailure)
        {
            _logger.LogError("Failed to get locations by ids");
            return locations.Error;
        }

        if (locations.Value.Any())
        {
            var missed = string.Join(", ", locations.Value.Select(id => id.Value));
            _logger.LogError("Missing locations: {Missed}", missed);
            return Error.Validation("locations", $"Locations not found: {missed}");
        }

        var departmentlocation =
            commandRequest.Request.locationIds.Select(id =>
                DepartmentLocation.Create(null, department.Value.Id, id).Value);

        department.Value.SetDepartmentsLocationsList(departmentlocation);

        await _departmentRepository.Save();

        return department.Value.Id;
    }
}