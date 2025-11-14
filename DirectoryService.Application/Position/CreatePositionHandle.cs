using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Position;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Positions.ValueObjects;
using Shared;

namespace DirectoryService.Application.Position;

public class CreatePositionHandle
{
    private readonly IPositionRepository _positionRepository;

    public CreatePositionHandle(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<Result<Guid, Error>> Handle(CreatePositionRequest request, CancellationToken cancellationToken)
    {
        PositionId positionId = PositionId.NewPositionId();

        var positionNameResult = PositionName.Create(request.Name.Value);
        if (positionNameResult.IsFailure)
        {
            return positionNameResult.Error;
        }

        PositionName positionName = positionNameResult.Value;

        var positionDescriptionResult = request.Description != null
            ? PositionDescription.Create(request.Description.Value)
            : Result.Success<PositionDescription, Error>(null!);

        if (positionDescriptionResult.IsFailure)
            return positionDescriptionResult.Error;

        PositionDescription? positionDescription = positionDescriptionResult.Value;

        Domain.Positions.Position position = new(positionId, positionName, new List<DepartmentPosition>(),
            positionDescription);

        return await _positionRepository.Add(position, cancellationToken);
    }
}