using System.Collections;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Locations.ValueObjects;
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
                return Error.Conflict(null!, "Location name already exists");
            }

            if (pgEx.ConstraintName == "ux_locations_address")
            {
                return Error.Conflict(null!, "Location address is already occupied");
            }

            return Error.Failure("location.insert", "Fail to insert location");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding location");

            return Error.Failure("location.insert", "Fail to insert location");
        }
    }

    public async Task<Result<IEnumerable<LocationId>, Error>> GetLocationsIds(
        IEnumerable<LocationId> locationIds,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<LocationId> enumerable = locationIds.ToList();
            Console.WriteLine("locationIds: " + string.Join(", ", enumerable.Select(li => li.Value.ToString())));
            var allLocationIds = await _dbContext.Location
                .Where(l => enumerable.Contains(l.Id))
                .Select(l => l.Id)
                .ToListAsync(cancellationToken: cancellationToken);

            var missedIds = enumerable.Except(allLocationIds);

            return Result.Success<IEnumerable<LocationId>, Error>(missedIds);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == "23503")
            {
                return Error.Conflict("foreign.key", "Related entity not found");
            }

            if (pgEx.SqlState == "23505")
            {
                return Error.Conflict("unique.constraint", "Duplicate value");
            }

            return Error.Failure("location.update", "Database error");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting locations ids");

            return Error.Failure("location.get", "Fail to get locations ids");
        }
    }
}