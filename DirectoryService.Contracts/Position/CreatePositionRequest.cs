using DirectoryService.Domain.Positions.ValueObjects;

namespace DirectoryService.Contracts.Position;

public record CreatePositionRequest(PositionName Name, PositionDescription? Description);