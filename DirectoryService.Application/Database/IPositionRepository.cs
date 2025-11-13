using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Application.Database;

public interface IPositionRepository
{
    Task<Result<Guid, Error>> Add(Domain.Positions.Position position, CancellationToken cancellationToken = default);
}