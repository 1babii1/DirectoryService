using System.Data;

namespace DirectoryService.Infrastructure.Postgres.Database;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}