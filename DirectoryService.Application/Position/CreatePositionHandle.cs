using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Position;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Positions.ValueObjects;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Position;

public class CreatePositionHandle
{
    private readonly IPositionRepository _positionRepository;
    private readonly ILogger<CreatePositionHandle> _logger;

    public CreatePositionHandle(IPositionRepository positionRepository, ILogger<CreatePositionHandle> logger)
    {
        _positionRepository = positionRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(CreatePositionRequest request, CancellationToken cancellationToken)
    {
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