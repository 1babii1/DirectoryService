using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres;

public class DirectoryServiceDbContext : DbContext
{
    private readonly string _connectionString;

    public DirectoryServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
        optionsBuilder.UseLoggerFactory(LoggerFactory);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
    }

    public DbSet<Locations> Location => Set<Locations>();

    public DbSet<Position> Position => Set<Position>();

    public DbSet<Departments> Department => Set<Departments>();

    public DbSet<DepartmentLocation> DepartmentLocation => Set<DepartmentLocation>();

    public DbSet<DepartmentPosition> DepartmentPosition => Set<DepartmentPosition>();

    private ILoggerFactory LoggerFactory =>
        Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
}