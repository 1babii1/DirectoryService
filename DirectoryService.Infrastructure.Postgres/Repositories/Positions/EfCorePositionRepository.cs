using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories.Positions;

public class EfCorePositionRepository : IPositionRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<EfCorePositionRepository> _logger;

    public EfCorePositionRepository(DirectoryServiceDbContext dbContext, ILogger<EfCorePositionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(
        Domain.Positions.Position position,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Position.AddAsync(position, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return position.Id.Value;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding position");

            return Error.Failure("position.insert", "Fail to insert position");
        }
    }
}