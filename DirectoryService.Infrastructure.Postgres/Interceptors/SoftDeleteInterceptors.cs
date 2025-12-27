using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Interceptors;

public class SoftDeleteInterceptors : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        if (eventData.Context == null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entries = eventData.Context?.ChangeTracker.Entries<Departments>()
            .Where(x => x.State == EntityState.Deleted);
        if (entries == null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.Delete();
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}