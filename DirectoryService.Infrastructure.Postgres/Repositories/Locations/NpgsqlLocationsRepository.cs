using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories.Locations;

public class NpgsqlLocationsRepository : ILocationsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<NpgsqlLocationsRepository> _logger;

    public NpgsqlLocationsRepository(IDbConnectionFactory connectionFactory, ILogger<NpgsqlLocationsRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(Domain.Locations.Locations locations, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertLocationSql = $"""
                                              INSERT INTO locations (id, name, timezone, street, city, country, is_active, created_at, updated_at)
                                              VALUES (@Id, @Name, @Timezone, @Street, @City, @Country, @IsActive, @CreatedAt, @UpdatedAt)
                                              """;

            var insertLocationParams = new
            {
                Id = locations.Id.Value,
                Name = locations.Name.Value,
                Timezone = locations.Timezone.Value,
                Street = locations.Address.Street,
                City = locations.Address.City,
                Country = locations.Address.Country,
                IsActive = locations.IsActive,
                CreatedAt = locations.CreatedAt,
                UpdatedAt = locations.UpdatedAt,
            };

            await connection.ExecuteAsync(insertLocationSql, insertLocationParams);

            transaction.Commit();

            return locations.Id.Value;
        }
        catch (Exception e)
        {
            transaction.Rollback();

            _logger.LogError(e, "Error adding location");

            return Error.Failure("location.insert", "Fail to insert location");
        }
    }
}