using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.DepartmentPositions.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentPositionConfigurations : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(d => d.Id).HasName("pk_department_position");

        builder.Property(d => d.Id)
            .HasConversion(d => d.Value, id => DepartmentPositionsId.FromValue(id))
            .HasColumnName("id");

        builder.Property(d => d.DepartmentId)
            .HasConversion(d => d.Value, id => DepartmentId.FromValue(id))
            .HasColumnName("department_id")
            .IsRequired();

        builder.Property(d => d.PositionId)
            .HasConversion(d => d.Value, id => PositionId.FromValue(id))
            .HasColumnName("position_id")
            .IsRequired();
    }
}