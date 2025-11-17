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

        builder.Property(dp => dp.DepartmentId)
            .HasColumnName("department_id");

        builder.Property(dp => dp.PositionId)
            .HasColumnName("position_id");

        builder.HasOne<Department>()
            .WithMany(d => d.Positions)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_department_positions_position_id");

        builder.HasOne<Position>()
            .WithMany()
            .HasForeignKey(d => d.PositionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_department_positions_department_id");
    }
}