using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories.Locations;

public class EfCoreLocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<EfCoreLocationsRepository> _logger;

    public EfCoreLocationsRepository(DirectoryServiceDbContext dbContext, ILogger<EfCoreLocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(
        Domain.Locations.Locations locations,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Location.AddAsync(locations, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return locations.Id.Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            _logger.LogError(ex, "Error adding location");

            if (pgEx.ConstraintName == "ux_locations_name")
            {
                return Error.Conflict(null, "Location name already exists");
            }

            if (pgEx.ConstraintName == "ux_locations_address")
            {
                return Error.Conflict(null, "Location address is already occupied");
            }

            return Error.Failure("location.insert", "Fail to insert location");
        }
    }
}