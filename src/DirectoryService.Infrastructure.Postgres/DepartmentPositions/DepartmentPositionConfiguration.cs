using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.DepartmentPositions;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => dp.Id)
            .HasName("pk_department_position");

        builder.Property(dp => dp.Id)
            .HasConversion(
                id => id.Value,
                value => DepartmentPositionId.Create(value))
            .HasColumnName("id");

        builder.HasOne<Department>()
            .WithMany(d => d.Positions)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(e => e.DepartmentId)
            .HasColumnName("department_id");

        builder.HasOne<Position>()
            .WithMany(p => p.Departments)
            .HasForeignKey(d => d.PositionId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(e => e.PositionId)
            .HasColumnName("position_id");
    }
}