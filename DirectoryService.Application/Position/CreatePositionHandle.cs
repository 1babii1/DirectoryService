using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Department;
using DirectoryService.Application.Position.Errors;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Position;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Positions.ValueObjects;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Position;

public class CreatePositionHandle
{
    private readonly IPositionRepository _positionRepository;
    private readonly GetIdsDepartment _getIdsDepartment;
    private readonly CreatePositionValidation _validator;
    private readonly ILogger<CreatePositionHandle> _logger;

    public CreatePositionHandle(IPositionRepository positionRepository, GetIdsDepartment getIdsDepartment,
        CreatePositionValidation validator,
        ILogger<CreatePositionHandle> logger)
    {
        _positionRepository = positionRepository;
        _getIdsDepartment = getIdsDepartment;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreatePositionCommand positionCommand,
        CancellationToken cancellationToken)
    {
        CreatePositionRequest request = positionCommand.request;

        // Валидация входных данных
        _logger.LogInformation("Validating department");
        ValidationResult validateResult = await _validator.ValidateAsync(positionCommand, cancellationToken);
        if (!validateResult.IsValid)
        {
            _logger.LogError("Failed to validate location111");

            return validateResult.ToError();
        }

        // Проверка на существование департамента
        var allDepartmentIds = await _getIdsDepartment.GetDepartmentsIds(cancellationToken);
        if (allDepartmentIds.IsFailure)
        {
            _logger.LogError("Failed to get locations ids");
            return allDepartmentIds.Error;
        }

        var locationsIdsNotFound = request.DepartmentIds.Except(allDepartmentIds.Value);
        if (locationsIdsNotFound.Any())
        {
            _logger.LogError("Failed to get locations ids");
            return PositionErrors.DepartmentIdsNotFound();
        }

        PositionId positionId = PositionId.NewPositionId();

        var positionNameResult = PositionName.Create(request.Name.Value);
        if (positionNameResult.IsFailure)
        {
            _logger.LogError("Failed to create position name");
            return positionNameResult.Error;
        }

        PositionName positionName = positionNameResult.Value;

        var positionDescriptionResult = request.Description != null
            ? PositionDescription.Create(request.Description.Value)
            : Result.Success<PositionDescription, Error>(null!);

        if (positionDescriptionResult.IsFailure)
        {
            _logger.LogError("Failed to create position description");
            return positionDescriptionResult.Error;
        }

        PositionDescription? positionDescription = positionDescriptionResult.Value;

        Domain.Positions.Position position = new(positionId, positionName, new List<DepartmentPosition>(),
            positionDescription);

        var result = await _positionRepository.Add(position, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("Failed to create position");
            return result.Error;
        }

        _logger.LogInformation("Position created successfully");

        return result;
    }
}