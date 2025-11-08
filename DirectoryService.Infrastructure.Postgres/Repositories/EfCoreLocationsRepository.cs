using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class EfCoreLocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<EfCoreLocationsRepository> _logger;

    public EfCoreLocationsRepository(DirectoryServiceDbContext dbContext, ILogger<EfCoreLocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(Locations locations, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Location.AddAsync(locations, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return locations.Id.Value;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding location");

            return Error.Failure("location.insert", "Fail to insert location");
        }
    }
}