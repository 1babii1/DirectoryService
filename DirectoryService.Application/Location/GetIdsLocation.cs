using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Location;

public class GetIdsLocation
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateLocationHandle> _logger;

    public GetIdsLocation(
        ILocationsRepository locationsRepository,
        ILogger<CreateLocationHandle> logger)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<LocationId>, Error>> GetLocationsIds(CancellationToken cancellationToken)
    {
            var existLocationIds = await _locationsRepository.GetLocationsIds(cancellationToken);
            if (existLocationIds.IsFailure)
            {
                _logger.LogError("Failed to get locations ids");
                return existLocationIds.Error;
            }

            return existLocationIds;
    }
}