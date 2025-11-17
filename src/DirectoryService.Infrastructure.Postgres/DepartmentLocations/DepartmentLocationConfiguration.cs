using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.DepartmentLocations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(dl => dl.Id)
            .HasName("pk_department_locations");

        builder.Property(dl => dl.Id)
            .HasConversion(
                id => id.Value,
                value => DepartmentLocationId.Create(value))
            .HasColumnName("id");

        builder.Property(dl => dl.DepartmentId)
            .HasColumnName("department_id");

        builder.Property(dl => dl.LocationId)
            .HasColumnName("location_id");

        builder.HasOne<Department>()
            .WithMany(d => d.Locations)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_department_locations_location_id");

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(dp => dp.LocationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_department_locations_department_id");
    }
}