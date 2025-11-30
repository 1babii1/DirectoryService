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

    public EfCoreDepartmentsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<EfCoreDepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Save()
    {
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Result<IReadOnlyList<Domain.Departments.Departments>, Error>> GetById(
        IReadOnlyList<DepartmentId> departmentIds,
        CancellationToken cancellationToken)
    {
        try
        {
            var departments = await _dbContext.Department
                .Where(d => departmentIds.Contains(d.Id))
                .ToListAsync(cancellationToken: cancellationToken);

            if (departments.Count == 0)
            {
                _logger.LogError("Departments not found");
                return Error.NotFound("departments.get", "Departments not found");
            }

            return Result.Success<IReadOnlyList<Domain.Departments.Departments>, Error>(departments);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            _logger.LogError(ex, "Error getting departments by ids");

            return Error.Failure("departments.get", "Fail to get departments by ids");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting departments by ids");

            return Error.Failure("departments.get", "Fail to get departments by ids");
        }
    }

    public async Task<Result<Domain.Departments.Departments, Error>> GetById(
        DepartmentId departmentIdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var department = await _dbContext.Department
                .Include(d => d.DepartmentsLocationsList)
                .FirstOrDefaultAsync(d => d.Id == departmentIdId, cancellationToken: cancellationToken);
            if (department is null)
            {
                _logger.LogError("Department not found");
                return Error.NotFound("department.get", "Department not found");
            }

            return Result.Success<Domain.Departments.Departments, Error>(department);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            _logger.LogError(ex, "Error getting department by id");

            return Error.Failure("department.get", "Fail to get department by id");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting department by id");

            return Error.Failure("department.get", "Fail to get department by id");
        }
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

    public async Task<Result<IEnumerable<DepartmentId>, Error>> GetDepartmentsIds(
        IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken)
    {
        try
        {
            var allDepartmentIds = await _dbContext.Department
                .Select(d => d.Id)
                .ToListAsync(cancellationToken: cancellationToken);

            var missIds = departmentIds.Except(allDepartmentIds);

            return Result.Success<IEnumerable<DepartmentId>, Error>(missIds);
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