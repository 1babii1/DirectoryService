using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories.Departments;

public class EfCoreDepartmentsRepository : IDepartmentRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<EfCoreDepartmentsRepository> _logger;
    private IDepartmentRepository _departmentRepositoryImplementation;

    public EfCoreDepartmentsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<EfCoreDepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(
        Domain.Departments.Departments department,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Department.AddAsync(department, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return department.Id.Value;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding department");

            return Error.Failure("department.insert", "Fail to insert department");
        }
    }

    public async Task<Result<IEnumerable<DepartmentId>, Error>> GetDepartmentsIds(CancellationToken cancellationToken)
    {
        try
        {
            var allDepartmentIds = await _dbContext.Department
                .Select(d => d.Id)
                .ToListAsync(cancellationToken: cancellationToken);
            return allDepartmentIds;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            _logger.LogError(ex, "Error getting departments ids");

            return Error.Failure("department.get", "Fail to get departments ids");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting departments ids");

            return Error.Failure("department.get", "Fail to get departments ids");
        }
    }
}