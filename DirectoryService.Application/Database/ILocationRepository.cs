using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Shared;

namespace DirectoryService.Application.Database;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> Add(Locations locations, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<LocationId>, Error>> GetLocationsIds(IEnumerable<LocationId> locationIds, CancellationToken cancellationToken);
}